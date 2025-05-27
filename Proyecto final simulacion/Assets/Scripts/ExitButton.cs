using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [Header("Configuración del Botón")]
    [SerializeField] private Button exitButton;
    
    [Header("Nombres de las Escenas")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string level1SceneName = "Level1";
    
    void Start()
    {
        // Si no se asignó el botón en el inspector, buscar el componente Button en este GameObject
        if (exitButton == null)
            exitButton = GetComponent<Button>();
        
        // Asignar la función al evento OnClick del botón
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMenu);
        else
            Debug.LogError("No se encontró el componente Button. Asigna el botón en el inspector o añade este script a un GameObject con Button.");
    }
    
    /// <summary>
    /// Función que se ejecuta al hacer clic en el botón de salida
    /// </summary>
    public void ExitToMenu()
    {
        // Verificar si estamos en Level1
        Scene currentScene = SceneManager.GetActiveScene();
        
        if (currentScene.name == level1SceneName)
        {
            // Cargar la escena del menú
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning($"El botón de salida solo funciona desde la escena {level1SceneName}. Escena actual: {currentScene.name}");
        }
    }
    
    /// <summary>
    /// Función alternativa para usar con corrutinas si necesitas efectos de transición
    /// </summary>
    public void ExitToMenuWithTransition()
    {
        StartCoroutine(ExitWithFade());
    }
    
    private System.Collections.IEnumerator ExitWithFade()
    {
        // Aquí puedes añadir efectos de fade o transición
        // Por ejemplo, un fade out
        
        // Simular un pequeño delay para la transición
        yield return new WaitForSeconds(0.5f);
        
        // Cargar la escena del menú
        SceneManager.LoadScene(menuSceneName);
    }
    
    void OnDestroy()
    {
        // Limpiar el listener cuando el objeto se destruya
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitToMenu);
    }
}