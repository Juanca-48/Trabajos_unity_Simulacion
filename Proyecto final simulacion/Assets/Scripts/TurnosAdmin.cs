using UnityEngine;
using System.Collections;
using TMPro; // Cambiado de UnityEngine.UI.Text a TextMeshPro
using System;

public class TurnosAdmin : MonoBehaviour
{
    public static TurnosAdmin instancia;

    // Event para notificar cambios de turno (envía el tag del jugador actual)
    public event Action<string> OnCambioTurno;

    // Referencias a los jugadores
    private GameObject jugador1;
    private GameObject jugador2;
    private GameObject jugadorActual;

    // Control de turnos
    private bool turnoActivo = false;
    private bool esperandoFinTurno = false;
    private GameObject proyectilActivo = null;

    // Referencias a componentes
    private ControlResortera resorteraActual;

    // UI para mostrar el turno actual (cambiado a TextMeshPro)
    public TMP_Text textoTurnoActual;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Buscar jugadores por tag
        jugador1 = GameObject.FindGameObjectWithTag("Jugador_1");
        jugador2 = GameObject.FindGameObjectWithTag("Jugador_2");

        if (jugador1 == null || jugador2 == null)
        {
            Debug.LogError("No se encontraron los jugadores con las etiquetas 'Jugador_1' y 'Jugador_2'");
            return;
        }

        // Iniciar el primer turno
        IniciarPrimerTurno();
    }

    private void IniciarPrimerTurno()
    {
        // Establecer el primer turno al jugador 1
        jugadorActual = jugador1;
        ActivarJugador(jugador1);
        DesactivarJugador(jugador2);
        ActualizarInterfazTurno();
        
        turnoActivo = true;
        esperandoFinTurno = false;
    }

    private void ActivarJugador(GameObject jugador)
    {
        if (jugador == null) return;
        
        // Activar el control de la resortera para este jugador
        ControlResortera controlResortera = jugador.GetComponent<ControlResortera>();
        if (controlResortera != null)
        {
            controlResortera.enabled = true;
            resorteraActual = controlResortera;
            
            // Suscribirse al evento de disparo
            controlResortera.OnProyectilDisparado += ProyectilDisparado;
        }
    }

    private void DesactivarJugador(GameObject jugador)
    {
        if (jugador == null) return;
        
        // Desactivar el control de la resortera para este jugador
        ControlResortera controlResortera = jugador.GetComponent<ControlResortera>();
        if (controlResortera != null)
        {
            controlResortera.enabled = false;
            
            // Desuscribirse del evento de disparo
            controlResortera.OnProyectilDisparado -= ProyectilDisparado;
        }
    }

    private void ProyectilDisparado(GameObject proyectil)
    {
        if (!turnoActivo || esperandoFinTurno) return;
        
        // Registrar el proyectil activo y comenzar a esperar que se destruya
        proyectilActivo = proyectil;
        esperandoFinTurno = true;
        
        // Iniciar corrutina para verificar si el proyectil sigue existiendo
        StartCoroutine(EsperarDestruccionProyectil(proyectil));
    }

    private IEnumerator EsperarDestruccionProyectil(GameObject proyectil)
    {
        // Esperar mientras el proyectil exista
        while (proyectil != null)
        {
            yield return null;
        }
        
        // Cuando el proyectil se destruya, cambiar el turno
        proyectilActivo = null;
        CambiarTurno();
    }

    private void CambiarTurno()
    {
        if (!turnoActivo) return;
        
        esperandoFinTurno = false;
        
        // Cambiar al otro jugador
        if (jugadorActual == jugador1)
        {
            jugadorActual = jugador2;
            DesactivarJugador(jugador1);
            ActivarJugador(jugador2);
        }
        else
        {
            jugadorActual = jugador1;
            DesactivarJugador(jugador2);
            ActivarJugador(jugador1);
        }
        
        // Actualizar la interfaz
        ActualizarInterfazTurno();
        
        Debug.Log($"Turno cambiado a: {jugadorActual.tag}");
    }

    private void ActualizarInterfazTurno()
    {
        if (textoTurnoActual != null)
        {
            textoTurnoActual.text = $"Turno de: {jugadorActual.tag}";
        }
        
        // Invocar el evento de cambio de turno
        OnCambioTurno?.Invoke(jugadorActual.tag);
    }

    // Método público para reiniciar el sistema de turnos
    public void ReiniciarTurnos()
    {
        StopAllCoroutines();
        turnoActivo = false;
        esperandoFinTurno = false;
        
        if (proyectilActivo != null)
        {
            Destroy(proyectilActivo);
            proyectilActivo = null;
        }
        
        // Reiniciar a primer turno
        IniciarPrimerTurno();
    }

    // Método para verificar si el turno está activo y espera por un disparo
    public bool PuedeDisparar()
    {
        return turnoActivo && !esperandoFinTurno;
    }
}