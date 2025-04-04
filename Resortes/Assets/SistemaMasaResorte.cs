using UnityEngine;

public class SistemaMasaResorte : MonoBehaviour
{
    public float DistanciaInicial = 2f, k = 10f, F;
    public float masa = 1f, b = 0.5f;

    private float X, v, a, Ancla = -2f;
    private float PosEquilibrio;

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

        Vector3 NuevaPos = transform.position;
        NuevaPos.x = PosEquilibrio;
        transform.position = NuevaPos;

        Resorte.SetPosition(1, new Vector3(Ancla, transform.position.y, transform.position.z));
    }

    void LateUpdate()
    {
        float PosM = transform.position.x;
        Resorte.SetPosition(0, new Vector3(PosM, transform.position.y, transform.position.z));
    }

    void FixedUpdate()
    {
        X = transform.position.x - PosEquilibrio;

        a = (F - k * X - b * v) / masa;
        v += a * Time.fixedDeltaTime;
        X += v * Time.fixedDeltaTime;

        Vector3 newPosition = transform.position;
        newPosition.x = PosEquilibrio + X;
        transform.position = newPosition;
    }

    public void AplicarFuerza(float Fuerza)
    {
        v += Fuerza / masa;
    }

    public void SetFuerza(float Fuerza)
    {
        F = Fuerza;
    }

    public void AnclaPto(float newAnchorX)
    {
        Ancla = newAnchorX;
        float direction = (transform.position.x > Ancla) ? 1 : -1;
        PosEquilibrio = Ancla + direction * DistanciaInicial;

        Resorte.SetPosition(1, new Vector3(Ancla, transform.position.y, transform.position.z));
    }

    public void Desplazamiento(float Xdesp)
    {
        X = Xdesp;
        v = 0f;

        Vector3 NuevaPos = transform.position;
        NuevaPos.x = PosEquilibrio + X;
        transform.position = NuevaPos;
    }
}


