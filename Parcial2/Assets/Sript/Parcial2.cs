using UnityEngine;
using System;

public class Parcial2 : MonoBehaviour
{
    public float DistanciaInicial = 0f, k = 10f, F;
    public float mA = 1f, mR = 1f, b = 0.5f, e = 0.7f, vxA;
    private float X, v, a, Ancla = 0;
    private float PosEquilibrio;

    public float limIzq = -4f, limDer = 8f, E_Pared = 1.0f;

    private float xaA, xaR, vaxA, vaxR, xauxA_R, vxR;

    Vector2 movimientoAzul = Vector2.zero, movimientoRojo = Vector2.zero;
    public GameObject particula_A, particula_R;

    public Color ColorResorte = Color.magenta;
    private LineRenderer Resorte;

    void Start()
    {
        Resorte = gameObject.AddComponent<LineRenderer>();
        Resorte.startWidth = Resorte.endWidth = 0.05f;
        Resorte.material = new Material(Shader.Find("Sprites/Default"));
        Resorte.startColor = Resorte.endColor = ColorResorte;
        Resorte.positionCount = 2;

        float direccion = (transform.position.x > Ancla) ? 1 : -1;
        PosEquilibrio = Ancla + direccion * DistanciaInicial;

        X = v = a = 0f;
        vxA = 0f;

        Vector3 NuevaPos = transform.position;
        NuevaPos.x = PosEquilibrio;
        transform.position = NuevaPos;

        Resorte.SetPosition(1, new Vector3(Ancla, transform.position.y, transform.position.z));

        particula_A = GameObject.Find("Particula");
        particula_R = GameObject.Find("Masa");

        if (particula_A == null || particula_R == null) {
            Debug.LogError("No se encontraron los objetos requeridos!");
            return;
        }

        xaA = particula_A.transform.position.x;
        xaR = particula_R.transform.position.x;
        
        movimientoAzul.x = 0f;
        movimientoAzul.y = 0f;
        particula_A.transform.position = movimientoAzul;
        
        movimientoRojo.x = PosEquilibrio;
        movimientoRojo.y = particula_R.transform.position.y;
        particula_R.transform.position = movimientoRojo;
        
        vxR = v;
    }

    void LateUpdate()
    {

        float PosM = particula_R.transform.position.x;
        Resorte.SetPosition(0, new Vector3(PosM, particula_R.transform.position.y, particula_R.transform.position.z));
    }

    void FixedUpdate()
    {
        xaR = particula_R.transform.position.x;
        X = xaR - PosEquilibrio;

        if (mR != 0)
        {
            a = (F - k * X - b * v) / mR;
            
            v += a * Time.fixedDeltaTime;
            X += v * Time.fixedDeltaTime;

            xaR = PosEquilibrio + X;
            vxR = v;
            
            movimientoRojo.x = xaR;
            movimientoRojo.y = particula_R.transform.position.y;
            particula_R.transform.position = movimientoRojo;
        }
    }
    void Update()
    {
        ColisionesParticulas();
        ColisionesParedes();
    }
    public void AplicarFuerza(float Fuerza)
    {
        v += Fuerza / mR;
        vxR = v;
    }
    public void SetFuerza(float Fuerza)
    {
        F = Fuerza;
    }
    public void AnclaPto(float newAnchorX)
    {
        Ancla = newAnchorX;
        float direction = (particula_R.transform.position.x > Ancla) ? 1 : -1;
        PosEquilibrio = Ancla + direction * DistanciaInicial;
        
        if (Resorte != null)
        {
            Resorte.SetPosition(1, new Vector3(Ancla, particula_R.transform.position.y, particula_R.transform.position.z));
        }
    }
    
    public void Desplazamiento(float Xdesp)
    {
        X = Xdesp;
        v = 0f;
        vxR = 0f;
        xaR = PosEquilibrio + X;
        
        movimientoRojo.x = xaR;
        movimientoRojo.y = particula_R.transform.position.y;
        particula_R.transform.position = movimientoRojo;
    }

    void ColisionesParticulas()
    {
        xaA = particula_A.transform.position.x;
        xauxA_R = Math.Abs(xaA - xaR);

        if (xauxA_R <= 1)
        {
            Debug.Log("Colisión Particula y Masa");
            float tempVxA = vxA;
            float tempVxR = vxR;
            
            vaxA = ((mA - e * mR) / (mA + mR)) * tempVxA + ((1 + e) * mR / (mA + mR)) * tempVxR;
            vaxR = ((1 + e) * mA / (mA + mR)) * tempVxA + ((mR - e * mA) / (mA + mR)) * tempVxR;

            vxA = vaxA;
            vxR = vaxR;
            v = vxR;
            
            float separacion = 1.01f;
            if (xaA < xaR) {
                xaA = xaR - separacion;
            } else {
                xaA = xaR + separacion;
            }
        }
        xaA += vxA * Time.deltaTime;
        
        movimientoAzul.x = xaA;
        movimientoAzul.y = particula_A.transform.position.y;
        particula_A.transform.position = movimientoAzul;
    }
    void ColisionesParedes()
    {
        if (xaA <= limIzq)
        {
            xaA = limIzq;
            vxA = -vxA * E_Pared;
            Debug.Log("Partícula rebota en pared izquierda");
        }
        else if (xaA >= limDer)
        {
            xaA = limDer;
            vxA = -vxA * E_Pared;
            Debug.Log("Partícula rebota en pared derecha");
        }
        if (particula_A != null)
        {
            movimientoAzul.x = xaA;
            particula_A.transform.position = movimientoAzul;
        }
    }
}