using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using System; 

public class Colisiones : MonoBehaviour 
{ 
    public float vxA, vxR, vyA, vyR; 
    public float mA, mR, e; 
    public float limIzq = -8f, limDer = 8f, limTop = 4f, limBot = -4f, Rebote = 1.0f; 
 
    private float xaA, xaR, yaA, yaR; 
    private float vaxA, vaxR, vayA, vayR; 
    private float xauxA_R, yauxA_R,; 

    Vector2 movimientoAzul = Vector2.zero, movimientoRojo = Vector2.zero, movimientoMorado = Vector2.zero; 

    public GameObject particula_A, particula_R; 

    void Start() 
    { 
        particula_A = GameObject.Find("Azul"); 
        particula_R = GameObject.Find("Rojo"); 

        xaA = particula_A.transform.position.x; 
        xaR = particula_R.transform.position.x; 

        yaA = particula_A.transform.position.y; 
        yaR = particula_R.transform.position.y; 
    } 
    void Update() 
    { 
        ColisionesParticulas(); 
        ColisionesParedes(); 
    } 
    void ColisionesParticulas() 
    { 
        xauxA_R = Math.Abs(xaA - xaR);  
        yauxA_R = Math.Abs(yaA - yaR); 

        if (xauxA_R <= 1 && yauxA_R <= 1 ) 
        { 
            Debug.Log("Colisión Azul y Roja"); 
            vaxA = ((mA - mR) / (mA + mR)) * vxA + (2 * mR / (mA + mR)) * vxR; 
            vaxR = ((mR - mA) / (mA + mR)) * vxR + (2 * mA / (mA + mR)) * vxA; 

            vxA = vaxA; 
            vxR = vaxR; 

            vayA = ((mA - mR) / (mA + mR)) * vyA + (2 * mR / (mA + mR)) * vyR; 
            vayR = ((mR - mA) / (mA + mR)) * vyR + (2 * mA / (mA + mR)) * vyA; 

            vyA = vayA;
            vyR = vayR; 
        } 

            xaA += vxA * Time.deltaTime; 
            xaR += vxR * Time.deltaTime; 

            yaA += vyA * Time.deltaTime; 
            yaR += vyR * Time.deltaTime;  

            movimientoAzul.x = xaA; 
            movimientoRojo.x = xaR;  

            movimientoAzul.y = yaA; 
            movimientoRojo.y = yaR; 

            particula_A.transform.position = movimientoAzul; 
            particula_R.transform.position = movimientoRojo; 
    } 
    void ColisionesParedes() 
    { 
        // Verificar colisión con paredes para la partícula Azul 
        if (xaA <= limIzq) 
        { 
            xaA = limIzq; 
            vxA = -vxA * Rebote; 
            Debug.Log("Partícula Azul rebota en pared izquierda"); 
        } 
        else if (xaA >= limDer) 
        { 
            xaA = limDer; 
            vxA = -vxA * Rebote; 
            Debug.Log("Partícula Azul rebota en pared derecha"); 
        } 
        if (yaA <= limBot) 
        { 
            yaA = limBot; 
            vyA = -vyA * Rebote; 
            Debug.Log("Partícula Azul rebota en pared inferior"); 
        } 
        else if (yaA >= limTop) 
        { 
            yaA = limTop; 
            vyA = -vyA * Rebote; 
            Debug.Log("Partícula Azul rebota en pared superior"); 
        } 
        // Verificar colisión con paredes para la partícula Roja 
        if (xaR <= limIzq) 
        { 
            xaR = limIzq; 
            vxR = -vxR * Rebote; 
            Debug.Log("Partícula Roja rebota en pared izquierda"); 
        } 
        else if (xaR >= limDer) 
        { 
            xaR = limDer; 
            vxR = -vxR * Rebote; 
            Debug.Log("Partícula Roja rebota en pared derecha"); 
        } 
        if (yaR <= limBot) 
        { 
            yaR = limBot; 
            vyR = -vyR * Rebote; 
            Debug.Log("Partícula Roja rebota en pared inferior"); 
        } 
        else if (yaR >= limTop) 
        { 
            yaR = limTop; 
            vyR = -vyR * Rebote; 
            Debug.Log("Partícula Roja rebota en pared superior"); 
        } 
    }
} ``