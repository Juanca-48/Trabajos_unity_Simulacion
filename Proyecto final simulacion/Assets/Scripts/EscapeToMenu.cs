using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeToMenu : MonoBehaviour
{
    private string previousScene = "";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene != "menu")
            {
                // Guardamos la escena actual antes de ir al menú
                previousScene = currentScene;
                SceneManager.LoadScene("menu", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync(currentScene);
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "menu")
        {
            // Aseguramos que el sistema de input esté habilitado en el menú
            // y que la escena previa no interfiera.
            SceneManager.SetActiveScene(scene);
        }
    }
}
