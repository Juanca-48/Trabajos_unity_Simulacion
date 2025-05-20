using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sistema integrado para la gestión de power ups en el juego
/// </summary>
public class SistemaPowerUps : MonoBehaviour
{
    #region Singleton
    public static SistemaPowerUps instancia;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Configuración Pública
    [Header("Configuración General")]
    public float coeficienteDebug = 1f;  // Utilizado para ajustar los valores de los efectos fácilmente

    [Header("Configuración de Spawning")]
    public GameObject[] prefabsPowerUp;
    public float tiempoEntreSpawns = 10f;
    public Transform[] puntosPosiblesSpawn;
    public bool spawnAleatorio = true;
    public bool spawnAlIniciar = true;

    [Header("Configuración de Tags")]
    [Tooltip("Tag que deben tener los objetos para ser considerados power-ups")]
    public string tagPowerUp = "Aumento";

    [Header("Efectos Visuales")]
    public GameObject efectoRecoleccion;
    public GameObject efectoActivacion;
    #endregion

    #region Variables Privadas
    // Estructura para almacenar datos de un power up activo
    private class PowerUpActivo
    {
        public string tipo;
        public GameObject objetivo;
        public float duracionRestante;
        public Action<GameObject> alFinalizar;
    }

    // Lista de power ups activos
    private List<PowerUpActivo> powerUpsActivos = new List<PowerUpActivo>();
    
    // Control de tiempo para spawning
    private float tiempoUltimoSpawn;
    #endregion

    #region Eventos
    // Eventos para notificar cuando se activa/desactiva un power up
    public event Action<string, GameObject> OnPowerUpActivado;
    public event Action<string, GameObject> OnPowerUpFinalizado;
    #endregion

    private void Start()
    {
        tiempoUltimoSpawn = Time.time;
        
        if (spawnAlIniciar)
        {
            SpawnPowerUp();
        }
    }

    private void Update()
    {
        // Actualizar tiempos de power ups activos
        ActualizarPowerUpsActivos();
        
        // Verificar si es tiempo de generar un nuevo power up
        if (Time.time - tiempoUltimoSpawn >= tiempoEntreSpawns)
        {
            SpawnPowerUp();
            tiempoUltimoSpawn = Time.time;
        }
    }

    /// <summary>
    /// Crea un nuevo power up en el mundo de juego
    /// </summary>
    public void SpawnPowerUp()
    {
        if (prefabsPowerUp == null || prefabsPowerUp.Length == 0)
        {
            Debug.LogWarning("No hay prefabs de power ups configurados");
            return;
        }

        // Seleccionar prefab y posición
        GameObject prefabSeleccionado = spawnAleatorio ? 
            prefabsPowerUp[UnityEngine.Random.Range(0, prefabsPowerUp.Length)] : 
            prefabsPowerUp[0];
            
        Vector3 posicionSpawn = (puntosPosiblesSpawn != null && puntosPosiblesSpawn.Length > 0) ?
            puntosPosiblesSpawn[UnityEngine.Random.Range(0, puntosPosiblesSpawn.Length)].position :
            transform.position;

        // Instanciar el power up
        GameObject powerUp = Instantiate(prefabSeleccionado, posicionSpawn, Quaternion.identity);
        
        // Asegurarse de que tenga el tag correcto
        powerUp.tag = tagPowerUp;
        
        // Asegurarse de que tenga el componente PowerUpItem
        if (powerUp.GetComponent<PowerUpItem>() == null)
        {
            powerUp.AddComponent<PowerUpItem>();
        }
        
        Debug.Log($"[PowerUp] Spawneado: {powerUp.name} en {posicionSpawn} con tag {tagPowerUp}");
    }

