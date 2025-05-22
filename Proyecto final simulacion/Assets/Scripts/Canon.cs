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

    [Header("Configuración visual de la línea")]
    public Material materialLinea; // Material personalizado para la línea
    public Color colorLinea = Color.red;
    public float anchoLinea = 1f;
    public int sortingOrder = 10; // Orden de renderizado alto para que aparezca encima

    public GameObject prefabProyectil;
    public Transform puntoDisparo;

    // Evento para notificar cuando se dispara un proyectil
    public event Action<GameObject> OnProyectilDisparado;

    void Start()
    {
        cam = Camera.main;
        if (puntoDisparo == null) puntoDisparo = transform;
        ConfigurarLineRenderer();
    }

    void ConfigurarLineRenderer()
    {
        if (mostrarTrayectoria)
        {
            if (lineaVisualizacion == null)
            {
                lineaVisualizacion = gameObject.AddComponent<LineRenderer>();
            }

            // Configuración básica del LineRenderer
            lineaVisualizacion.startWidth = anchoLinea;
            lineaVisualizacion.endWidth = anchoLinea;
            lineaVisualizacion.positionCount = 2;
            lineaVisualizacion.useWorldSpace = true;
            
            // Configurar material y color
            if (materialLinea != null)
            {
                lineaVisualizacion.material = materialLinea;
            }
            else
            {
                // Crear un material básico si no se asigna uno
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = colorLinea;
                lineaVisualizacion.material = mat;
            }
            
            // CONFIGURACIÓN CLAVE PARA QUE APAREZCA ENCIMA
            lineaVisualizacion.sortingLayerName = "Default"; // O el layer que uses
            lineaVisualizacion.sortingOrder = sortingOrder; // Número alto para renderizar encima
            
            // Para juegos 2D, asegurar que esté en el plano correcto
            lineaVisualizacion.alignment = LineAlignment.TransformZ;
            
            // Configuraciones adicionales para mejor calidad visual
            lineaVisualizacion.numCornerVertices = 4;
            lineaVisualizacion.numCapVertices = 4;
            lineaVisualizacion.generateLightingData = false;
            lineaVisualizacion.receiveShadows = false;
            lineaVisualizacion.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            
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
                if (mostrarTrayectoria && lineaVisualizacion != null) 
                    lineaVisualizacion.enabled = false;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0) && EstaClicSobreObjeto())
        {
            estaArrastrando = true;
            posicionInicial = transform.position;
            if (mostrarTrayectoria && lineaVisualizacion != null) 
                lineaVisualizacion.enabled = true;
        }

        if (estaArrastrando)
        {
            Vector2 dir = (Vector2)(transform.position - posicionActualMouse);
            if (dir.magnitude > 0.1f)
            {
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + offsetRotacion;
                transform.rotation = Quaternion.Euler(0,0,ang);
                
                if (mostrarTrayectoria && lineaVisualizacion != null)
                {
                    ActualizarVisualizacionLinea(dir);
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && estaArrastrando)
        {
            Vector2 direccionDisparo = ObtenerDireccionDisparo();
            GameObject proyectil = DispararProyectil(direccionDisparo);
            estaArrastrando = false;
            if (mostrarTrayectoria && lineaVisualizacion != null) 
                lineaVisualizacion.enabled = false;
        }
    }

    private void ActualizarVisualizacionLinea(Vector2 direccion)
    {
        float dist = Mathf.Min(direccion.magnitude, maxDistanciaArrastre);
        Vector2 dl = direccion.normalized * dist;
        
        // Asegurar que las posiciones estén en el plano Z correcto
        Vector3 puntoInicio = puntoDisparo.position;
        Vector3 puntoFin = puntoDisparo.position + (Vector3)dl;
        
        // Para juegos 2D, mantener Z consistente
        puntoInicio.z = transform.position.z - 0.1f; // Ligeramente hacia adelante
        puntoFin.z = transform.position.z - 0.1f;
        
        lineaVisualizacion.SetPosition(0, puntoInicio);
        lineaVisualizacion.SetPosition(1, puntoFin);
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

    // Método público para cambiar el sorting order en runtime si es necesario
    public void CambiarOrdenRenderizado(int nuevoOrden)
    {
        sortingOrder = nuevoOrden;
        if (lineaVisualizacion != null)
        {
            lineaVisualizacion.sortingOrder = sortingOrder;
        }
    }

    // Método para cambiar el color de la línea
    public void CambiarColorLinea(Color nuevoColor)
    {
        colorLinea = nuevoColor;
        if (lineaVisualizacion != null && lineaVisualizacion.material != null)
        {
            lineaVisualizacion.material.color = nuevoColor;
        }
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