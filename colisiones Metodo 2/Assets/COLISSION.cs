using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElasticCollision : MonoBehaviour
{
    public float mass;
    public Vector2 initialVelocity;
    private Rigidbody2D rb;

    // Referencia al otro objeto
    public GameObject otherObject;
    private Rigidbody2D otherRb;

    // Factor de elasticidad (0-1), donde 1 es perfectamente el�stico
    public float elasticity = 1.0f;

    void Start()
    {
        // Obtener el Rigidbody2D de este objeto
        rb = GetComponent<Rigidbody2D>();

        // Configurar la masa y velocidad inicial
        rb.mass = mass;
        rb.linearVelocity = initialVelocity;

        // Eliminar efectos que reducen energ�a
        rb.gravityScale = 0;
        rb.linearDamping = 0; // Eliminar fricci�n con el aire
        rb.angularDamping = 0; // Eliminar fricci�n rotacional

        // Configurar material para colisiones perfectamente el�sticas
        // Solo si no has configurado ya un Physics Material 2D
        if (GetComponent<CircleCollider2D>() != null && GetComponent<CircleCollider2D>().sharedMaterial == null)
        {
            PhysicsMaterial2D bouncyMaterial = new PhysicsMaterial2D();
            bouncyMaterial.bounciness = 1.0f;
            bouncyMaterial.friction = 0.0f;
            GetComponent<CircleCollider2D>().sharedMaterial = bouncyMaterial;
        }

        // Obtener referencia al otro objeto
        if (otherObject != null)
        {
            otherRb = otherObject.GetComponent<Rigidbody2D>();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si la colisi�n es con el otro objeto que nos interesa
        if (collision.gameObject == otherObject)
        {
            // Calcular velocidad relativa
            Vector2 relativeVelocity = rb.linearVelocity - otherRb.linearVelocity;

            // Direcci�n de la colisi�n (normalizada)
            Vector2 collisionNormal = (rb.position - otherRb.position).normalized;

            // Velocidad a lo largo de la direcci�n de colisi�n
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, collisionNormal);

            // Si los objetos se est�n alejando, no hacemos nada
            if (velocityAlongNormal > 0)
                return;

            // Impulso que se aplicar�
            float impulseScalar = -(1 + elasticity) * velocityAlongNormal;
            impulseScalar /= (1 / rb.mass) + (1 / otherRb.mass);

            // Aplicar impulso
            Vector2 impulse = impulseScalar * collisionNormal;
            rb.linearVelocity += impulse / rb.mass;
            otherRb.linearVelocity -= impulse / otherRb.mass;

            Debug.Log("Colisi�n el�stica entre: " + gameObject.name + " y " + collision.gameObject.name);
        }
    }

    // Alternativa: Puedes utilizar esto para prevenir que Unity maneje las colisiones a su manera
    void OnCollisionStay2D(Collision2D collision)
    {
        // Asegurarse de que los objetos no pierdan velocidad debido a contacto continuo
        rb.linearVelocity = rb.linearVelocity.magnitude * rb.linearVelocity.normalized;
        if (collision.gameObject == otherObject)
        {
            otherRb.linearVelocity = otherRb.linearVelocity.magnitude * otherRb.linearVelocity.normalized;
        }
    }
}