    /// <summary>
    /// Actualiza el tiempo restante de todos los power ups activos
    /// </summary>
    private void ActualizarPowerUpsActivos()
    {
        for (int i = powerUpsActivos.Count - 1; i >= 0; i--)
        {
            PowerUpActivo powerUp = powerUpsActivos[i];
            
            // Reducir tiempo restante
            powerUp.duracionRestante -= Time.deltaTime;
            
            // Verificar si ha terminado
            if (powerUp.duracionRestante <= 0 || powerUp.objetivo == null)
            {
                // Ejecutar callback de finalización
                if (powerUp.alFinalizar != null && powerUp.objetivo != null)
                {
                    powerUp.alFinalizar(powerUp.objetivo);
                }
                
                // Notificar que el power up ha finalizado
                if (powerUp.objetivo != null)
                {
                    OnPowerUpFinalizado?.Invoke(powerUp.tipo, powerUp.objetivo);
                    Debug.Log($"[PowerUp] Finalizado: {powerUp.tipo} en {powerUp.objetivo.name}");
                }
                
                // Eliminar de la lista
                powerUpsActivos.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Activa un power up en el objetivo especificado
    /// </summary>
    public void ActivarPowerUp(string tipo, GameObject objetivo, float duracion, Action<GameObject> efecto, Action<GameObject> alFinalizar = null)
    {
        if (objetivo == null)
        {
            Debug.LogWarning("[PowerUp] No se puede activar power up en objetivo nulo");
            return;
        }
        
        // Crear una instancia del power up activo
        PowerUpActivo nuevoPowerUp = new PowerUpActivo
        {
            tipo = tipo,
            objetivo = objetivo,
            duracionRestante = duracion,
            alFinalizar = alFinalizar
        };
        
        // Añadir a la lista de power ups activos
        powerUpsActivos.Add(nuevoPowerUp);
        
        // Aplicar el efecto inmediatamente
        efecto?.Invoke(objetivo);
        
        // Crear efecto visual si existe
        if (efectoActivacion != null)
        {
            Instantiate(efectoActivacion, objetivo.transform.position, Quaternion.identity, objetivo.transform);
        }
        
        // Notificar que se ha activado el power up
        OnPowerUpActivado?.Invoke(tipo, objetivo);
        Debug.Log($"[PowerUp] Activado: {tipo} en {objetivo.name} con duración {duracion}s");
    }

    /// <summary>
    /// Verifica si un objeto tiene un power up activo
    /// </summary>
    public bool TienePowerUpActivo(GameObject objetivo, string tipo)
    {
        foreach (var powerUp in powerUpsActivos)
        {
            if (powerUp.objetivo == objetivo && powerUp.tipo == tipo)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Cancela todos los power ups de un objeto
    /// </summary>
    public void CancelarPowerUps(GameObject objetivo)
    {
        for (int i = powerUpsActivos.Count - 1; i >= 0; i--)
        {
            if (powerUpsActivos[i].objetivo == objetivo)
            {
                string tipo = powerUpsActivos[i].tipo;
                powerUpsActivos[i].alFinalizar?.Invoke(objetivo);
                OnPowerUpFinalizado?.Invoke(tipo, objetivo);
                powerUpsActivos.RemoveAt(i);
            }
        }
    }
}

/// <summary>
/// Componente para objetos que pueden ser recogidos como power ups
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerUpItem : MonoBehaviour
{
    [Tooltip("Tipo de power up")]
    public string tipoPowerUp = "PowerUp";
    
    [Tooltip("Duración del efecto en segundos")]
    public float duracion = 5f;
    
    private void Start()
    {
        // Asegurar que tiene el tag correcto
        if (tag != SistemaPowerUps.instancia.tagPowerUp)
        {
            tag = SistemaPowerUps.instancia.tagPowerUp;
            Debug.Log($"[PowerUp] Configurando tag '{SistemaPowerUps.instancia.tagPowerUp}' al power up: {gameObject.name}");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si es un proyectil
        Proyectil proyectil = collision.GetComponent<Proyectil>();
        if (proyectil != null)
        {
            // Mostrar efecto de recolección
            if (SistemaPowerUps.instancia.efectoRecoleccion != null)
            {
                Instantiate(SistemaPowerUps.instancia.efectoRecoleccion, transform.position, Quaternion.identity);
            }
            
            // Activar el power up según su tipo
            ActivarPowerUp(proyectil.gameObject);
            
            // Destruir este item
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Activa el power up en el proyectil
    /// </summary>
    private void ActivarPowerUp(GameObject proyectil)
    {
        // Aquí se determina el comportamiento del power up basado en su tipo
        switch (tipoPowerUp)
        {
            case "MasaExtra":
                // Ejemplo: Aumentar masa
                SistemaPowerUps.instancia.ActivarPowerUp(
                    tipoPowerUp, 
                    proyectil, 
                    duracion,
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            // Guardar masa original en nombre para restaurarla después
                            obj.name = $"{obj.name}_OrigMasa_{proy.masa}";
                            proy.masa *= 2f * SistemaPowerUps.instancia.coeficienteDebug;
                            Debug.Log($"[PowerUp] Masa aumentada a {proy.masa}");
                        }
                    },
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            // Restaurar masa original
                            string nombre = obj.name;
                            int indice = nombre.LastIndexOf("_OrigMasa_");
                            if (indice >= 0)
                            {
                                string masaOrigStr = nombre.Substring(indice + 10);
                                if (float.TryParse(masaOrigStr, out float masaOrig))
                                {
                                    proy.masa = masaOrig;
                                    obj.name = nombre.Substring(0, indice);
                                    Debug.Log($"[PowerUp] Masa restaurada a {proy.masa}");
                                }
                            }
                        }
                    }
                );
                break;
                
            case "RadioExtra":
                // Ejemplo: Aumentar radio
                SistemaPowerUps.instancia.ActivarPowerUp(
                    tipoPowerUp, 
                    proyectil, 
                    duracion,
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            float radioOriginal = proy.radio;
                            // Almacenar radio original para restaurarlo
                            obj.name = $"{obj.name}_OrigRadio_{radioOriginal}";
                            proy.radio *= 1.5f * SistemaPowerUps.instancia.coeficienteDebug;
                            
                            // Ajustar también la escala visual
                            obj.transform.localScale *= 1.5f;
                            
                            Debug.Log($"[PowerUp] Radio aumentado a {proy.radio}");
                        }
                    },
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            // Extraer radio original del nombre
                            string nombre = obj.name;
                            int indice = nombre.LastIndexOf("_OrigRadio_");
                            if (indice >= 0)
                            {
                                string radioStr = nombre.Substring(indice + 11);
                                if (float.TryParse(radioStr, out float radioOrig))
                                {
                                    proy.radio = radioOrig;
                                    // Restaurar escala visual
                                    obj.transform.localScale = new Vector3(1, 1, 1);
                                    // Restaurar nombre
                                    obj.name = nombre.Substring(0, indice);
                                    Debug.Log($"[PowerUp] Radio restaurado a {proy.radio}");
                                }
                            }
                        }
                    }
                );
                break;

            case "Gravedad":
                // Nuevo power up: Activar gravedad
                SistemaPowerUps.instancia.ActivarPowerUp(
                    tipoPowerUp,
                    proyectil,
                    duracion,
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            // Configurar color para indicar visualmente que está bajo gravedad
                            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                            {
                                // Guardar color original
                                obj.name = $"{obj.name}_OrigColor_{ColorToString(renderer.color)}";
                                // Aplicar tinte para indicar que está bajo gravedad
                                renderer.color = new Color(0.7f, 0.7f, 1.0f);
                            }
                            
                            // Activar la gravedad en el sistema físico
                            SistemaFisica.instancia.ActivarGravedad(obj, true, 0.2f * SistemaPowerUps.instancia.coeficienteDebug);
                            Debug.Log($"[PowerUp] Gravedad activada para {obj.name}");
                        }
                    },
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            // Restaurar color original
                            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                            {
                                string nombre = obj.name;
                                int indice = nombre.LastIndexOf("_OrigColor_");
                                if (indice >= 0)
                                {
                                    string colorStr = nombre.Substring(indice + 10);
                                    renderer.color = StringToColor(colorStr);
                                    obj.name = nombre.Substring(0, indice);
                                }
                            }
                            
                            // Desactivar la gravedad
                            SistemaFisica.instancia.ActivarGravedad(obj, false);
                            Debug.Log($"[PowerUp] Gravedad desactivada para {obj.name}");
                        }
                    }
                );
                break;
                
            default:
                Debug.LogWarning($"[PowerUp] Tipo de power up desconocido: {tipoPowerUp}");
                break;
        }
    }
    
    // Método auxiliar para convertir un color a string
    private string ColorToString(Color color)
    {
        return $"{color.r}_{color.g}_{color.b}_{color.a}";
    }
    
    // Método auxiliar para convertir un string a color
    private Color StringToColor(string colorStr)
    {
        string[] componentes = colorStr.Split('_');
        if (componentes.Length >= 4)
        {
            float r = float.Parse(componentes[0]);
            float g = float.Parse(componentes[1]);
            float b = float.Parse(componentes[2]);
            float a = float.Parse(componentes[3]);
            return new Color(r, g, b, a);
        }
        return Color.white;
    }
}

/// <summary>
/// Componente para crear fácilmente un power up desde el editor usando un sprite 2D
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PowerUpSprite : MonoBehaviour
{
    [Tooltip("Tipo de power up")]
    public string tipoPowerUp = "MasaExtra";
    
    [Tooltip("Duración del efecto en segundos")]
    public float duracion = 5f;
    
    [Tooltip("¿El collider debe ser trigger?")]
    public bool esTrigger = true;
    
    void Start()
    {
        // Configurar tag
        tag = SistemaPowerUps.instancia.tagPowerUp;
        
        // Configurar el collider como trigger
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = esTrigger;
        }
        
        // Añadir el componente PowerUpItem y configurarlo
        PowerUpItem item = gameObject.AddComponent<PowerUpItem>();
        item.tipoPowerUp = tipoPowerUp;
        item.duracion = duracion;
        
        Debug.Log($"[PowerUp] Sprite configurado como power up: {gameObject.name}, tipo: {tipoPowerUp}, duración: {duracion}s");
    }
}