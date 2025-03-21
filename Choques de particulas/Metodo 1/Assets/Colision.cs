using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particula : MonoBehaviour
{
    public float masa = 1.0f;  // Masa de la partícula
    public Vector2 velocidadInicial;  // Velocidad inicial de la partícula
    public float coeficienteRestitucion = 1.0f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocidadInicial;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Particula"))
        {
            Particula otraParticula = collision.gameObject.GetComponent<Particula>();

            if (otraParticula != null)
            {
                float masa1 = masa;
                float masa2 = otraParticula.masa;
                Vector2 v1 = rb.linearVelocity;
                Vector2 v2 = otraParticula.rb.linearVelocity;

                // Fórmula de velocidad después de colisión elástica
                Vector2 nuevaVelocidad1 = ((masa1 - masa2) / (masa1 + masa2)) * v1 + ((2 * masa2) / (masa1 + masa2)) * v2;
                Vector2 nuevaVelocidad2 = ((masa2 - masa1) / (masa1 + masa2)) * v2 + ((2 * masa1) / (masa1 + masa2)) * v1;

                // Aplicar coeficiente de restitución (rebote)
                nuevaVelocidad1 *= coeficienteRestitucion;
                nuevaVelocidad2 *= coeficienteRestitucion;

                rb.linearVelocity = nuevaVelocidad1;
                otraParticula.rb.linearVelocity = nuevaVelocidad2;
            }
        }
    }
}
