using UnityEngine;

public class TiroParabolico : MonoBehaviour
{
    public float g = 9.81f, m=5f, c=0.2f, v0=10f, angulo=30f;
    private float vx, vy, ax, ay;
    private Vector2 z0;
    void Start()
    {
        z0 = transform.position;
        
        float rad = angulo * Mathf.Deg2Rad;
        
        vx = v0 * Mathf.Cos(rad);
        vy = v0 * Mathf.Sin(rad);
    }
    void Update()
    {
        ax = -(c / m) * vx;
        ay = -g - (c / m) * vy;
        
        vx += ax * Time.deltaTime;
        vy += ay * Time.deltaTime;
        
        Vector2 posicion = transform.position;
        posicion.x += vx * Time.deltaTime;
        posicion.y += vy * Time.deltaTime;
        transform.position = posicion;

       Debug.Log (posicion.x);
       Debug.Log (posicion.y);

    }
}