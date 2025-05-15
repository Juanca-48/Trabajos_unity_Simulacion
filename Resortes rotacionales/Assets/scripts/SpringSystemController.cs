using UnityEngine;

public class SpringSystemController : MonoBehaviour
{
    public Transform M1; // Fixed mass
    public Transform M2;
    public Transform M3;

    public float mass1 = 10f;
    public float mass2 = 15f;
    public float mass3 = 21f;

    public float k1 = 10f;
    public float b1 = 1.26f;
    public float restLength1 = 1f;

    public float k2 = 10f;
    public float b2 = 1.26f;
    public float restLength2 = 1f;

    public float torqueM3 = 5f;
    public float torqueDecayDuration = 1.5f; // tiempo en segundos para desactivar torque

    public bool useSymplecticEuler = true;
    public float linearDamping2 = 0.1f;
    public float linearDamping3 = 0.1f;

    public Vector3 gravity = new Vector3(0f, -9.81f, 0f);

    public LineRenderer spring1Renderer;
    public LineRenderer spring2Renderer;
    public int springSegments = 20;
    public float coilRadius = 0.1f;
    public float coils = 3f;

    private Vector3 pos1;
    private Vector3 pos2;
    private Vector3 pos3;
    private Vector3 vel2;
    private Vector3 vel3;
    private float torqueTimer = 0f;
    private bool torqueActive = true;

    void Start()
    {
        pos1 = M1 != null ? M1.position : Vector3.zero;
        pos2 = M2 != null ? M2.position : Vector3.zero;
        pos3 = M3 != null ? M3.position : Vector3.zero;
        vel2 = Vector3.zero;
        vel3 = Vector3.zero;

        if (spring1Renderer != null) spring1Renderer.positionCount = springSegments + 1;
        if (spring2Renderer != null) spring2Renderer.positionCount = springSegments + 1;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Decaimiento del torque
        if (torqueActive)
        {
            torqueTimer += dt;
            if (torqueTimer >= torqueDecayDuration)
            {
                torqueActive = false;
            }
        }

        // Calcular fuerza del resorte M1-M2
        Vector3 dir12 = pos2 - pos1;
        float dist12 = dir12.magnitude;
        Vector3 norm12 = dir12.normalized;
        Vector3 force12 = -k1 * (dist12 - restLength1) * norm12
                         - b1 * Vector3.Dot(vel2, norm12) * norm12;

        // Calcular fuerza del resorte M2-M3
        Vector3 dir23 = pos3 - pos2;
        float dist23 = dir23.magnitude;
        Vector3 norm23 = dir23.normalized;
        Vector3 force23 = -k2 * (dist23 - restLength2) * norm23
                         - b2 * Vector3.Dot(vel3 - vel2, norm23) * norm23;

        // Gravedad
        Vector3 gravityForce2 = gravity * mass2;
        Vector3 gravityForce3 = gravity * mass3;

        Vector3 totalForce2 = force12 - force23 + gravityForce2;
        Vector3 totalForce3 = force23 + gravityForce3;

        // Aplicar torque a M3 solo durante los primeros segundos
        if (torqueActive)
        {
            Vector3 radial = pos3 - pos1;
            if (radial.sqrMagnitude > Mathf.Epsilon)
            {
                Vector3 tangent = new Vector3(-radial.y, radial.x, 0f).normalized;
                totalForce3 += tangent * torqueM3;
            }
        }

        // Integración
        if (useSymplecticEuler)
        {
            Vector3 accel2 = totalForce2 / mass2;
            vel2 += accel2 * dt;
            vel2 *= Mathf.Max(0f, 1f - linearDamping2 * dt);
            pos2 += vel2 * dt;

            Vector3 accel3 = totalForce3 / mass3;
            vel3 += accel3 * dt;
            vel3 *= Mathf.Max(0f, 1f - linearDamping3 * dt);
            pos3 += vel3 * dt;
        }
        else
        {
            Vector3 accel2 = totalForce2 / mass2;
            vel2 += accel2 * dt;
            vel2 *= Mathf.Max(0f, 1f - linearDamping2 * dt);
            pos2 += vel2 * dt;

            Vector3 accel3 = totalForce3 / mass3;
            vel3 += accel3 * dt;
            vel3 *= Mathf.Max(0f, 1f - linearDamping3 * dt);
            pos3 += vel3 * dt;
        }

        if (M2 != null) M2.position = pos2;
        if (M3 != null) M3.position = pos3;

        DrawSpring(spring1Renderer, pos1, pos2);
        DrawSpring(spring2Renderer, pos2, pos3);
    }

    void DrawSpring(LineRenderer lr, Vector3 pA, Vector3 pB)
    {
        if (lr == null) return;
        Vector3 delta = pB - pA;
        Vector3 dir = delta.normalized;
        for (int i = 0; i <= springSegments; i++)
        {
            float t = (float)i / springSegments;
            Vector3 center = Vector3.Lerp(pA, pB, t);
            float angle = t * coils * 2f * Mathf.PI;
            Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;
            Vector3 offset = perp * Mathf.Sin(angle) * coilRadius;
            lr.SetPosition(i, center + offset);
        }
    }
}
