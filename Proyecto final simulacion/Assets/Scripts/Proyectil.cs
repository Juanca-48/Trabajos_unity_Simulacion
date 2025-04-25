using UnityEngine;
using System.Collections;

public class Proyectil : MonoBehaviour, IObjetoColisionable
{
    public float masa = 1f;
    public float radio = 0.25f;
    public float tiempoAutodestruccionPrimeraColision = 3f;
    public float tiempoAutodestruccionSinColisiones = 5f;

    private Vector2 velocidadInicial;
    private int contadorColisiones = 0;
    private bool temporizadorColisionActivo = false;
    private Coroutine temporizadorColisionCoroutine;
    private Coroutine temporizadorInicialCoroutine;

    public void Inicializar(Vector2 vel)
    {
        velocidadInicial = vel;
        SistemaFisica.instancia.RegistrarObjeto(gameObject, masa, radio, velocidadInicial);
        Debug.Log($"[Proyectil] Inicializado con vel={vel}");
        
        // Iniciar temporizador de vida máxima sin colisiones
        temporizadorInicialCoroutine = StartCoroutine(TemporizadorSinColisiones());
    }

    public void OnColision(GameObject otro)
    {
        // Cancelar el temporizador inicial ya que ocurrió una colisión
        if (temporizadorInicialCoroutine != null)
        {
            StopCoroutine(temporizadorInicialCoroutine);
            temporizadorInicialCoroutine = null;
        }

        // Boundary collision
        if (otro == null)
        {
            contadorColisiones++;
            Debug.Log($"[Proyectil] Rebote límite #{contadorColisiones}");
        }
        else if (otro.CompareTag("Bola"))
        {
            Debug.Log("[Proyectil] Colisión con Bola: destruido");
            Destroy(gameObject);
            return;
        }
        else
        {
            contadorColisiones++;
            Debug.Log($"[Proyectil] Rebote objeto #{contadorColisiones} con {otro.name}");
        }

        // Después de la primera colisión, iniciar temporizador de autodestrucción
        if (contadorColisiones == 1)
        {
            IniciarTemporizadorPostColision();
        }
        // Reiniciar temporizador en colisiones subsecuentes
        else if (temporizadorColisionActivo)
        {
            ReiniciarTemporizador();
        }

        if (contadorColisiones >= 2)
        {
            Debug.Log("[Proyectil] Segundo rebote: destruido");
            Destroy(gameObject);
        }
    }

    private void IniciarTemporizadorPostColision()
    {
        if (!temporizadorColisionActivo)
        {
            temporizadorColisionActivo = true;
            temporizadorColisionCoroutine = StartCoroutine(TemporizadorPostColision());
            Debug.Log($"[Proyectil] Temporizador post-colisión iniciado ({tiempoAutodestruccionPrimeraColision} segundos)");
        }
    }

    private void ReiniciarTemporizador()
    {
        if (temporizadorColisionCoroutine != null)
        {
            StopCoroutine(temporizadorColisionCoroutine);
            temporizadorColisionCoroutine = StartCoroutine(TemporizadorPostColision());
            Debug.Log("[Proyectil] Temporizador post-colisión reiniciado");
        }
    }

    private IEnumerator TemporizadorPostColision()
    {
        yield return new WaitForSeconds(tiempoAutodestruccionPrimeraColision);
        Debug.Log("[Proyectil] Tiempo agotado sin colisiones después de primer rebote: destruido");
        Destroy(gameObject);
    }

    private IEnumerator TemporizadorSinColisiones()
    {
        yield return new WaitForSeconds(tiempoAutodestruccionSinColisiones);
        Debug.Log("[Proyectil] Tiempo máximo de vida sin colisiones agotado: destruido");
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (SistemaFisica.instancia != null)
            SistemaFisica.instancia.EliminarObjeto(gameObject);
    }
}