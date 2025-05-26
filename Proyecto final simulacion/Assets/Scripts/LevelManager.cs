using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Camera")]
    public Camera levelCamera;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    void Awake()
    {
        // Este script debe ejecutarse antes que cualquier otro
        Debug.Log($"=== LEVEL MANAGER INICIADO EN: {SceneManager.GetActiveScene().name} ===");
        
        // Limpiar cualquier objeto del menú que haya sobrevivido
        CleanupMenuObjects();
        
        // Configurar la cámara del nivel
        SetupLevelCamera();
        
        // Debug de la escena
        if (enableDebugLogs)
        {
            DebugSceneInfo();
        }
    }
    
    void CleanupMenuObjects()
    {
        // Buscar y destruir objetos del menú que puedan haber sobrevivido
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Destruir objetos que contengan componentes del menú
            if (obj.GetComponent<MenuController>() != null)
            {
                Debug.Log($"Destruyendo objeto del menú sobreviviente: {obj.name}");
                Destroy(obj);
            }
            
            // Destruir Canvas del menú si existe
            if (obj.name.Contains("MenuCanvas") || obj.name.Contains("UICanvas"))
            {
                Debug.Log($"Destruyendo canvas del menú: {obj.name}");
                Destroy(obj);
            }
        }
    }
    
    void SetupLevelCamera()
    {
        // Si no se asignó cámara manualmente, buscar la Main Camera
        if (levelCamera == null)
        {
            levelCamera = Camera.main;
            
            // Si aún no hay Main Camera, buscar cualquier cámara
            if (levelCamera == null)
            {
                levelCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (levelCamera != null)
        {
            // Asegurar que la cámara del nivel esté activa
            levelCamera.gameObject.SetActive(true);
            levelCamera.enabled = true;
            
            // Configurar para 2D si es necesario
            if (!levelCamera.orthographic)
            {
                Debug.Log("Configurando cámara para 2D");
                levelCamera.orthographic = true;
                levelCamera.orthographicSize = 5f;
            }
            
            // Asegurar posición correcta para 2D
            if (levelCamera.transform.position.z >= 0)
            {
                Vector3 pos = levelCamera.transform.position;
                pos.z = -10f;
                levelCamera.transform.position = pos;
            }
            
            Debug.Log($"Cámara del nivel configurada: {levelCamera.name}");
            Debug.Log($"Posición: {levelCamera.transform.position}");
            Debug.Log($"Ortográfica: {levelCamera.orthographic}, Tamaño: {levelCamera.orthographicSize}");
        }
        else
        {
            Debug.LogError("No se encontró cámara en el nivel! Creando una nueva...");
            CreateDefaultCamera();
        }
    }
    
    void CreateDefaultCamera()
    {
        GameObject cameraObj = new GameObject("Main Camera");
        levelCamera = cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";
        
        // Configurar para 2D
        levelCamera.orthographic = true;
        levelCamera.orthographicSize = 5f;
        levelCamera.transform.position = new Vector3(0, 0, -10);
        levelCamera.clearFlags = CameraClearFlags.SolidColor;
        levelCamera.backgroundColor = new Color(0.53f, 0.81f, 0.92f, 1f); // Azul cielo
        
        // Agregar AudioListener si no existe
        if (FindObjectOfType<AudioListener>() == null)
        {
            cameraObj.AddComponent<AudioListener>();
        }
        
        Debug.Log("Cámara por defecto creada");
    }
    
    void DebugSceneInfo()
    {
        Debug.Log($"=== INFO DE LA ESCENA: {SceneManager.GetActiveScene().name} ===");
        Debug.Log($"Cámaras en escena: {FindObjectsOfType<Camera>().Length}");
        Debug.Log($"Main Camera: {Camera.main != null}");
        Debug.Log($"AudioListeners: {FindObjectsOfType<AudioListener>().Length}");
        Debug.Log($"Total de GameObjects: {FindObjectsOfType<GameObject>().Length}");
        
        // Listar objetos principales
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        Debug.Log($"Objetos raíz en escena: {rootObjects.Length}");
        foreach (GameObject obj in rootObjects)
        {
            Debug.Log($"  - {obj.name}");
        }
    }
    
    // Método público para cambiar configuración de cámara si es necesario
    public void SetCameraSize(float newSize)
    {
        if (levelCamera != null && levelCamera.orthographic)
        {
            levelCamera.orthographicSize = newSize;
            Debug.Log($"Tamaño de cámara cambiado a: {newSize}");
        }
    }
    
    // Método para volver al menú (opcional)
    public void ReturnToMenu()
    {
        Debug.Log("Regresando al menú principal");
        SceneManager.LoadScene("menu");
    }
}