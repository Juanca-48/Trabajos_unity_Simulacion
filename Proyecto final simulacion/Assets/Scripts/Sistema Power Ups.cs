using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SistemaPowerUps : MonoBehaviour
{
    #region Singleton
    public static SistemaPowerUps instancia;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Configuración Pública
    [Header("Configuración General")]
    public float coeficienteDebug = 1f;

    [Header("Configuración de Spawning")]
    public GameObject[] prefabsPowerUp;
    public float tiempoEntreSpawns = 10f;
    public Transform[] puntosPosiblesSpawn;
    public bool spawnAleatorio = true;
    public bool spawnAlIniciar = true;
    [Tooltip("Máximo número de power-ups activos en el mundo")]
    public int maxPowerUpsEnMundo = 3;

    [Header("Configuración de Tags")]
    [Tooltip("Tag que deben tener los objetos para ser considerados power-ups")]
    public string tagPowerUp = "Aumento";
    #endregion

    #region Variables Privadas
    private class PowerUpActivo
    {
        public string tipo;
        public GameObject objetivo;
        public float duracionRestante;
        public Action<GameObject> alFinalizar;
    }

    private List<PowerUpActivo> powerUpsActivos = new List<PowerUpActivo>();
    private float tiempoUltimoSpawn;
    private List<GameObject> powerUpsEnMundo = new List<GameObject>();
    #endregion

    #region Eventos
    public event Action<string, GameObject> OnPowerUpActivado;
    public event Action<string, GameObject> OnPowerUpFinalizado;
    #endregion

    private void Start()
    {
        if (instancia != this)
        {
            Debug.LogError("[PowerUp] Error en la configuración del Singleton. Destruyendo este objeto.");
            Destroy(gameObject);
            return;
        }

        // Validar configuración antes de inicializar
        if (!ValidarConfiguracion())
        {
            Debug.LogError("[PowerUp] Configuración inválida. El sistema no funcionará correctamente.");
            return;
        }

        if (powerUpsActivos == null)
            powerUpsActivos = new List<PowerUpActivo>();
        
        if (powerUpsEnMundo == null)
            powerUpsEnMundo = new List<GameObject>();

        tiempoUltimoSpawn = Time.time;
        
        Debug.Log("[PowerUp] Sistema inicializado correctamente");
        
        if (spawnAlIniciar)
        {
            SpawnPowerUp();
        }
    }

    /// <summary>
    /// Valida que la configuración del sistema sea correcta
    /// </summary>
    private bool ValidarConfiguracion()
    {
        bool configValida = true;

        if (prefabsPowerUp == null || prefabsPowerUp.Length == 0)
        {
            Debug.LogError("[PowerUp] No hay prefabs de power ups configurados");
            configValida = false;
        }
        else
        {
            // Verificar que ningún prefab sea null
            for (int i = 0; i < prefabsPowerUp.Length; i++)
            {
                if (prefabsPowerUp[i] == null)
                {
                    Debug.LogError($"[PowerUp] El prefab en el índice {i} es null");
                    configValida = false;
                }
            }
        }

        if (puntosPosiblesSpawn == null || puntosPosiblesSpawn.Length == 0)
        {
            Debug.LogError("[PowerUp] No hay puntos de spawn configurados");
            configValida = false;
        }
        else
        {
            // Verificar que ningún punto de spawn sea null
            for (int i = 0; i < puntosPosiblesSpawn.Length; i++)
            {
                if (puntosPosiblesSpawn[i] == null)
                {
                    Debug.LogError($"[PowerUp] El punto de spawn en el índice {i} es null");
                    configValida = false;
                }
            }
        }

        if (string.IsNullOrEmpty(tagPowerUp))
        {
            Debug.LogWarning("[PowerUp] Tag de power up está vacío, usando 'Untagged'");
            tagPowerUp = "Untagged";
        }

        return configValida;
    }

    private void Update()
    {
        if (instancia == null || instancia != this)
        {
            Debug.LogWarning("[PowerUp] Problema con la instancia del Singleton en Update");
            return;
        }

        ActualizarPowerUpsActivos();
        LimpiarPowerUpsNulos();
        
        if (powerUpsEnMundo != null && 
            Time.time - tiempoUltimoSpawn >= tiempoEntreSpawns && 
            powerUpsEnMundo.Count < maxPowerUpsEnMundo)
        {
            SpawnPowerUp();
            tiempoUltimoSpawn = Time.time;
        }
    }

    private void LimpiarPowerUpsNulos()
    {
        if (powerUpsEnMundo == null) return;
        
        for (int i = powerUpsEnMundo.Count - 1; i >= 0; i--)
        {
            if (powerUpsEnMundo[i] == null)
            {
                powerUpsEnMundo.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Crea un nuevo power up en el mundo de juego con validación mejorada
    /// </summary>
    public void SpawnPowerUp()
    {
        // Validaciones de seguridad
        if (this == null)
        {
            Debug.LogError("[PowerUp] El componente SistemaPowerUps ha sido destruido");
            return;
        }

        if (prefabsPowerUp == null || prefabsPowerUp.Length == 0)
        {
            Debug.LogWarning("[PowerUp] No hay prefabs de power ups configurados");
            return;
        }

        if (puntosPosiblesSpawn == null || puntosPosiblesSpawn.Length == 0)
        {
            Debug.LogWarning("[PowerUp] No hay puntos de spawn configurados");
            return;
        }

        // Seleccionar prefab con validación
        GameObject prefabSeleccionado = null;
        int maxIntentos = prefabsPowerUp.Length;
        int intentos = 0;
        
        while (prefabSeleccionado == null && intentos < maxIntentos)
        {
            int indice = spawnAleatorio ? 
                UnityEngine.Random.Range(0, prefabsPowerUp.Length) : 0;
            
            if (prefabsPowerUp[indice] != null)
            {
                prefabSeleccionado = prefabsPowerUp[indice];
            }
            intentos++;
        }

        if (prefabSeleccionado == null)
        {
            Debug.LogError("[PowerUp] No se encontró ningún prefab válido para spawnear");
            return;
        }

        // Seleccionar posición con validación
        Transform puntoSpawn = null;
        maxIntentos = puntosPosiblesSpawn.Length;
        intentos = 0;
        
        while (puntoSpawn == null && intentos < maxIntentos)
        {
            int indice = UnityEngine.Random.Range(0, puntosPosiblesSpawn.Length);
            if (puntosPosiblesSpawn[indice] != null)
            {
                puntoSpawn = puntosPosiblesSpawn[indice];
            }
            intentos++;
        }

        if (puntoSpawn == null)
        {
            Debug.LogError("[PowerUp] No se encontró ningún punto de spawn válido");
            return;
        }

        Vector3 posicionSpawn = puntoSpawn.position;

        try
        {
            // Instanciar el power up
            GameObject powerUp = Instantiate(prefabSeleccionado, posicionSpawn, Quaternion.identity);
            
            if (powerUp == null)
            {
                Debug.LogError("[PowerUp] Falló la instanciación del power up");
                return;
            }
            
            // Configurar el power up
            ConfigurarPowerUp(powerUp);
            
            Debug.Log($"[PowerUp] Spawneado exitosamente: {powerUp.name} en {posicionSpawn}. Total en mundo: {powerUpsEnMundo.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PowerUp] Error al spawnear power up: {e.Message}");
        }
    }

    private void ConfigurarPowerUp(GameObject powerUp)
    {
        // Asegurarse de que tenga el tag correcto
        powerUp.tag = tagPowerUp;
        
        // Asegurarse de que tenga el componente PowerUpItem
        PowerUpItem powerUpItem = powerUp.GetComponent<PowerUpItem>();
        if (powerUpItem == null)
        {
            powerUpItem = powerUp.AddComponent<PowerUpItem>();
        }
        
        // Registrar en la lista de power ups en el mundo
        if (powerUpsEnMundo == null)
        {
            powerUpsEnMundo = new List<GameObject>();
        }
        powerUpsEnMundo.Add(powerUp);
        
        // Configurar callback para cuando se destruya
        powerUpItem.OnDestruido += () => {
            if (powerUpsEnMundo != null && powerUpsEnMundo.Contains(powerUp))
            {
                powerUpsEnMundo.Remove(powerUp);
            }
        };
    }

    private void ActualizarPowerUpsActivos()
    {
        if (powerUpsActivos == null) return;
        
        for (int i = powerUpsActivos.Count - 1; i >= 0; i--)
        {
            PowerUpActivo powerUp = powerUpsActivos[i];
            
            powerUp.duracionRestante -= Time.deltaTime;
            
            if (powerUp.duracionRestante <= 0 || powerUp.objetivo == null)
            {
                if (powerUp.alFinalizar != null && powerUp.objetivo != null)
                {
                    powerUp.alFinalizar(powerUp.objetivo);
                }
                
                if (powerUp.objetivo != null)
                {
                    OnPowerUpFinalizado?.Invoke(powerUp.tipo, powerUp.objetivo);
                    Debug.Log($"[PowerUp] Finalizado: {powerUp.tipo} en {powerUp.objetivo.name}");
                }
                
                powerUpsActivos.RemoveAt(i);
            }
        }
    }

    public void ActivarPowerUp(string tipo, GameObject objetivo, float duracion, Action<GameObject> efecto, Action<GameObject> alFinalizar = null)
    {
        if (objetivo == null)
        {
            Debug.LogWarning("[PowerUp] No se puede activar power up en objetivo nulo");
            return;
        }
        
        PowerUpActivo nuevoPowerUp = new PowerUpActivo
        {
            tipo = tipo,
            objetivo = objetivo,
            duracionRestante = duracion,
            alFinalizar = alFinalizar
        };
        
        if (powerUpsActivos == null)
        {
            powerUpsActivos = new List<PowerUpActivo>();
        }
        
        powerUpsActivos.Add(nuevoPowerUp);
        efecto?.Invoke(objetivo);
        
        OnPowerUpActivado?.Invoke(tipo, objetivo);
        Debug.Log($"[PowerUp] Activado: {tipo} en {objetivo.name} con duración {duracion}s");
    }

    public bool TienePowerUpActivo(GameObject objetivo, string tipo)
    {
        if (powerUpsActivos == null) return false;
        
        foreach (var powerUp in powerUpsActivos)
        {
            if (powerUp.objetivo == objetivo && powerUp.tipo == tipo)
                return true;
        }
        return false;
    }

    public void CancelarPowerUps(GameObject objetivo)
    {
        if (powerUpsActivos == null) return;
        
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

[RequireComponent(typeof(Collider2D))]
public class PowerUpItem : MonoBehaviour
{
    [Tooltip("Tipo de power up")]
    public string tipoPowerUp = "PowerUp";
    
    [Tooltip("Duración del efecto en segundos")]
    public float duracion = 5f;
    
    [Tooltip("¿Ya fue recogido? (para evitar múltiples activaciones)")]
    private bool yaRecogido = false;
    
    public event Action OnDestruido;
    
    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        if (SistemaPowerUps.instancia != null)
        {
            if (tag != SistemaPowerUps.instancia.tagPowerUp)
            {
                tag = SistemaPowerUps.instancia.tagPowerUp;
                Debug.Log($"[PowerUp] Configurando tag '{SistemaPowerUps.instancia.tagPowerUp}' al power up: {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[PowerUp] SistemaPowerUps.instancia es null en Start() para {gameObject.name}");
        }
    }
    
private void OnTriggerEnter2D(Collider2D collision)
{
    // Verifica si ya fue recogido (para evitar múltiples activaciones)
    if (yaRecogido) return;

    // Detecta si el objeto colisionante es un proyectil
    if (collision.CompareTag("Proyectil"))
    {
        yaRecogido = true; // Marca que ya ha sido recogido
        Debug.Log($"[PowerUp] Colisión detectada con proyectil: {collision.gameObject.name}");

        // Registra la colisión si es necesario (por ejemplo, para estadísticas)
        RegistrarColision();

        // Destruye el power-up sin afectar al proyectil
        Destroy(gameObject);
    }
}

    
    private void ActivarPowerUp(GameObject proyectil)
    {
        if (SistemaPowerUps.instancia == null)
        {
            Debug.LogError("[PowerUp] No se encontró instancia del SistemaPowerUps");
            return;
        }
        
        switch (tipoPowerUp)
        {
            case "MasaExtra":
                SistemaPowerUps.instancia.ActivarPowerUp(
                    tipoPowerUp, 
                    proyectil, 
                    duracion,
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            obj.name = $"{obj.name}_OrigMasa_{proy.masa}";
                            proy.masa *= 2f * SistemaPowerUps.instancia.coeficienteDebug;
                            Debug.Log($"[PowerUp] Masa aumentada a {proy.masa}");
                        }
                    },
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
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
                SistemaPowerUps.instancia.ActivarPowerUp(
                    tipoPowerUp, 
                    proyectil, 
                    duracion,
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            float radioOriginal = proy.radio;
                            obj.name = $"{obj.name}_OrigRadio_{radioOriginal}";
                            proy.radio *= 1.5f * SistemaPowerUps.instancia.coeficienteDebug;
                            obj.transform.localScale *= 1.5f;
                            Debug.Log($"[PowerUp] Radio aumentado a {proy.radio}");
                        }
                    },
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            string nombre = obj.name;
                            int indice = nombre.LastIndexOf("_OrigRadio_");
                            if (indice >= 0)
                            {
                                string radioStr = nombre.Substring(indice + 11);
                                if (float.TryParse(radioStr, out float radioOrig))
                                {
                                    proy.radio = radioOrig;
                                    obj.transform.localScale = new Vector3(1, 1, 1);
                                    obj.name = nombre.Substring(0, indice);
                                    Debug.Log($"[PowerUp] Radio restaurado a {proy.radio}");
                                }
                            }
                        }
                    }
                );
                break;

            case "Gravedad":
                SistemaPowerUps.instancia.ActivarPowerUp(
                    tipoPowerUp,
                    proyectil,
                    duracion,
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                            {
                                obj.name = $"{obj.name}_OrigColor_{ColorToString(renderer.color)}";
                                renderer.color = new Color(0.7f, 0.7f, 1.0f);
                            }
                            
                            if (SistemaFisica.instancia != null)
                            {
                                SistemaFisica.instancia.ActivarGravedad(obj, true, 0.2f * SistemaPowerUps.instancia.coeficienteDebug);
                            }
                            Debug.Log($"[PowerUp] Gravedad activada para {obj.name}");
                        }
                    },
                    (obj) => {
                        Proyectil proy = obj.GetComponent<Proyectil>();
                        if (proy != null)
                        {
                            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                            {
                                string nombre = obj.name;
                                int indice = nombre.LastIndexOf("_OrigColor_");
                                if (indice >= 0)
                                {
                                    string colorStr = nombre.Substring(indice + 11);
                                    renderer.color = StringToColor(colorStr);
                                    obj.name = nombre.Substring(0, indice);
                                }
                            }
                            
                            if (SistemaFisica.instancia != null)
                            {
                                SistemaFisica.instancia.ActivarGravedad(obj, false);
                            }
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
    
    private string ColorToString(Color color)
    {
        return $"{color.r}_{color.g}_{color.b}_{color.a}";
    }
    
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
        if (SistemaPowerUps.instancia != null)
        {
            tag = SistemaPowerUps.instancia.tagPowerUp;
        }

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = esTrigger;
        }

        PowerUpItem item = gameObject.GetComponent<PowerUpItem>();
        if (item == null)
        {
            item = gameObject.AddComponent<PowerUpItem>();
        }
        item.tipoPowerUp = tipoPowerUp;
        item.duracion = duracion;

        Debug.Log($"[PowerUp] Sprite configurado como power up: {gameObject.name}, tipo: {tipoPowerUp}, duración: {duracion}s");
    }
}