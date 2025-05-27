using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AdminJuego : MonoBehaviour
{
    // Singleton para acceso global
    public static AdminJuego instancia;
    
    // Prefab de la bola para instanciar
    public GameObject Bola;
    
    // Posición de inicio para la bola
    public Vector2 posicionInicio = Vector2.zero;
    
    // Tiempo de espera antes de crear una nueva bola
    public float tiempoEsperaRespawn = 1.5f;
    
    // Configuración del juego
    [Header("Configuración del Juego")]
    public int golesParaGanar = 5;
    public string nombreEscenaVictoria = "EscenaVictoria"; // Nombre de la escena a la que ir cuando termine el juego
    public float tiempoEsperaAntesCambioEscena = 2f; // Tiempo de espera antes del cambio de escena
    
    // Estado del juego
    private bool juegoTerminado = false;
    
    private void Awake()
    {
        // Configuración del singleton
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Inicializar el estado del juego
        juegoTerminado = false;
        
        // Crear la primera bola al iniciar
        if (Bola != null)
        {
            CrearNuevaBola();
        }
        else
        {
            Debug.LogError("¡El prefab de la bola no está asignado en el GestorJuego!");
        }
    }
    
    // Método llamado cuando una bola alcanza la meta
    public void BolaMetaAlcanzada(GameObject bola, Meta metaQueAnoto)
    {
        // Si el juego ya terminó, no hacer nada
        if (juegoTerminado)
            return;
        
        // Destruir la bola actual
        Destroy(bola);
        
        // Verificar si el juego ha terminado
        if (metaQueAnoto.ObtenerConteo() >= golesParaGanar)
        {
            TerminarJuego(metaQueAnoto);
        }
        else
        {
            // Iniciar la rutina para crear una nueva bola después de un tiempo
            StartCoroutine(CrearNuevaBolaConRetraso());
        }
    }
    
    // Método para terminar el juego
    private void TerminarJuego(Meta metaGanadora)
    {
        juegoTerminado = true;
        
        Debug.Log($"¡Juego terminado! La meta {metaGanadora.name} ganó con {metaGanadora.ObtenerConteo()} goles!");
        
        // Detener la creación de nuevas bolas
        StopAllCoroutines();
        
        // Destruir todas las bolas restantes
        GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Bola");
        foreach (var bolaRestante in todasLasBolas)
        {
            Destroy(bolaRestante);
        }
        
        // Cambiar a la escena de victoria después de un breve delay
        StartCoroutine(CambiarEscenaConRetraso());
    }
    
    // Corrutina para cambiar de escena con un retraso
    private IEnumerator CambiarEscenaConRetraso()
    {
        yield return new WaitForSeconds(tiempoEsperaAntesCambioEscena);
        CambiarEscena();
    }
    
    // Método para cambiar de escena
    public void CambiarEscena()
    {
        // Verificar si la escena existe en el build
        if (Application.CanStreamedLevelBeLoaded(nombreEscenaVictoria))
        {
            SceneManager.LoadScene(nombreEscenaVictoria);
        }
        else
        {
            Debug.LogError($"La escena '{nombreEscenaVictoria}' no existe o no está añadida al Build Settings!");
            // Como alternativa, podrías recargar la escena actual
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    // Crear una nueva bola inmediatamente
    public void CrearNuevaBola()
    {
        // Solo crear una nueva bola si el juego no ha terminado
        if (!juegoTerminado)
        {
            GameObject nuevaBola = Instantiate(Bola, posicionInicio, Quaternion.identity);
            Debug.Log("Nueva bola creada en la posición " + posicionInicio);
        }
    }
    
    // Esperar un tiempo antes de crear una nueva bola
    private IEnumerator CrearNuevaBolaConRetraso()
    {
        yield return new WaitForSeconds(tiempoEsperaRespawn);
        CrearNuevaBola();
    }
    
    // Reiniciar el juego (puedes llamar a este método desde un botón UI)
    public void ReiniciarJuego()
    {
        // Resetear el estado del juego
        juegoTerminado = false;
        
        // Detener todas las corrutinas
        StopAllCoroutines();
        
        // Destruir todas las bolas existentes
        GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Bola");
        foreach (var bola in todasLasBolas)
        {
            Destroy(bola);
        }
        
        // Resetear los contadores de todas las metas
        Meta[] todasLasMetas = FindObjectsOfType<Meta>();
        foreach (var meta in todasLasMetas)
        {
            meta.ResetearConteo();
        }
        
        // Crear una nueva bola
        CrearNuevaBola();
    }
    
    // Método público para verificar si el juego ha terminado
    public bool EstaJuegoTerminado()
    {
        return juegoTerminado;
    }
    
    // Método público para obtener los goles necesarios para ganar
    public int ObtenerGolesParaGanar()
    {
        return golesParaGanar;
    }
}