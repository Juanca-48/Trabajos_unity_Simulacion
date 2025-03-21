using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Colisiones : MonoBehaviour
{
    public float vxA, vxR, vxM;
    public float vyA, vyR, vyM;
    public float mA, mR, mM;

    private float xaA, xaR, xaM, yaA, yaR, yaM;
    private float vaxA, vaxR, vaxM, vayA, vayR, vayM;
    private float xauxA_R, xauxR_M, xauxA_M, yauxA_R, yauxR_M, yauxA_M;

    Vector2 movimientoAzul = Vector2.zero, movimientoRojo = Vector2.zero, movimientoMorado = Vector2.zero;

    public GameObject particula_A, particula_R, particula_M;
    
    public float limIzq = -8f, limDer = 8f, limTop = 4f, limBot = -4f, Rebote = 1.0f;

    void Start()
    {
        particula_A = GameObject.Find("Azul");
        particula_R = GameObject.Find("Rojo");
        particula_M = GameObject.Find("Morado");
        
        xaA = particula_A.transform.position.x;
        xaR = particula_R.transform.position.x;
        xaM = particula_M.transform.position.x;

        yaA = particula_A.transform.position.y;
        yaR = particula_R.transform.position.y;
        yaM = particula_M.transform.position.y;
    }

    void Update()
    {
        ColisionesParticulas();
        ColisionesParedes();
    }
    
    void ColisionesParticulas()
    {
        xauxA_R = Math.Abs(xaA - xaR);
        xauxR_M = Math.Abs(xaR - xaM);
        xauxA_M = Math.Abs(xaA - xaM);

        yauxA_R = Math.Abs(yaA - yaR);
        yauxR_M = Math.Abs(yaR - yaM);
        yauxA_M = Math.Abs(yaA - yaM);

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

        if (xauxR_M <= 1 && yauxR_M <= 1)
        {
            Debug.Log("Colisión Roja y Morada");
            vaxR = ((mR - mM) / (mR + mM)) * vxR + (2 * mM / (mR + mM)) * vxM;
            vaxM = ((mM - mR) / (mR + mM)) * vxM + (2 * mR / (mR + mM)) * vxR;
            vxR = vaxR;
            vxM = vaxM;
            
            vayR = ((mR - mM) / (mR + mM)) * vyR + (2 * mM / (mR + mM)) * vyM;
            vayM = ((mM - mR) / (mR + mM)) * vyM + (2 * mR / (mR + mM)) * vyR;
            vyR = vayR;
            vyM = vayM;
        }
        
        if (xauxA_M <= 1 && yauxA_M <= 1)
        {
            Debug.Log("Colisión Azul y Morada");
            vaxA = ((mA - mM) / (mA + mM)) * vxA + (2 * mM / (mA + mM)) * vxM;
            vaxM = ((mM - mA) / (mA + mM)) * vxM + (2 * mA / (mA + mM)) * vxA;
            vxA = vaxA;
            vxM = vaxM;

            vayA = ((mA - mM) / (mA + mM)) * vyA + (2 * mM / (mA + mM)) * vyM;
            vayM = ((mM - mA) / (mA + mM)) * vyM + (2 * mA / (mA + mM)) * vyA;
            vyA = vayA;
            vyM = vayM;
        }
            xaA += vxA * Time.deltaTime;
            xaR += vxR * Time.deltaTime;
            xaM += vxM * Time.deltaTime;

            yaA += vyA * Time.deltaTime;
            yaR += vyR * Time.deltaTime;
             yaM += vyM * Time.deltaTime;

            movimientoAzul.x = xaA;
            movimientoRojo.x = xaR;
            movimientoMorado.x = xaM;

            movimientoAzul.y = yaA;
            movimientoRojo.y = yaR;
            movimientoMorado.y = yaM;

            particula_A.transform.position = movimientoAzul;
            particula_R.transform.position = movimientoRojo;
            particula_M.transform.position = movimientoMorado;
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
        
        // Verificar colisión con paredes para la partícula Morada
        if (xaM <= limIzq)
        {
            xaM = limIzq;
            vxM = -vxM * Rebote;
            Debug.Log("Partícula Morada rebota en pared izquierda");
        }
        else if (xaM >= limDer)
        {
            xaM = limDer;
            vxM = -vxM * Rebote;
            Debug.Log("Partícula Morada rebota en pared derecha");
        }
        
        if (yaM <= limBot)
        {
            yaM = limBot;
            vyM = -vyM * Rebote;
            Debug.Log("Partícula Morada rebota en pared inferior");
        }
        else if (yaM >= limTop)
        {
            yaM = limTop;
            vyM = -vyM * Rebote;
            Debug.Log("Partícula Morada rebota en pared superior");
        }
    }
    
}