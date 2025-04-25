using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SistemaFisica : MonoBehaviour
{
    // Singleton para acceso global
    public static SistemaFisica instancia;

    // Diccionarios para seguimiento de objetos
    private Dictionary<GameObject, Vector2> posiciones = new Dictionary<GameObject, Vector2>();
    private Dictionary<GameObject, Vector2> velocidades = new Dictionary<GameObject, Vector2>();
    private Dictionary<GameObject, float> masas = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> radios = new Dictionary<GameObject, float>();

    // Lista de objetos activos
    private List<GameObject> objetosActivos = new List<GameObject>();

    // Propiedades físicas
    public float coefRestitucion = 0.8f;  // Coeficiente de restitución (e)
    public float coefFriccion = 0.99f;    // Coeficiente de fricción
    public float velocidadMinima = 0.1f;  // Velocidad mínima antes de detenerse

    // Límites del mundo
    public float limiteIzquierdo = -8f;
    public float limiteDerecho = 10f;
    public float limiteSuperior = 5f;
    public float limiteInferior = -5f;

    private void Awake()
    {
        // Configurar singleton
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

    // Registra un objeto para ser controlado por el sistema de física
    public void RegistrarObjeto(GameObject objeto, float masa, float radio, Vector2 velocidadInicial = default)
    {
        if (!posiciones.ContainsKey(objeto))
        {
            posiciones[objeto] = objeto.transform.position;
            velocidades[objeto] = velocidadInicial;
            masas[objeto] = masa;
            radios[objeto] = radio;
            objetosActivos.Add(objeto);
        }
    }

    // Elimina un objeto del sistema de física
    public void EliminarObjeto(GameObject objeto)
    {
        if (posiciones.ContainsKey(objeto))
        {
            posiciones.Remove(objeto);
            velocidades.Remove(objeto);
            masas.Remove(objeto);
            radios.Remove(objeto);
            objetosActivos.Remove(objeto);
        }
    }

    // Aplica una fuerza (velocidad instantánea) a un objeto
    public void AplicarFuerza(GameObject objeto, Vector2 fuerza)
    {
        if (velocidades.ContainsKey(objeto))
        {
            velocidades[objeto] += fuerza;
        }
    }

    // Actualiza la física de todos los objetos
    private void ActualizarFisica()
    {
        for (int i = objetosActivos.Count - 1; i >= 0; i--)
        {
            var obj = objetosActivos[i];
            if (obj == null) { objetosActivos.RemoveAt(i); continue; }

            Vector2 pos = posiciones[obj];
            Vector2 vel = velocidades[obj];

            pos += vel * Time.fixedDeltaTime;
            vel *= coefFriccion;
            if (vel.magnitude < velocidadMinima) vel = Vector2.zero;

            // Detectar rebote en límites y notificar
            bool rebotado = ComprobarColisionesConLimites(ref pos, ref vel, obj);
            if (rebotado)
            {
                var col = obj.GetComponent<MonoBehaviour>() as IObjetoColisionable;
                if (col != null)
                {
                    col.OnColision(null);  // null indica rebote contra límite
                }
            }

            posiciones[obj] = pos;
            velocidades[obj] = vel;
            obj.transform.position = new Vector3(pos.x, pos.y, obj.transform.position.z);
        }

        ComprobarColisionesEntreObjetos();
    }

    // Comprueba y resuelve colisiones con los límites del mundo
    private bool ComprobarColisionesConLimites(ref Vector2 pos, ref Vector2 vel, GameObject obj)
    {
        bool rebotado = false;
        if (pos.x < limiteIzquierdo) { pos.x = limiteIzquierdo; vel.x = -vel.x * coefRestitucion; rebotado = true; }
        else if (pos.x > limiteDerecho) { pos.x = limiteDerecho; vel.x = -vel.x * coefRestitucion; rebotado = true; }
        if (pos.y < limiteInferior) { pos.y = limiteInferior; vel.y = -vel.y * coefRestitucion; rebotado = true; }
        else if (pos.y > limiteSuperior) { pos.y = limiteSuperior; vel.y = -vel.y * coefRestitucion; rebotado = true; }
        return rebotado;
    }

    // Comprueba y resuelve colisiones entre todos los objetos
    private void ComprobarColisionesEntreObjetos()
    {
        // Comprobar colisiones entre cada par de objetos
        for (int i = 0; i < objetosActivos.Count; i++)
        {
            for (int j = i + 1; j < objetosActivos.Count; j++)
            {
                GameObject obj1 = objetosActivos[i];
                GameObject obj2 = objetosActivos[j];
                
                // Verificar que ambos objetos existan
                if (obj1 == null || obj2 == null) continue;
                
                // Obtener posiciones y radios
                Vector2 pos1 = posiciones[obj1];
                Vector2 pos2 = posiciones[obj2];
                float radio1 = radios[obj1];
                float radio2 = radios[obj2];
                
                // Calcular distancia
                float distancia = Vector2.Distance(pos1, pos2);
                float distanciaColision = radio1 + radio2;
                
                // Verificar colisión
                if (distancia <= distanciaColision)
                {
                    // Hay colisión, resolver
                    ResolverColision(obj1, obj2);
                    
                    // Notificar a los objetos sobre la colisión
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

    // Resuelve la colisión entre dos objetos usando las fórmulas de física
    private void ResolverColision(GameObject obj1, GameObject obj2)
    {
        // Obtener velocidades y masas
        Vector2 vel1 = velocidades[obj1];
        Vector2 vel2 = velocidades[obj2];
        float m1 = masas[obj1];
        float m2 = masas[obj2];
        
        // Calcular nuevas velocidades según fórmulas de colisiones elásticas
        // Para el objeto 1
        float vx1Nueva = ((m1 - coefRestitucion * m2) / (m1 + m2)) * vel1.x +
                        ((1 + coefRestitucion) * m2 / (m1 + m2)) * vel2.x;
            
        float vy1Nueva = ((m1 - m2) / (m1 + m2)) * vel1.y +
                        (2 * m2 / (m1 + m2)) * vel2.y;
        
        // Para el objeto 2
        float vx2Nueva = (((1 + coefRestitucion) * m1) / (m1 + m2)) * vel1.x +
                        ((m2 - coefRestitucion * m1) / (m1 + m2)) * vel2.x;
            
        float vy2Nueva = ((2 * m1) / (m1 + m2)) * vel1.y +
                        ((m2 - m1) / (m1 + m2)) * vel2.y;
        
        // Actualizar velocidades
        velocidades[obj1] = new Vector2(vx1Nueva, vy1Nueva);
        velocidades[obj2] = new Vector2(vx2Nueva, vy2Nueva);
        
        // Separar objetos para evitar que queden superpuestos
        SepararObjetos(obj1, obj2);
    }

    // Separa objetos para evitar superposición continua
    private void SepararObjetos(GameObject obj1, GameObject obj2)
    {
        Vector2 pos1 = posiciones[obj1];
        Vector2 pos2 = posiciones[obj2];
        
        Vector2 direccion = (pos1 - pos2).normalized;
        float radio1 = radios[obj1];
        float radio2 = radios[obj2];
        
        // Calcular la superposición
        float distancia = Vector2.Distance(pos1, pos2);
        float superposicion = (radio1 + radio2) - distancia;
        
        if (superposicion > 0)
        {
            // Ajustar posiciones para evitar superposición
            float m1 = masas[obj1];
            float m2 = masas[obj2];
            float sumaMasas = m1 + m2;
            
            // Mover cada objeto en proporción inversa a su masa
            posiciones[obj1] = pos1 + (direccion * superposicion * (m2 / sumaMasas));
            posiciones[obj2] = pos2 - (direccion * superposicion * (m1 / sumaMasas));
            
            // Actualizar posiciones visuales
            obj1.transform.position = new Vector3(posiciones[obj1].x, posiciones[obj1].y, obj1.transform.position.z);
            obj2.transform.position = new Vector3(posiciones[obj2].x, posiciones[obj2].y, obj2.transform.position.z);
        }
    }

    // Método para visualización en el editor
    void OnDrawGizmos()
    {
        // Dibujar límites del mundo
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(limiteIzquierdo, limiteSuperior), new Vector3(limiteDerecho, limiteSuperior));
        Gizmos.DrawLine(new Vector3(limiteDerecho, limiteSuperior), new Vector3(limiteDerecho, limiteInferior));
        Gizmos.DrawLine(new Vector3(limiteDerecho, limiteInferior), new Vector3(limiteIzquierdo, limiteInferior));
        Gizmos.DrawLine(new Vector3(limiteIzquierdo, limiteInferior), new Vector3(limiteIzquierdo, limiteSuperior));
        
        // Dibujar objetos en el sistema de física
        if (Application.isPlaying)
        {
            foreach (var objeto in objetosActivos)
            {
                if (objeto != null)
                {
                    Gizmos.color = Color.green;
                    Vector2 pos = posiciones[objeto];
                    float radio = radios[objeto];
                    Gizmos.DrawWireSphere(new Vector3(pos.x, pos.y, 0), radio);
                    
                    // Dibujar vector de velocidad
                    Gizmos.color = Color.red;
                    Vector2 vel = velocidades[objeto];
                    Gizmos.DrawLine(new Vector3(pos.x, pos.y, 0), new Vector3(pos.x + vel.x, pos.y + vel.y, 0));
                }
            }
        }
    }
}