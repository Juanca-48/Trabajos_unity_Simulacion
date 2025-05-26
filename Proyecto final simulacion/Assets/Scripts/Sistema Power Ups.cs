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
    public class TipoPowerUp
    {
        [Tooltip("Nombre del tipo de power-up")]
        public string nombre;
        [Tooltip("Probabilidad de spawn (peso relativo)")]
        [Range(0f, 100f)]
        public float probabilidad = 33.3f;
        [Tooltip("Duración del efecto en segundos")]
        public float duracion = 5f;
        [Tooltip("Color del power-up")]
        public Color color = Color.white;
        [Tooltip("Descripción del efecto")]
        public string descripcion;
    }

    [Tooltip("Tipos de power-ups disponibles con sus configuraciones")]
    public TipoPowerUp[] tiposPowerUp = new TipoPowerUp[]
    {
        new TipoPowerUp { nombre = "MasaExtra", probabilidad = 33.3f, duracion = 5f, color = Color.red, descripcion = "Duplica la masa del proyectil" },
        new TipoPowerUp { nombre = "RadioExtra", probabilidad = 33.3f, duracion = 5f, color = Color.yellow, descripcion = "Aumenta el radio del proyectil" },
        new TipoPowerUp { nombre = "Gravedad", probabilidad = 33.4f, duracion = 5f, color = Color.blue, descripcion = "Activa gravedad en el proyectil" }
    };

    [Header("Configuración de Tags")]
    [Tooltip("Tag que deben tener los objetos para ser considerados power-ups")]
    public string tagPowerUp = "Aumento";

    [Header("Configuración de Física")]
    [Tooltip("Masa de los power-ups para el sistema de física")]
    public float masaPowerUp = 1f;
    [Tooltip("Radio de los power-ups para el sistema de física")]
    public float radioPowerUp = 0.5f;
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

        // Validar tipos de power-up
        if (tiposPowerUp == null || tiposPowerUp.Length == 0)
        {
            Debug.LogError("[PowerUp] No hay tipos de power-up configurados");
            configValida = false;
        }
        else
        {
            float sumaProb = 0f;
            foreach (var tipo in tiposPowerUp)
            {
                if (string.IsNullOrEmpty(tipo.nombre))
                {
                    Debug.LogError("[PowerUp] Hay un tipo de power-up sin nombre");
                    configValida = false;
                }
                sumaProb += tipo.probabilidad;
            }
            
            if (Mathf.Approximately(sumaProb, 0f))
            {
                Debug.LogError("[PowerUp] La suma de probabilidades de los power-ups no puede ser 0");
                configValida = false;
            }
        }

        // Validar que el sistema de física esté disponible
        if (SistemaFisica.instancia == null)
        {
            Debug.LogError("[PowerUp] SistemaFisica.instancia no está disponible");
            configValida = false;
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
    /// Selecciona un tipo de power-up aleatorio basado en las probabilidades configuradas
    /// </summary>
    private TipoPowerUp SeleccionarTipoAleatorio()
    {
        if (tiposPowerUp == null || tiposPowerUp.Length == 0)
        {
            Debug.LogError("[PowerUp] No hay tipos de power-up configurados");
            return null;
        }

        // Calcular la suma total de probabilidades
        float sumaTotal = 0f;
        foreach (var tipo in tiposPowerUp)
        {
            sumaTotal += tipo.probabilidad;
        }

        if (sumaTotal <= 0f)
        {
            Debug.LogWarning("[PowerUp] Suma de probabilidades es 0, seleccionando tipo aleatorio uniforme");
            return tiposPowerUp[UnityEngine.Random.Range(0, tiposPowerUp.Length)];
        }

        // Generar número aleatorio
        float valorAleatorio = UnityEngine.Random.Range(0f, sumaTotal);
        
        // Seleccionar tipo basado en probabilidades
        float acumulado = 0f;
        foreach (var tipo in tiposPowerUp)
        {
            acumulado += tipo.probabilidad;
            if (valorAleatorio <= acumulado)
            {
                return tipo;
            }
        }

        // Fallback - devolver el último tipo
        return tiposPowerUp[tiposPowerUp.Length - 1];
    }

    /// <summary>
    /// Crea un nuevo power up en el mundo de juego con tipo aleatorio
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

        // Seleccionar tipo de power-up aleatorio
        TipoPowerUp tipoSeleccionado = SeleccionarTipoAleatorio();
        if (tipoSeleccionado == null)
        {
            Debug.LogError("[PowerUp] No se pudo seleccionar un tipo de power-up");
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
            
            // Configurar el power up con el tipo seleccionado
            ConfigurarPowerUp(powerUp, tipoSeleccionado);
            
            Debug.Log($"[PowerUp] Spawneado exitosamente: {powerUp.name} (Tipo: {tipoSeleccionado.nombre}) en {posicionSpawn}. Total en mundo: {powerUpsEnMundo.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PowerUp] Error al spawnear power up: {e.Message}");
        }
    }

    private void ConfigurarPowerUp(GameObject powerUp, TipoPowerUp tipo)
    {
        // Asegurarse de que tenga el tag correcto
        powerUp.tag = tagPowerUp;
        
        // Asegurarse de que tenga el componente PowerUpItem
        PowerUpItem powerUpItem = powerUp.GetComponent<PowerUpItem>();
        if (powerUpItem == null)
        {
            powerUpItem = powerUp.AddComponent<PowerUpItem>();
        }

        // Configurar el tipo y duración aleatorios
        powerUpItem.tipoPowerUp = tipo.nombre;
        powerUpItem.duracion = tipo.duracion;
        powerUpItem.masa = masaPowerUp;
        powerUpItem.radio = radioPowerUp;

        // Aplicar color visual si tiene SpriteRenderer
        SpriteRenderer spriteRenderer = powerUp.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = tipo.color;
        }

        // Cambiar el nombre para identificar el tipo
        powerUp.name = $"{powerUp.name}_{tipo.nombre}";

        // Eliminar cualquier collider 2D existente ya que usaremos el sistema de física personalizado
        Collider2D[] colliders = powerUp.GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            DestroyImmediate(collider);
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

        Debug.Log($"[PowerUp] Power-up configurado: {powerUp.name} - Tipo: {tipo.nombre}, Duración: {tipo.duracion}s, Color: {tipo.color}");
    }

    /// <summary>
    /// Obtiene información sobre un tipo de power-up específico
    /// </summary>
    public TipoPowerUp ObtenerTipoPowerUp(string nombreTipo)
    {
        if (tiposPowerUp == null) return null;

        foreach (var tipo in tiposPowerUp)
        {
            if (tipo.nombre == nombreTipo)
                return tipo;
        }
        return null;
    }

    /// <summary>
    /// Fuerza el spawn de un power-up de un tipo específico (útil para testing)
    /// </summary>
    public void SpawnPowerUpTipo(string nombreTipo)
    {
        TipoPowerUp tipo = ObtenerTipoPowerUp(nombreTipo);
        if (tipo == null)
        {
            Debug.LogError($"[PowerUp] Tipo '{nombreTipo}' no encontrado");
            return;
        }

        // Usar la misma lógica de spawn pero con tipo forzado
        if (prefabsPowerUp == null || prefabsPowerUp.Length == 0 || 
            puntosPosiblesSpawn == null || puntosPosiblesSpawn.Length == 0)
        {
            Debug.LogError("[PowerUp] Configuración inválida para spawn forzado");
            return;
        }

        GameObject prefab = prefabsPowerUp[0]; // Usar el primer prefab disponible
        Transform punto = puntosPosiblesSpawn[UnityEngine.Random.Range(0, puntosPosiblesSpawn.Length)];
        
        GameObject powerUp = Instantiate(prefab, punto.position, Quaternion.identity);
        ConfigurarPowerUp(powerUp, tipo);
        
        Debug.Log($"[PowerUp] Spawn forzado de tipo '{nombreTipo}' completado");
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

/// <summary>
/// Componente PowerUpItem integrado con el sistema de física personalizado
/// </summary>
public class PowerUpItem : MonoBehaviour, IObjetoColisionable
{
    [Header("Configuración del Power-Up")]
    [Tooltip("Tipo de power up (se asigna automáticamente al spawn)")]
    public string tipoPowerUp = "PowerUp";
    
    [Tooltip("Duración del efecto en segundos (se asigna automáticamente al spawn)")]
    public float duracion = 5f;
    
    [Header("Configuración de Física")]
    [Tooltip("Masa del power-up en el sistema de física")]
    public float masa = 1f;
    
    [Tooltip("Radio del power-up en el sistema de física")]
    public float radio = 0.5f;
    
    [Tooltip("¿Ya fue recogido? (para evitar múltiples activaciones)")]
    private bool yaRecogido = false;
    
    public event Action OnDestruido;
    
    private void Start()
    {
        // Validar que el sistema de física esté disponible
        if (SistemaFisica.instancia == null)
        {
            Debug.LogError($"[PowerUp] SistemaFisica.instancia no está disponible para {gameObject.name}");
            return;
        }

        // Registrar este power-up en el sistema de física personalizado
        SistemaFisica.instancia.RegistrarObjeto(gameObject, masa, radio);
        
        // Configurar tag si el sistema de power-ups está disponible
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

        Debug.Log($"[PowerUp] {gameObject.name} registrado en el sistema de física con masa {masa} y radio {radio}. Tipo: {tipoPowerUp}, Duración: {duracion}s");
    }
    
    private void OnDestroy()
    {
        // Invocar el evento OnDestruido y eliminar del sistema de física
        OnDestruido?.Invoke();
        
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.EliminarObjeto(gameObject);
        }

        Debug.Log($"[PowerUp] {gameObject.name} eliminado del sistema de física");
    }

    /// <summary>
    /// Método que se ejecuta cuando ocurre una colisión dentro del sistema de física personalizado
    /// </summary>
    /// <param name="otroObjeto">El objeto con el que colisionó este power-up</param>
    public void OnColision(GameObject otroObjeto)
    {
        if (yaRecogido || otroObjeto == null) return;

        // Verificar si el objeto que colisionó es un proyectil
        if (otroObjeto.CompareTag("Proyectil"))
        {
            yaRecogido = true;  // Marcar el power-up como "recogido"
            Debug.Log($"[PowerUp] Colisión detectada entre {gameObject.name} (Tipo: {tipoPowerUp}) y proyectil: {otroObjeto.name}");

            // Activar el power-up en el proyectil
            ActivarPowerUp(otroObjeto);

            // Registrar la colisión (opcional, por ejemplo, estadísticas)
            RegistrarColision();

            // Destruir el power-up
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
                            Debug.Log($"[PowerUp] Masa aumentada a {proy.masa} (Duración: {duracion}s)");
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
                            Debug.Log($"[PowerUp] Radio aumentado a {proy.radio} (Duración: {duracion}s)");
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
                            Debug.Log($"[PowerUp] Gravedad activada para {obj.name} (Duración: {duracion}s)");
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
    
    /// <summary>
    /// Método opcional para registrar estadísticas o eventos de colisión
    /// </summary>
    private void RegistrarColision()
    {
        Debug.Log($"[PowerUp] Power-up {gameObject.name} (Tipo: {tipoPowerUp}) destruido tras colisión con proyectil");
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

/// <summary>
/// Componente para configurar sprites como power-ups usando el sistema de física personalizado
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpSprite : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    [Tooltip("Tipo de power up (se asignará automáticamente si está vacío)")]
    public string tipoPowerUp = "";

    [Tooltip("Duración del efecto en segundos (se asignará automáticamente si es 0)")]
    public float duracion = 0f;

    [Header("Configuración de Física")]
    [Tooltip("Masa del power-up en el sistema de física")]
    public float masa = 1f;
    
    [Tooltip("Radio del power-up en el sistema de física")]
    public float radio = 0.5f;

    void Start()
    {
        // Si no tiene tipo o duración configurados, asignar aleatorios
        if (string.IsNullOrEmpty(tipoPowerUp) || duracion <= 0f)
        {
            AsignarTipoAleatorio();
        }

        // Configurar tag
        if (SistemaPowerUps.instancia != null)
        {
            tag = SistemaPowerUps.instancia.tagPowerUp;
        }

        // Eliminar cualquier collider existente
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            DestroyImmediate(collider);
        }

        // Agregar o configurar el componente PowerUpItem
        PowerUpItem item = gameObject.GetComponent<PowerUpItem>();
        if (item == null)
        {
            item = gameObject.AddComponent<PowerUpItem>();
        }
        
        item.tipoPowerUp = tipoPowerUp;
        item.duracion = duracion;
        item.masa = masa;
        item.radio = radio;

        Debug.Log($"[PowerUp] Sprite configurado como power up: {gameObject.name}, tipo: {tipoPowerUp}, duración: {duracion}s, masa: {masa}, radio: {radio}");
    }

    /// <summary>
    /// Asigna un tipo aleatorio basado en la configuración del SistemaPowerUps
    /// </summary>
    private void AsignarTipoAleatorio()
    {
        if (SistemaPowerUps.instancia == null || 
            SistemaPowerUps.instancia.tiposPowerUp == null || 
            SistemaPowerUps.instancia.tiposPowerUp.Length == 0)
        {
            // Valores por defecto si no hay sistema configurado
            tipoPowerUp = "MasaExtra";
            duracion = 5f;
            Debug.LogWarning($"[PowerUp] No se encontró configuración de tipos, usando valores por defecto para {gameObject.name}");
            return;
        }

        // Seleccionar tipo aleatorio usando la misma lógica que el sistema
        var tipos = SistemaPowerUps.instancia.tiposPowerUp;
        
        // Calcular probabilidades
        float sumaTotal = 0f;
        foreach (var tipo in tipos)
        {
            sumaTotal += tipo.probabilidad;
        }

        if (sumaTotal <= 0f)
        {
            // Selección uniforme si no hay probabilidades válidas
            var tipoSeleccionado = tipos[UnityEngine.Random.Range(0, tipos.Length)];
            tipoPowerUp = tipoSeleccionado.nombre;
            duracion = tipoSeleccionado.duracion;
        }
        else
        {
            // Selección basada en probabilidades
            float valorAleatorio = UnityEngine.Random.Range(0f, sumaTotal);
            float acumulado = 0f;
            
            foreach (var tipo in tipos)
            {
                acumulado += tipo.probabilidad;
                if (valorAleatorio <= acumulado)
                {
                    tipoPowerUp = tipo.nombre;
                    duracion = tipo.duracion;
                    
                    // Aplicar color visual
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = tipo.color;
                    }
                    break;
                }
            }
        }

        Debug.Log($"[PowerUp] Tipo aleatorio asignado a {gameObject.name}: {tipoPowerUp} (Duración: {duracion}s)");
    }
}