using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeToMenu : MonoBehaviour
{
    [Header("Nombres de las Escenas")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string level1SceneName = "Level1";
    
    [Header("Configuración")]
    [SerializeField] private bool enableEscapeKey = true;
    
    void Update()
    {
        // Verificar si se presiona la tecla Escape
        if (enableEscapeKey && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitToMenu();
        }
    }
    
    /// <summary>
    /// Función que maneja la salida al menú principal
    /// </summary>
    public void ExitToMenu()
    {
        // Verificar si estamos en Level1
        Scene currentScene = SceneManager.GetActiveScene();
        
        if (currentScene.name == level1SceneName)
        {
            Debug.Log("Regresando al menú principal...");
            
            // Marcar que venimos de un nivel (si usas la Opción 1)
            // MenuController.SetReturningFromLevel();
            
            // Cargar la escena del menú de forma completa (reinicia todo)
            SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning($"La función de escape solo funciona desde la escena {level1SceneName}. Escena actual: {currentScene.name}");
        }
    }
    
    /// <summary>
    /// Función para habilitar/deshabilitar la funcionalidad de Escape
    /// </summary>
    public void SetEscapeEnabled(bool enabled)
    {
        enableEscapeKey = enabled;
    }
    
    /// <summary>
    /// Función alternativa con transición suave
    /// </summary>
    public void ExitToMenuWithTransition()
    {
        if (enableEscapeKey)
        {
            StartCoroutine(ExitWithDelay());
        }
    }
    
    private System.Collections.IEnumerator ExitWithDelay()
    {
        Debug.Log("Iniciando transición al menú...");
        
        // Aquí puedes añadir efectos como fade out, sonidos, etc.
        // Por ejemplo, pausar el juego brevemente
        Time.timeScale = 0.1f;
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Restaurar la escala de tiempo
        Time.timeScale = 1f;
        
        // Cargar la escena del menú
        SceneManager.LoadScene(menuSceneName);
    }
}