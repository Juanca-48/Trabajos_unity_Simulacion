using UnityEngine;
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
    public void BolaMetaAlcanzada(GameObject Bola)
    {
        // Destruir la bola actual
        Destroy(Bola);
        
        // Iniciar la rutina para crear una nueva bola después de un tiempo
        StartCoroutine(CrearNuevaBolaConRetraso());
    }
    
    // Crear una nueva bola inmediatamente
    public void CrearNuevaBola()
    {
        GameObject nuevaBola = Instantiate(Bola, posicionInicio, Quaternion.identity);
        Debug.Log("Nueva bola creada en la posición " + posicionInicio);
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
}
