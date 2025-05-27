using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Meta : MonoBehaviour
{
    // Referencia opcional al texto para mostrar el conteo
    public TextMeshProUGUI textoContador;
    
    // Dimensiones de la meta
    public float ancho = 2f;
    public float alto = 0.5f;
    
    // Conteo de bolas que han cruzado la meta
    private int conteo = 0;
    
    // Para controlar que no se cuente múltiples veces la misma bola
    private HashSet<GameObject> bolasRegistradas = new HashSet<GameObject>();
    
    void Start()
    {
        // Registrar la meta en el sistema de física
        SistemaFisica.instancia.RegistrarMeta(this);
        
        // Actualizar el texto inicial
        ActualizarTextoContador();
    }

    void OnDestroy()
    {
        // Eliminar la meta del sistema de física cuando se destruya
        if (SistemaFisica.instancia != null)
            SistemaFisica.instancia.EliminarMeta(this);
    }
    
    // Método llamado por el sistema de física cuando una bola cruza la meta
    public void ContabilizarBola(GameObject bola)
    {
        if (!bolasRegistradas.Contains(bola))
        {
            // Incrementar el conteo
            conteo++;
            
            // Registrar la bola para no contarla nuevamente
            bolasRegistradas.Add(bola);
            
            // Actualizar el texto del contador
            ActualizarTextoContador();
            
            Debug.Log($"¡Gol! Conteo actual: {conteo}");
            
            // Notificar al GestorJuego
            AdminJuego.instancia.BolaMetaAlcanzada(bola);
        }
    }
    
    // Método para actualizar el texto del contador
    private void ActualizarTextoContador()
    {
        if (textoContador != null)
        {
            textoContador.text = $"Goles: {conteo}";
        }
    }
    
    // Obtener los límites de la meta en coordenadas del mundo
    public Bounds ObtenerLimites()
    {
        Vector3 centro = transform.position;
        Vector3 tamaño = new Vector3(ancho, alto, 0.1f);
        return new Bounds(centro, tamaño);
    }
    
    // Método para visualización en el editor
    void OnDrawGizmos()
    {
        // Dibujar un rectángulo que represente la meta
        Gizmos.color = Color.green;
        Vector3 centro = transform.position;
        Vector3 tamaño = new Vector3(ancho, alto, 0.1f);
        Gizmos.DrawWireCube(centro, tamaño);
    }
    
    // Método para obtener el conteo actual (útil para otros scripts)
    public int ObtenerConteo()
    {
        return conteo;
    }
    
    // Método para resetear el conteo
    public void ResetearConteo()
    {
        conteo = 0;
        bolasRegistradas.Clear();
        ActualizarTextoContador();
        Debug.Log("Conteo de meta reiniciado");
    }
}
