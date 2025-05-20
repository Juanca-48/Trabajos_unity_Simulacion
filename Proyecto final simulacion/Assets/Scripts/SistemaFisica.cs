using System.Collections.Generic;
using UnityEngine;

public class SistemaFisica : MonoBehaviour
{
    public static SistemaFisica instancia;

    private class DatosObjeto
    {
        public Vector2 posicion;
        public Vector2 velocidad;
        public float masa;
        public float radio;
        public bool afectadoPorGravedad = false;
        public float coefArrastre = 0.2f;  // Coeficiente de arrastre para el objeto
    }

    private Dictionary<GameObject, DatosObjeto> objetosFisicos = new Dictionary<GameObject, DatosObjeto>();
    private List<GameObject> objetosActivos = new List<GameObject>();
    private List<Meta> metas = new List<Meta>();

    public float coefRestitucion = 0.8f;
    public float coefFriccion = 0.2f;
    public float velocidadMinima = 0.1f;
    public float gravedad = 9.81f;  // Valor de la gravedad

    public float limiteIzquierdo = -8f;
    public float limiteDerecho = 8f;
    public float limiteSuperior = 4.5f;
    public float limiteInferior = -4.5f;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        ActualizarFisica();
    }

    public void RegistrarObjeto(GameObject objeto, float masa, float radio, Vector2 velocidadInicial = default)
    {
        if (!objetosFisicos.ContainsKey(objeto))
        {
            objetosFisicos[objeto] = new DatosObjeto
            {
                posicion = objeto.transform.position,
                velocidad = velocidadInicial,
                masa = masa,
                radio = radio
            };
            objetosActivos.Add(objeto);
        }
    }

    public void EliminarObjeto(GameObject objeto)
    {
        if (objetosFisicos.ContainsKey(objeto))
        {
            objetosFisicos.Remove(objeto);
            objetosActivos.Remove(objeto);
        }
    }

    public void AplicarFuerza(GameObject objeto, Vector2 fuerza)
    {
        if (objetosFisicos.TryGetValue(objeto, out DatosObjeto datos))
        {
            datos.velocidad += fuerza;
        }
    }

    // Método para activar la gravedad en un objeto
    public void ActivarGravedad(GameObject objeto, bool activar, float coefArrastre = 0.2f)
    {
        if (objetosFisicos.TryGetValue(objeto, out DatosObjeto datos))
        {
            datos.afectadoPorGravedad = activar;
            datos.coefArrastre = coefArrastre;
            Debug.Log($"Gravedad {(activar ? "activada" : "desactivada")} para {objeto.name}");
        }
    }

    // Método para comprobar si un objeto está afectado por la gravedad
    public bool EstaBajoGravedad(GameObject objeto)
    {
        if (objetosFisicos.TryGetValue(objeto, out DatosObjeto datos))
        {
            return datos.afectadoPorGravedad;
        }
        return false;
    }

    private void ActualizarFisica()
    {
        for (int i = objetosActivos.Count - 1; i >= 0; i--)
        {
            if (objetosActivos[i] == null)
            {
                objetosFisicos.Remove(objetosActivos[i]);
                objetosActivos.RemoveAt(i);
            }
        }

        foreach (var obj in objetosActivos)
        {
            var datos = objetosFisicos[obj];
            
            // Aplicar gravedad y resistencia del aire si está activa
            if (datos.afectadoPorGravedad)
            {
                float ax = -(datos.coefArrastre / datos.masa) * datos.velocidad.x;
                float ay = -gravedad - (datos.coefArrastre / datos.masa) * datos.velocidad.y;
                
                datos.velocidad.x += ax * Time.fixedDeltaTime;
                datos.velocidad.y += ay * Time.fixedDeltaTime;
            }
            else
            {
                // Comportamiento normal - aplicar fricción
                datos.velocidad *= coefFriccion;
            }
            
            // Actualizar posición
            datos.posicion += datos.velocidad * Time.fixedDeltaTime;
            
            // Comprobar velocidad mínima solo para objetos no afectados por gravedad
            if (!datos.afectadoPorGravedad && datos.velocidad.magnitude < velocidadMinima)
                datos.velocidad = Vector2.zero;

            bool rebotado = ComprobarColisionesConLimites(ref datos.posicion, ref datos.velocidad, obj);
            if (rebotado)
            {
                var col = obj.GetComponent<MonoBehaviour>() as IObjetoColisionable;
                if (col != null)
                {
                    col.OnColision(null);
                }
            }

            obj.transform.position = new Vector3(datos.posicion.x, datos.posicion.y, obj.transform.position.z);
        }

        ComprobarColisionesEntreObjetos();
        ComprobarCrucesMeta();
    }

    private bool ComprobarColisionesConLimites(ref Vector2 pos, ref Vector2 vel, GameObject obj)
    {
        bool rebotado = false;

        if (pos.x < limiteIzquierdo) { pos.x = limiteIzquierdo; vel.x = -vel.x * coefRestitucion; rebotado = true; }
        else if (pos.x > limiteDerecho) { pos.x = limiteDerecho; vel.x = -vel.x * coefRestitucion; rebotado = true; }

        if (pos.y < limiteInferior) { pos.y = limiteInferior; vel.y = -vel.y * coefRestitucion; rebotado = true; }
        else if (pos.y > limiteSuperior) { pos.y = limiteSuperior; vel.y = -vel.y * coefRestitucion; rebotado = true; }

        return rebotado;
    }

    private void ComprobarColisionesEntreObjetos()
    {
        for (int i = 0; i < objetosActivos.Count; i++)
        {
            for (int j = i + 1; j < objetosActivos.Count; j++)
            {
                GameObject obj1 = objetosActivos[i];
                GameObject obj2 = objetosActivos[j];

                if (obj1 == null || obj2 == null) continue;

                var datos1 = objetosFisicos[obj1];
                var datos2 = objetosFisicos[obj2];

                float distancia = Vector2.Distance(datos1.posicion, datos2.posicion);
                float distanciaColision = datos1.radio + datos2.radio;

                if (distancia <= distanciaColision)
                {
                    Colision(obj1, obj2);

                    var colisionable1 = obj1.GetComponent<MonoBehaviour>() as IObjetoColisionable;
                    var colisionable2 = obj2.GetComponent<MonoBehaviour>() as IObjetoColisionable;

                    colisionable1?.OnColision(obj2);
                    colisionable2?.OnColision(obj1);
                }
            }
        }
    }

    private void Colision(GameObject obj1, GameObject obj2)
    {
        var datos1 = objetosFisicos[obj1];
        var datos2 = objetosFisicos[obj2];

        Vector2 vel1 = datos1.velocidad;
        Vector2 vel2 = datos2.velocidad;

        float m1 = datos1.masa;
        float m2 = datos2.masa;

        float vx1Nueva = ((m1 - coefRestitucion * m2) / (m1 + m2)) * vel1.x + ((1 + coefRestitucion) * m2 / (m1 + m2)) * vel2.x;
        float vy1Nueva = ((m1 - m2) / (m1 + m2)) * vel1.y + (2 * m2 / (m1 + m2)) * vel2.y;

        float vx2Nueva = ((1 + coefRestitucion) * m1 / (m1 + m2)) * vel1.x + ((m2 - coefRestitucion * m1) / (m1 + m2)) * vel2.x;
        float vy2Nueva = ((2 * m1) / (m1 + m2)) * vel1.y + ((m2 - m1) / (m1 + m2)) * vel2.y;

        datos1.velocidad = new Vector2(vx1Nueva, vy1Nueva);
        datos2.velocidad = new Vector2(vx2Nueva, vy2Nueva);
    }

    private void ComprobarCrucesMeta()
    {
        foreach (var obj in objetosActivos)
        {
            if (obj == null || obj.GetComponent<Bola>() == null) continue;

            if (objetosFisicos.TryGetValue(obj, out DatosObjeto datos))
            {
                Vector2 posicion = datos.posicion;

                foreach (var meta in metas)
                {
                    Bounds limitesMeta = meta.ObtenerLimites();

                    if (limitesMeta.Contains(new Vector3(posicion.x, posicion.y, 0)))
                    {
                        meta.ContabilizarBola(obj);
                    }
                }
            }
        }
    }

    public void RegistrarMeta(Meta meta)
    {
        if (!metas.Contains(meta))
        {
            metas.Add(meta);
        }
    }

    public void EliminarMeta(Meta meta)
    {
        metas.Remove(meta);
    }
}