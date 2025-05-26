using UnityEngine;
using System;

/// <summary>
/// Versión simplificada del PowerUpItem para compatibilidad con el sistema anterior
/// Esta clase es compatible con power-ups que no usan el sistema aleatorio completo
/// </summary>
public class PowerUpItem1 : MonoBehaviour, IObjetoColisionable
{
    [Header("Configuración del Power-Up")]
    [Tooltip("Tipo del power-up - si está vacío, se asignará aleatoriamente")]
    public string tipoPowerUp = "";  // Vacío para asignación aleatoria
    
    [Tooltip("Duración del efecto - si es 0, se asignará aleatoriamente")]
    public float duracion = 0f;     // 0 para asignación aleatoria
    
    public event Action OnDestruido;         // Evento que se llama al destruir el power-up

    private bool yaRecogido = false;         // Bandera para evitar múltiples activaciones

    private void Start()
    {
        // Asignar tipo y duración aleatorios si no están configurados
        if (string.IsNullOrEmpty(tipoPowerUp) || duracion <= 0f)
        {
            AsignarAtributosAleatorios();
        }

        // Registrar este power-up en el sistema de físicas
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.RegistrarObjeto(gameObject, 1f, 0.5f); // Masa y radio predeterminados
        }

        Debug.Log($"[PowerUp] {gameObject.name} registrado en el sistema de físicas. Tipo: {tipoPowerUp}, Duración: {duracion}s");
    }

    /// <summary>
    /// Asigna tipo y duración aleatorios basándose en la configuración del SistemaPowerUps
    /// </summary>
    private void AsignarAtributosAleatorios()
    {
        if (SistemaPowerUps.instancia != null && 
            SistemaPowerUps.instancia.tiposPowerUp != null && 
            SistemaPowerUps.instancia.tiposPowerUp.Length > 0)
        {
            // Usar la lógica del sistema para seleccionar tipo aleatorio
            var tipos = SistemaPowerUps.instancia.tiposPowerUp;
            
            // Calcular probabilidades
            float sumaTotal = 0f;
            foreach (var tipo in tipos)
            {
                sumaTotal += tipo.probabilidad;
            }

            if (sumaTotal > 0f)
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
                        
                        // Aplicar color visual si tiene SpriteRenderer
                        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.color = tipo.color;
                        }
                        
                        // Cambiar nombre para identificar el tipo
                        gameObject.name = $"{gameObject.name}_{tipo.nombre}";
                        
                        Debug.Log($"[PowerUp] Atributos aleatorios asignados: {tipo.nombre} (Duración: {tipo.duracion}s, Color: {tipo.color})");
                        return;
                    }
                }
            }
            else
            {
                // Selección uniforme si no hay probabilidades válidas
                var tipoSeleccionado = tipos[UnityEngine.Random.Range(0, tipos.Length)];
                tipoPowerUp = tipoSeleccionado.nombre;
                duracion = tipoSeleccionado.duracion;
                
                // Aplicar color visual
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = tipoSeleccionado.color;
                }
                
                gameObject.name = $"{gameObject.name}_{tipoSeleccionado.nombre}";
            }
        }
        else
        {
            // Valores por defecto si no hay sistema configurado
            string[] tiposDisponibles = { "MasaExtra", "RadioExtra", "Gravedad" };
            float[] duracionesDisponibles = { 5f, 7f, 4f };
            Color[] coloresDisponibles = { Color.red, Color.yellow, Color.blue };
            
            int indiceAleatorio = UnityEngine.Random.Range(0, tiposDisponibles.Length);
            tipoPowerUp = tiposDisponibles[indiceAleatorio];
            duracion = duracionesDisponibles[indiceAleatorio];
            
            // Aplicar color
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = coloresDisponibles[indiceAleatorio];
            }
            
            gameObject.name = $"{gameObject.name}_{tipoPowerUp}";
            
            Debug.LogWarning($"[PowerUp] Sistema no disponible, usando valores por defecto: {tipoPowerUp}");
        }
    }

    private void OnDestroy()
    {
        // Invocar el evento OnDestruido y eliminar del sistema de físicas
        OnDestruido?.Invoke();
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.EliminarObjeto(gameObject);
        }

        Debug.Log($"[PowerUp] {gameObject.name} eliminado del sistema de físicas.");
    }

    public void OnColision(GameObject colisionador)
    {
        if (yaRecogido || colisionador == null) return;

        // Verificar si el objeto que colisionó es un proyectil
        if (colisionador.CompareTag("Proyectil"))
        {
            yaRecogido = true;  // Marcar el power-up como "recogido"
            Debug.Log($"[PowerUp] Colisión detectada entre {gameObject.name} (Tipo: {tipoPowerUp}) y proyectil: {colisionador.name}");

            // Activar el power-up en el proyectil
            ActivarPowerUp(colisionador);

            // Registrar o manejar la colisión (opcional, por ejemplo, estadísticas)
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

    private void RegistrarColision()
    {
        Debug.Log($"[PowerUp] Power-up {gameObject.name} (Tipo: {tipoPowerUp}) destruido tras colisión con proyectil.");
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