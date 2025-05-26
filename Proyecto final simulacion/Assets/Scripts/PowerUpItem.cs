using UnityEngine;
using System;

public class PowerUpItem1 : MonoBehaviour, IObjetoColisionable
{
    [Header("Configuración del Power-Up")]
    public string tipoPowerUp = "Generico";  // Tipo del power-up (puede ser usado para lógica específica)
    public event Action OnDestruido;         // Evento que se llama al destruir el power-up

    private bool yaRecogido = false;         // Bandera para evitar múltiples activaciones

    private void Start()
    {
        // Registrar este power-up en el sistema de físicas
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.RegistrarObjeto(gameObject, 1f, 0.5f); // Masa y radio predeterminados
        }

        Debug.Log($"[PowerUp] {gameObject.name} registrado en el sistema de físicas.");
    }

    private void OnDestroy()
    {
        // Invocar el evento OnDestruido y eliminar del sistema de físicas
        OnDestruido?.Invoke();
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.EliminarObjeto(gameObject);
        }

        Debug.Log($"[PowerUp] {gameObject.name} eliminado del sistema de físicas.");
    }

    /// <summary>
    /// Método que se ejecuta cuando ocurre una colisión dentro del sistema de físicas personalizado.
    /// </summary>
    /// <param name="colisionador">El objeto con el que colisionó este power-up.</param>
    public void OnColision(GameObject colisionador)
    {
        if (yaRecogido || colisionador == null) return;

        // Verificar si el objeto que colisionó es un proyectil
        if (colisionador.CompareTag("Proyectil"))
        {
            yaRecogido = true;  // Marcar el power-up como "recogido"
            Debug.Log($"[PowerUp] Colisión detectada entre {gameObject.name} y proyectil: {colisionador.name}");

            // Registrar o manejar la colisión (opcional, por ejemplo, estadísticas)
            RegistrarColision();

            // Destruir el power-up
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Método opcional para registrar estadísticas o eventos de colisión.
    /// </summary>
    private void RegistrarColision()
    {
        Debug.Log($"[PowerUp] Power-up {gameObject.name} destruido tras colisión con proyectil.");
    }
}
