using UnityEngine;
using System;

public class ControlResortera : MonoBehaviour
{
    private Camera cam;
    private bool estaArrastrando = false;
    private Vector3 posicionInicial;
    private Vector3 posicionActualMouse;

    public float factorFuerza = 1.0f;
    public float maxDistanciaArrastre = 3.0f;
    public bool mostrarTrayectoria = true;
    public LineRenderer lineaVisualizacion;
    public float offsetRotacion = 0.0f;

    public GameObject prefabProyectil;
    public Transform puntoDisparo;

    // Evento para notificar cuando se dispara un proyectil
    public event Action<GameObject> OnProyectilDisparado;

    void Start()
    {
        cam = Camera.main;
        if (puntoDisparo == null) puntoDisparo = transform;
        if (mostrarTrayectoria && lineaVisualizacion == null)
        {
            lineaVisualizacion = gameObject.AddComponent<LineRenderer>();
            lineaVisualizacion.startWidth = 0.1f;
            lineaVisualizacion.endWidth = 0.1f;
            lineaVisualizacion.positionCount = 2;
            lineaVisualizacion.enabled = false;
        }
    }

    void Update()
    {
        posicionActualMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        posicionActualMouse.z = transform.position.z;
        ManejarControlesResortera();
    }

    void ManejarControlesResortera()
    {
        // Verificar si se puede disparar según el sistema de turnos
        if (TurnosAdmin.instancia != null && !TurnosAdmin.instancia.PuedeDisparar())
        {
            // No está activo el turno de este jugador, no permitir disparos
            if (estaArrastrando)
            {
                estaArrastrando = false;
                if (mostrarTrayectoria) lineaVisualizacion.enabled = false;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0) && EstaClicSobreObjeto())
        {
            estaArrastrando = true;
            posicionInicial = transform.position;
            if (mostrarTrayectoria) lineaVisualizacion.enabled = true;
        }

        if (estaArrastrando)
        {
            Vector2 dir = (Vector2)(transform.position - posicionActualMouse);
            if (dir.magnitude > 0.1f)
            {
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + offsetRotacion;
                transform.rotation = Quaternion.Euler(0,0,ang);
                if (mostrarTrayectoria)
                {
                    float dist = Mathf.Min(dir.magnitude, maxDistanciaArrastre);
                    Vector2 dl = dir.normalized * dist;
                    lineaVisualizacion.SetPosition(0, puntoDisparo.position);
                    lineaVisualizacion.SetPosition(1, puntoDisparo.position + (Vector3)dl);
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && estaArrastrando)
        {
            Vector2 direccionDisparo = ObtenerDireccionDisparo();
            GameObject proyectil = DispararProyectil(direccionDisparo);
            estaArrastrando = false;
            if (mostrarTrayectoria) lineaVisualizacion.enabled = false;
        }
    }

    private bool EstaClicSobreObjeto()
    {
        float distancia = Vector2.Distance(transform.position, posicionActualMouse);
        return distancia <= 0.5f;
    }

    public Vector2 ObtenerDireccionDisparo()
    {
        Vector2 direccion = (Vector2)(transform.position - posicionActualMouse);
        if (direccion.magnitude > maxDistanciaArrastre)
            direccion = direccion.normalized * maxDistanciaArrastre;
        return direccion * factorFuerza;
    }

    private GameObject DispararProyectil(Vector2 direccion)
    {
        GameObject proyectilGO = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);
        var pComp = proyectilGO.GetComponent<Proyectil>();
        if (pComp != null) 
        {
            pComp.Inicializar(direccion);
            
            // Invocar el evento para notificar que se disparó un proyectil
            OnProyectilDisparado?.Invoke(proyectilGO);
        }
        else 
        {
            Debug.LogError("El prefab no tiene el script Proyectil");
        }
        
        return proyectilGO;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxDistanciaArrastre);
        if (puntoDisparo != null && Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(puntoDisparo.position, 0.1f);
        }
    }
}