using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AdminJuego : MonoBehaviour
{
    public static AdminJuego instancia;

    public GameObject Bola;
    public Vector2 posicionInicio = Vector2.zero;
    public float tiempoEsperaRespawn = 1.5f;

    [Header("Configuración del Juego")]
    public int golesParaGanar = 5;
    public float tiempoEsperaAntesCambioEscena = 3f; // Cambiado a 3s como pediste

    private bool juegoTerminado = false;
    private Scene escenaActual;

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

    void Start()
    {
        juegoTerminado = false;
        escenaActual = SceneManager.GetActiveScene();

        if (Bola != null)
        {
            CrearNuevaBola();
        }
        else
        {
            Debug.LogError("¡El prefab de la bola no está asignado en el GestorJuego!");
        }
    }

    public void BolaMetaAlcanzada(GameObject bola, Meta metaQueAnoto)
    {
        if (juegoTerminado)
            return;

        Destroy(bola);

        if (metaQueAnoto.ObtenerConteo() >= golesParaGanar)
        {
            TerminarJuego(metaQueAnoto);
        }
        else
        {
            StartCoroutine(CrearNuevaBolaConRetraso());
        }
    }

    private void TerminarJuego(Meta metaGanadora)
    {
        juegoTerminado = true;

        Debug.Log($"¡Juego terminado! La meta {metaGanadora.name} ganó con {metaGanadora.ObtenerConteo()} goles!");

        StopAllCoroutines();

        GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Bola");
        foreach (var bolaRestante in todasLasBolas)
        {
            Destroy(bolaRestante);
        }

        StartCoroutine(VolverAlMenuConRetraso());
    }

    private IEnumerator VolverAlMenuConRetraso()
    {
        yield return new WaitForSeconds(tiempoEsperaAntesCambioEscena);

        AsyncOperation cargaMenu = SceneManager.LoadSceneAsync("menu", LoadSceneMode.Additive);
        while (!cargaMenu.isDone)
        {
            yield return null;
        }

        Scene escenaMenu = SceneManager.GetSceneByName("menu");
        if (escenaMenu.IsValid())
        {
            SceneManager.SetActiveScene(escenaMenu);
            SceneManager.UnloadSceneAsync(escenaActual);
        }
        else
        {
            Debug.LogError("La escena 'menu' no fue encontrada después de cargarla.");
        }
    }

    public void CrearNuevaBola()
    {
        if (!juegoTerminado)
        {
            GameObject nuevaBola = Instantiate(Bola, posicionInicio, Quaternion.identity);
            Debug.Log("Nueva bola creada en la posición " + posicionInicio);
        }
    }

    private IEnumerator CrearNuevaBolaConRetraso()
    {
        yield return new WaitForSeconds(tiempoEsperaRespawn);
        CrearNuevaBola();
    }

    public void ReiniciarJuego()
    {
        juegoTerminado = false;
        StopAllCoroutines();

        GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Bola");
        foreach (var bola in todasLasBolas)
        {
            Destroy(bola);
        }

        Meta[] todasLasMetas = FindObjectsOfType<Meta>();
        foreach (var meta in todasLasMetas)
        {
            meta.ResetearConteo();
        }

        CrearNuevaBola();
    }

    public bool EstaJuegoTerminado()
    {
        return juegoTerminado;
    }

    public int ObtenerGolesParaGanar()
    {
        return golesParaGanar;
    }
}
