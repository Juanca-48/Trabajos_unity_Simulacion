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
        public float coefArrastre = 0.2f;
        // Nuevos campos para manejar diferentes formas
        public bool esRectangular = false;
        public Vector2 tamanoRectangulo = Vector2.zero;
    }

    private Dictionary<GameObject, DatosObjeto> objetosFisicos = new Dictionary<GameObject, DatosObjeto>();
    private List<GameObject> objetosActivos = new List<GameObject>();
    private List<Meta> metas = new List<Meta>();

    public float coefRestitucion = 0.8f;
    public float coefFriccion = 0.2f;
    public float velocidadMinima = 0.1f;
    public float gravedad = 9.81f;

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

    // Método original para objetos circulares
    public void RegistrarObjeto(GameObject objeto, float masa, float radio, Vector2 velocidadInicial = default)
    {
        if (!objetosFisicos.ContainsKey(objeto))
        {
            objetosFisicos[objeto] = new DatosObjeto
            {
                posicion = objeto.transform.position,
                velocidad = velocidadInicial,
                masa = masa,
                radio = radio,
                esRectangular = false
            };
            objetosActivos.Add(objeto);
        }
    }

    // Nuevo método para registrar objetos rectangulares
    public void RegistrarObjetoRectangular(GameObject objeto, float masa, Vector2 tamanoRectangulo, Vector2 velocidadInicial = default)
    {
        if (!objetosFisicos.ContainsKey(objeto))
        {
            // Calcular radio equivalente para optimizaciones de distancia aproximada
            float radioEquivalente = Vector2.Distance(Vector2.zero, tamanoRectangulo) / 2f;
            
            objetosFisicos[objeto] = new DatosObjeto
            {
                posicion = objeto.transform.position,
                velocidad = velocidadInicial,
                masa = masa,
                radio = radioEquivalente, // Para optimización inicial
                esRectangular = true,
                tamanoRectangulo = tamanoRectangulo
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

    public void ActivarGravedad(GameObject objeto, bool activar, float coefArrastre = 0.2f)
    {
        if (objetosFisicos.TryGetValue(objeto, out DatosObjeto datos))
        {
            datos.afectadoPorGravedad = activar;
            datos.coefArrastre = coefArrastre;
            Debug.Log($"Gravedad {(activar ? "activada" : "desactivada")} para {objeto.name}");
        }
    }

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
            
            if (datos.afectadoPorGravedad)
            {
                float ax = -(datos.coefArrastre / datos.masa) * datos.velocidad.x;
                float ay = -gravedad - (datos.coefArrastre / datos.masa) * datos.velocidad.y;
                
                datos.velocidad.x += ax * Time.fixedDeltaTime;
                datos.velocidad.y += ay * Time.fixedDeltaTime;
            }
            else
            {
                datos.velocidad *= coefFriccion;
            }
            
            datos.posicion += datos.velocidad * Time.fixedDeltaTime;
            
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
        var datos = objetosFisicos[obj];

        if (datos.esRectangular)
        {
            // Para rectángulos, usar los bordes reales del rectángulo
            Vector2 mitadTamano = datos.tamanoRectangulo / 2f;
            
            if (pos.x - mitadTamano.x < limiteIzquierdo) 
            { 
                pos.x = limiteIzquierdo + mitadTamano.x; 
                vel.x = -vel.x * coefRestitucion; 
                rebotado = true; 
            }
            else if (pos.x + mitadTamano.x > limiteDerecho) 
            { 
                pos.x = limiteDerecho - mitadTamano.x; 
                vel.x = -vel.x * coefRestitucion; 
                rebotado = true; 
            }

            if (pos.y - mitadTamano.y < limiteInferior) 
            { 
                pos.y = limiteInferior + mitadTamano.y; 
                vel.y = -vel.y * coefRestitucion; 
                rebotado = true; 
            }
            else if (pos.y + mitadTamano.y > limiteSuperior) 
            { 
                pos.y = limiteSuperior - mitadTamano.y; 
                vel.y = -vel.y * coefRestitucion; 
                rebotado = true; 
            }
        }
        else
        {
            // Para círculos, usar el radio
            if (pos.x - datos.radio < limiteIzquierdo) 
            { 
                pos.x = limiteIzquierdo + datos.radio; 
                vel.x = -vel.x * coefRestitucion; 
                rebotado = true; 
            }
            else if (pos.x + datos.radio > limiteDerecho) 
            { 
                pos.x = limiteDerecho - datos.radio; 
                vel.x = -vel.x * coefRestitucion; 
                rebotado = true; 
            }

            if (pos.y - datos.radio < limiteInferior) 
            { 
                pos.y = limiteInferior + datos.radio; 
                vel.y = -vel.y * coefRestitucion; 
                rebotado = true; 
            }
            else if (pos.y + datos.radio > limiteSuperior) 
            { 
                pos.y = limiteSuperior - datos.radio; 
                vel.y = -vel.y * coefRestitucion; 
                rebotado = true; 
            }
        }

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

                // NUEVA VERIFICACIÓN: Evitar colisiones entre Bola y PowerUp
                if (EsColisionBolaConPowerUp(obj1, obj2))
                {
                    continue; // Saltarse esta colisión
                }

                var datos1 = objetosFisicos[obj1];
                var datos2 = objetosFisicos[obj2];

                // Verificar colisión según las formas de los objetos
                bool hayColision = false;
                Vector2 puntoContacto = Vector2.zero;
                Vector2 normalColision = Vector2.zero;

                if (!datos1.esRectangular && !datos2.esRectangular)
                {
                    // Colisión círculo vs círculo
                    hayColision = ColisionCirculoVsCirculo(datos1, datos2, out puntoContacto, out normalColision);
                }
                else if (datos1.esRectangular && !datos2.esRectangular)
                {
                    // Colisión rectángulo vs círculo
                    hayColision = ColisionRectanguloVsCirculo(datos1, datos2, out puntoContacto, out normalColision);
                }
                else if (!datos1.esRectangular && datos2.esRectangular)
                {
                    // Colisión círculo vs rectángulo
                    hayColision = ColisionRectanguloVsCirculo(datos2, datos1, out puntoContacto, out normalColision);
                    normalColision = -normalColision; // Invertir normal
                }
                else
                {
                    // Colisión rectángulo vs rectángulo
                    hayColision = ColisionRectanguloVsRectangulo(datos1, datos2, out puntoContacto, out normalColision);
                }

                if (hayColision)
                {
                    bool esPowerUpVsProyectil = EsColisionPowerUpConProyectil(obj1, obj2);
                    
                    if (esPowerUpVsProyectil)
                    {
                        ManejarColisionPowerUpProyectil(obj1, obj2);
                    }
                    else
                    {
                        ColisionConSeparacion(obj1, obj2, normalColision);
                        
                        var colisionable1 = obj1 != null ? obj1.GetComponent<MonoBehaviour>() as IObjetoColisionable : null;
                        var colisionable2 = obj2 != null ? obj2.GetComponent<MonoBehaviour>() as IObjetoColisionable : null;

                        colisionable1?.OnColision(obj2);
                        colisionable2?.OnColision(obj1);
                    }
                }
            }
        }
    }

    // NUEVO MÉTODO: Verificar si es una colisión entre Bola y PowerUp
    private bool EsColisionBolaConPowerUp(GameObject obj1, GameObject obj2)
    {
        return (EsBola(obj1) && EsPowerUp(obj2)) || 
               (EsBola(obj2) && EsPowerUp(obj1));
    }

    // NUEVO MÉTODO: Verificar si un objeto es una Bola
    private bool EsBola(GameObject obj)
    {
        return obj.GetComponent<Bola>() != null;
    }

    private bool ColisionCirculoVsCirculo(DatosObjeto datos1, DatosObjeto datos2, out Vector2 puntoContacto, out Vector2 normalColision)
    {
        float distancia = Vector2.Distance(datos1.posicion, datos2.posicion);
        float distanciaColision = datos1.radio + datos2.radio;
        
        puntoContacto = (datos1.posicion + datos2.posicion) / 2f;
        normalColision = (datos2.posicion - datos1.posicion).normalized;
        
        return distancia <= distanciaColision;
    }

    private bool ColisionRectanguloVsCirculo(DatosObjeto datosRect, DatosObjeto datosCirc, out Vector2 puntoContacto, out Vector2 normalColision)
    {
        Vector2 posRect = datosRect.posicion;
        Vector2 posCirc = datosCirc.posicion;
        Vector2 mitadTamano = datosRect.tamanoRectangulo / 2f;
        
        // Calcular límites del rectángulo
        Vector2 minRect = posRect - mitadTamano;
        Vector2 maxRect = posRect + mitadTamano;
        
        // Encontrar el punto más cercano del rectángulo al círculo
        Vector2 puntoMasCercano = new Vector2(
            Mathf.Clamp(posCirc.x, minRect.x, maxRect.x),
            Mathf.Clamp(posCirc.y, minRect.y, maxRect.y)
        );
        
        float distancia = Vector2.Distance(posCirc, puntoMasCercano);
        puntoContacto = puntoMasCercano;
        
        if (distancia <= datosCirc.radio)
        {
            if (distancia > 0)
            {
                normalColision = (posCirc - puntoMasCercano).normalized;
            }
            else
            {
                // El círculo está exactamente en el punto más cercano, calcular normal basada en la posición relativa
                Vector2 diferencia = posCirc - posRect;
                if (Mathf.Abs(diferencia.x) > Mathf.Abs(diferencia.y))
                {
                    normalColision = new Vector2(Mathf.Sign(diferencia.x), 0);
                }
                else
                {
                    normalColision = new Vector2(0, Mathf.Sign(diferencia.y));
                }
            }
            return true;
        }
        
        normalColision = Vector2.zero;
        return false;
    }

    private bool ColisionRectanguloVsRectangulo(DatosObjeto datos1, DatosObjeto datos2, out Vector2 puntoContacto, out Vector2 normalColision)
    {
        Vector2 pos1 = datos1.posicion;
        Vector2 pos2 = datos2.posicion;
        Vector2 mitad1 = datos1.tamanoRectangulo / 2f;
        Vector2 mitad2 = datos2.tamanoRectangulo / 2f;
        
        // Calcular solapamiento en cada eje
        float solapamientoX = (mitad1.x + mitad2.x) - Mathf.Abs(pos1.x - pos2.x);
        float solapamientoY = (mitad1.y + mitad2.y) - Mathf.Abs(pos1.y - pos2.y);
        
        puntoContacto = (pos1 + pos2) / 2f;
        
        if (solapamientoX > 0 && solapamientoY > 0)
        {
            // Hay colisión, determinar la normal basada en el menor solapamiento
            if (solapamientoX < solapamientoY)
            {
                normalColision = new Vector2(Mathf.Sign(pos2.x - pos1.x), 0);
            }
            else
            {
                normalColision = new Vector2(0, Mathf.Sign(pos2.y - pos1.y));
            }
            return true;
        }
        
        normalColision = Vector2.zero;
        return false;
    }

    private void ColisionConSeparacion(GameObject obj1, GameObject obj2, Vector2 normalColision)
    {
        var datos1 = objetosFisicos[obj1];
        var datos2 = objetosFisicos[obj2];

        // Separar objetos para evitar que se queden pegados
        float penetracion = CalcularPenetracion(datos1, datos2);
        if (penetracion > 0)
        {
            Vector2 separacion = normalColision * (penetracion / 2f + 0.01f); // Pequeño buffer adicional
            
            // Solo mover objetos que no sean estáticos (masa muy alta)
            if (datos1.masa < 100f) // Objetos dinámicos
            {
                datos1.posicion -= separacion;
            }
            if (datos2.masa < 100f) // Objetos dinámicos
            {
                datos2.posicion += separacion;
            }
        }

        // Aplicar colisión de velocidades
        Colision(obj1, obj2);
    }

    private float CalcularPenetracion(DatosObjeto datos1, DatosObjeto datos2)
    {
        if (!datos1.esRectangular && !datos2.esRectangular)
        {
            // Círculo vs círculo
            float distancia = Vector2.Distance(datos1.posicion, datos2.posicion);
            return (datos1.radio + datos2.radio) - distancia;
        }
        else if (datos1.esRectangular && !datos2.esRectangular)
        {
            // Rectángulo vs círculo
            Vector2 puntoMasCercano = CalcularPuntoMasCercanoRectanguloACirculo(datos1, datos2.posicion);
            float distancia = Vector2.Distance(datos2.posicion, puntoMasCercano);
            return datos2.radio - distancia;
        }
        else if (!datos1.esRectangular && datos2.esRectangular)
        {
            // Círculo vs rectángulo
            Vector2 puntoMasCercano = CalcularPuntoMasCercanoRectanguloACirculo(datos2, datos1.posicion);
            float distancia = Vector2.Distance(datos1.posicion, puntoMasCercano);
            return datos1.radio - distancia;
        }
        else
        {
            // Rectángulo vs rectángulo
            Vector2 diferencia = datos2.posicion - datos1.posicion;
            Vector2 solapamiento = (datos1.tamanoRectangulo + datos2.tamanoRectangulo) / 2f - new Vector2(Mathf.Abs(diferencia.x), Mathf.Abs(diferencia.y));
            return Mathf.Min(solapamiento.x, solapamiento.y);
        }
    }

    private Vector2 CalcularPuntoMasCercanoRectanguloACirculo(DatosObjeto datosRect, Vector2 posCirculo)
    {
        Vector2 mitadTamano = datosRect.tamanoRectangulo / 2f;
        Vector2 minRect = datosRect.posicion - mitadTamano;
        Vector2 maxRect = datosRect.posicion + mitadTamano;
        
        return new Vector2(
            Mathf.Clamp(posCirculo.x, minRect.x, maxRect.x),
            Mathf.Clamp(posCirculo.y, minRect.y, maxRect.y)
        );
    }

    private bool EsColisionPowerUpConProyectil(GameObject obj1, GameObject obj2)
    {
        return (EsPowerUp(obj1) && EsProyectil(obj2)) || 
               (EsPowerUp(obj2) && EsProyectil(obj1));
    }

    private bool EsPowerUp(GameObject obj)
    {
        if (obj.CompareTag("Aumento")) return true;
        if (obj.GetComponent<PowerUpItem>() != null) return true;
        if (obj.GetComponent<PowerUpItem1>() != null) return true;
        if (obj.GetComponent<PowerUpSprite>() != null) return true;
        return false;
    }

    private bool EsProyectil(GameObject obj)
    {
        return obj.CompareTag("Proyectil") || obj.GetComponent<Proyectil>() != null;
    }

    private void ManejarColisionPowerUpProyectil(GameObject obj1, GameObject obj2)
    {
        GameObject powerUp = null;
        GameObject proyectil = null;
        
        if (EsPowerUp(obj1) && EsProyectil(obj2))
        {
            powerUp = obj1;
            proyectil = obj2;
        }
        else if (EsPowerUp(obj2) && EsProyectil(obj1))
        {
            powerUp = obj2;
            proyectil = obj1;
        }
        
        if (powerUp != null && proyectil != null)
        {
            Debug.Log($"[SistemaFisica] Colisión power-up detectada: {powerUp.name} con {proyectil.name}");
            
            Vector2 velocidadOriginalProyectil = Vector2.zero;
            if (objetosFisicos.TryGetValue(proyectil, out DatosObjeto datosProyectil))
            {
                velocidadOriginalProyectil = datosProyectil.velocidad;
            }
            
            var colisionablePowerUp = powerUp.GetComponent<MonoBehaviour>() as IObjetoColisionable;
            colisionablePowerUp?.OnColision(proyectil);
            
            if (objetosFisicos.TryGetValue(proyectil, out DatosObjeto datosProyectilDespues))
            {
                datosProyectilDespues.velocidad = velocidadOriginalProyectil;
                Debug.Log($"[SistemaFisica] Velocidad del proyectil {proyectil.name} preservada: {velocidadOriginalProyectil}");
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