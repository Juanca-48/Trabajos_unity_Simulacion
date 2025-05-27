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
    
    // Identificación del jugador (opcional)
    [Header("Configuración del Jugador")]
    public string nombreJugador = "Jugador 1";
    
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
        // Verificar si el juego ya terminó
        if (AdminJuego.instancia.EstaJuegoTerminado())
            return;
        
        if (!bolasRegistradas.Contains(bola))
        {
            // Incrementar el conteo
            conteo++;
            
            // Registrar la bola para no contarla nuevamente
            bolasRegistradas.Add(bola);
            
            // Actualizar el texto del contador
            ActualizarTextoContador();
            
            Debug.Log($"¡Gol para {nombreJugador}! Conteo actual: {conteo}");
            
            // Verificar si este jugador ganó
            if (conteo >= AdminJuego.instancia.ObtenerGolesParaGanar())
            {
                Debug.Log($"¡{nombreJugador} ha ganado con {conteo} goles!");
            }
            
            // Notificar al AdminJuego pasando referencia a esta meta
            AdminJuego.instancia.BolaMetaAlcanzada(bola, this);
        }
    }
    
    // Método para actualizar el texto del contador
    private void ActualizarTextoContador()
    {
        if (textoContador != null)
        {
            int golesParaGanar = AdminJuego.instancia.ObtenerGolesParaGanar();
            textoContador.text = $"{nombreJugador}: {conteo}/{golesParaGanar}";
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
        
        // Mostrar el nombre del jugador en el editor
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * (alto/2 + 0.5f), nombreJugador);
        #endif
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
        Debug.Log($"Conteo de meta de {nombreJugador} reiniciado");
    }
    
    // Método para obtener el nombre del jugador
    public string ObtenerNombreJugador()
    {
        return nombreJugador;
    }
    
    // Método para verificar si este jugador ha ganado
    public bool HaGanado()
    {
        return conteo >= AdminJuego.instancia.ObtenerGolesParaGanar();
    }
}