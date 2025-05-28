using UnityEngine;

public class Bola : MonoBehaviour, IObjetoColisionable
{
    public float masa = 1f;
    public float radio = 0.5f;

    void Start()
    {
        // Registrar la bola en el sistema de física sin velocidad inicial
        SistemaFisica.instancia.RegistrarObjeto(gameObject, masa, radio);
    }

    public void OnColision(GameObject otro)
    {
        if (otro == null)
        {
            // Colisión con los límites del área
            Debug.Log("[Bola] Colisión con límite detectada");
            return;
        }

        if (otro.CompareTag("Proyectil"))
        {
            Debug.Log("[Bola] Golpeada por proyectil");
        }
    }

    void OnDestroy()
    {
        if (SistemaFisica.instancia != null)
            SistemaFisica.instancia.EliminarObjeto(gameObject);
    }
}