using UnityEngine;
using UnityEngine.UI;
using TMPro; // Añadido para usar TextMeshPro

public class InterfazTurnos : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text textoTurnoActual; // Cambiado de Text a TMP_Text
    public Image indicadorJugador1;
    public Image indicadorJugador2;
    public Color colorJugadorActivo = Color.green;
    public Color colorJugadorInactivo = Color.gray;

    private void Start()
    {
        // Verificar que existe el administrador de turnos
        if (TurnosAdmin.instancia == null)
        {
            Debug.LogError("No se encontró el AdministradorTurnos en la escena");
            return;
        }

        // Asignar referencia al texto del turno
        if (textoTurnoActual != null)
        {
            TurnosAdmin.instancia.textoTurnoActual = textoTurnoActual;
        }

        // Suscribirse al evento de cambio de turno
        TurnosAdmin.instancia.OnCambioTurno += ActualizarIndicadores;
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento para evitar referencias nulas
        if (TurnosAdmin.instancia != null)
        {
            TurnosAdmin.instancia.OnCambioTurno -= ActualizarIndicadores;
        }
    }

    // Actualiza visualmente los indicadores de turno
    public void ActualizarIndicadores(string jugadorTag)
    {
        if (indicadorJugador1 != null && indicadorJugador2 != null)
        {
            indicadorJugador1.color = (jugadorTag == "Jugador_1") ? colorJugadorActivo : colorJugadorInactivo;
            indicadorJugador2.color = (jugadorTag == "Jugador_2") ? colorJugadorActivo : colorJugadorInactivo;
        }
    }
}