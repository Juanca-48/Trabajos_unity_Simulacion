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
    }
    private Dictionary<GameObject, DatosObjeto> objetosFisicos = new Dictionary<GameObject, DatosObjeto>();
    private List<GameObject> objetosActivos = new List<GameObject>();

    // Propiedades físicas
    public float coefRestitucion = 0.8f;
    public float coefFriccion = 0.2f; 
    public float velocidadMinima = 0.1f;
    // Límites del mundo
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

        // Actualizar posición y velocidad de cada objeto
        foreach (var obj in objetosActivos)
        {
            var datos = objetosFisicos[obj];
            datos.posicion += datos.velocidad * Time.fixedDeltaTime;
            datos.velocidad *= coefFriccion;
            if (datos.velocidad.magnitude < velocidadMinima)
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
                    
                    if (colisionable1 != null)
                    {
                        colisionable1.OnColision(obj2);
                    }
                    
                    if (colisionable2 != null)
                    {
                        colisionable2.OnColision(obj1);
                    }
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
        float vx1Nueva = ((m1 - coefRestitucion * m2) / (m1 + m2)) * vel1.x +
                      ((1 + coefRestitucion) * m2 / (m1 + m2)) * vel2.x;
            
        float vy1Nueva = ((m1 - m2) / (m1 + m2)) * vel1.y +
                      (2 * m2 / (m1 + m2)) * vel2.y;
        
        float vx2Nueva = (((1 + coefRestitucion) * m1) / (m1 + m2)) * vel1.x +
                      ((m2 - coefRestitucion * m1) / (m1 + m2)) * vel2.x;
            
        float vy2Nueva = ((2 * m1) / (m1 + m2)) * vel1.y +
                      ((m2 - m1) / (m1 + m2)) * vel2.y;
        datos1.velocidad = new Vector2(vx1Nueva, vy1Nueva);
        datos2.velocidad = new Vector2(vx2Nueva, vy2Nueva);
        SepararObjetos(obj1, obj2);
    }
    private void SepararObjetos(GameObject obj1, GameObject obj2)
    {
        var datos1 = objetosFisicos[obj1];
        var datos2 = objetosFisicos[obj2];
        
        Vector2 direccion = (datos1.posicion - datos2.posicion).normalized;
        float radio1 = datos1.radio;
        float radio2 = datos2.radio;
        
        float distancia = Vector2.Distance(datos1.posicion, datos2.posicion);
        float superposicion = (radio1 + radio2) - distancia;
        
        if (superposicion > 0)
        {
            float m1 = datos1.masa;
            float m2 = datos2.masa;
            float sumaMasas = m1 + m2;
            datos1.posicion += (direccion * superposicion * (m2 / sumaMasas));
            datos2.posicion -= (direccion * superposicion * (m1 / sumaMasas));
        }
    }

    // Método para visualización en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(limiteIzquierdo, limiteSuperior), new Vector3(limiteDerecho, limiteSuperior));
        Gizmos.DrawLine(new Vector3(limiteDerecho, limiteSuperior), new Vector3(limiteDerecho, limiteInferior));
        Gizmos.DrawLine(new Vector3(limiteDerecho, limiteInferior), new Vector3(limiteIzquierdo, limiteInferior));
        Gizmos.DrawLine(new Vector3(limiteIzquierdo, limiteInferior), new Vector3(limiteIzquierdo, limiteSuperior));
    }
}