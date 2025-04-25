using UnityEngine;
using System.Collections;

public class Proyectil : MonoBehaviour, IObjetoColisionable
{
    public float masa = 1f;
    public float radio = 0.25f;
    public float tiempoMaximoVida = 5f;
    public float tiempoMaximoPostColision = 3f;

    private Vector2 velocidadInicial;
    private int contadorColisiones = 0;
    private Coroutine temporizadorActivo;

    public void Inicializar(Vector2 vel)
    {
        velocidadInicial = vel;
        SistemaFisica.instancia.RegistrarObjeto(gameObject, masa, radio, velocidadInicial);
        Debug.Log($"[Proyectil] Inicializado con vel={vel}");
        
        // Iniciar temporizador de vida máxima
        IniciarTemporizador(tiempoMaximoVida, "Tiempo máximo de vida agotado");
    }

    public void OnColision(GameObject otro)
    {
        // Colisión con una bola
        if (otro != null && otro.CompareTag("Bola"))
        {
            Debug.Log("[Proyectil] Colisión con Bola: destruido");
            Destroy(gameObject);
            return;
        }

        // Incrementar contador de colisiones
        contadorColisiones++;
        
        // Registrar el tipo de colisión
        string tipoColision = otro == null ? "límite" : otro.name;
        Debug.Log($"[Proyectil] Rebote #{contadorColisiones} con {tipoColision}");

        // Si es segunda colisión, destruir
        if (contadorColisiones >= 2)
        {
            Debug.Log("[Proyectil] Segundo rebote: destruido");
            Destroy(gameObject);
            return;
        }

        // Si es primera colisión, reiniciar temporizador
        if (contadorColisiones == 1)
        {
            IniciarTemporizador(tiempoMaximoPostColision, "Tiempo agotado después de primera colisión");
        }
    }

    private void IniciarTemporizador(float tiempo, string mensaje)
    {
        // Cancelar temporizador anterior si existe
        if (temporizadorActivo != null)
        {
            StopCoroutine(temporizadorActivo);
        }
        
        // Iniciar nuevo temporizador
        temporizadorActivo = StartCoroutine(Temporizador(tiempo, mensaje));
    }

    private IEnumerator Temporizador(float tiempo, string mensaje)
    {
        yield return new WaitForSeconds(tiempo);
        Debug.Log($"[Proyectil] {mensaje}: destruido");
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (SistemaFisica.instancia != null)
            SistemaFisica.instancia.EliminarObjeto(gameObject);
    }
}