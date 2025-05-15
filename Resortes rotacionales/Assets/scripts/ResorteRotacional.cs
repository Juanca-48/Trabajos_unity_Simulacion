using UnityEngine;

public class ResorteRotacionalDoble2D : MonoBehaviour
{
    // Parámetros del primer resorte
    public float masa1 = 1.0f; // Masa del objeto 1 (kg)
    public float longitudBrazo1 = 2.0f; // Longitud del brazo 1 (m)
    public float tasaMomento1 = 5.0f; // Tasa de momento del resorte 1 (Nm/rad)
    public float anguloInicial1 = 30.0f; // Ángulo inicial del resorte 1 (grados)

    // Parámetros del segundo resorte
    public float masa2 = 1.0f; // Masa del objeto 2 (kg)
    public float longitudBrazo2 = 1.5f; // Longitud del brazo 2 (m)
    public float tasaMomento2 = 4.0f; // Tasa de momento del resorte 2 (Nm/rad)
    public float anguloInicial2 = 20.0f; // Ángulo inicial del resorte 2 (grados)

    // Amortiguamiento (aplicado a ambos sistemas)
    public float amortiguamiento = 0.1f; // Factor de amortiguamiento (adimensional)

    // Variables privadas
    private float momentoInercia1; // Momento de inercia del resorte 1 (kg·m²)
    private float momentoInercia2; // Momento de inercia del resorte 2 (kg·m²)
    private float frecuenciaNatural1; // Frecuencia natural del resorte 1 (rad/s)
    private float frecuenciaNatural2; // Frecuencia natural del resorte 2 (rad/s)
    private float frecuenciaAmortiguada1; // Frecuencia amortiguada del resorte 1 (rad/s)
    private float frecuenciaAmortiguada2; // Frecuencia amortiguada del resorte 2 (rad/s)

    private Transform objetoTransform1; // Transform del objeto 1
    private Transform objetoTransform2; // Transform del objeto 2

    private float tiempo; // Tiempo acumulado

    void Start()
    {
        // Configuración inicial
        objetoTransform1 = transform.GetChild(0); // Primer objeto (hijo 1)
        objetoTransform2 = transform.GetChild(1); // Segundo objeto (hijo 2)

        // Cálculo de los momentos de inercia
        momentoInercia1 = masa1 * Mathf.Pow(longitudBrazo1, 2);
        momentoInercia2 = masa2 * Mathf.Pow(longitudBrazo2, 2);

        // Cálculo de las frecuencias naturales
        frecuenciaNatural1 = Mathf.Sqrt(tasaMomento1 / momentoInercia1);
        frecuenciaNatural2 = Mathf.Sqrt(tasaMomento2 / momentoInercia2);

        // Cálculo de las frecuencias amortiguadas
        frecuenciaAmortiguada1 = frecuenciaNatural1 * Mathf.Sqrt(1 - Mathf.Pow(amortiguamiento, 2));
        frecuenciaAmortiguada2 = frecuenciaNatural2 * Mathf.Sqrt(1 - Mathf.Pow(amortiguamiento, 2));

        // Convertir ángulos iniciales a radianes
        anguloInicial1 = Mathf.Deg2Rad * anguloInicial1;
        anguloInicial2 = Mathf.Deg2Rad * anguloInicial2;
    }

    void Update()
    {
        // Actualizar el tiempo
        tiempo += Time.deltaTime;

        // Cálculo del ángulo del primer resorte
        float angulo1 = anguloInicial1 * Mathf.Exp(-amortiguamiento * frecuenciaNatural1 * tiempo) * Mathf.Cos(frecuenciaAmortiguada1 * tiempo);

        // Posición del primer objeto
        float x1 = longitudBrazo1 * Mathf.Cos(angulo1);
        float y1 = longitudBrazo1 * Mathf.Sin(angulo1);
        objetoTransform1.position = new Vector3(x1, y1, 0);

        // Cálculo del ángulo del segundo resorte
        float angulo2 = anguloInicial2 * Mathf.Exp(-amortiguamiento * frecuenciaNatural2 * tiempo) * Mathf.Cos(frecuenciaAmortiguada2 * tiempo);

        // Posición del segundo objeto (relativa al primero)
        float x2 = x1 + longitudBrazo2 * Mathf.Cos(angulo2);
        float y2 = y1 + longitudBrazo2 * Mathf.Sin(angulo2);
        objetoTransform2.position = new Vector3(x2, y2, 0);
    }
}


