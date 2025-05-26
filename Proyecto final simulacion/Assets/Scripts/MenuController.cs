using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("Video Player")]
    public VideoPlayer videoPlayer;
    
    [Header("Video Clips")]
    public VideoClip interfazIn;
    public VideoClip interfazOut;
    public VideoClip levelsIn;
    public VideoClip levelsOut;
    
    [Header("UI Elements")]
    public GameObject playButton;
    public GameObject levelButtons;
    public GameObject exitButton;
    public GameObject confirmationPanel;
    public Button confirmExitButton;
    public Button cancelExitButton;
    
    [Header("Confirmation Messages")]
    public Text confirmationText;
    [SerializeField] private string[] confirmationMessages = {
        "¿Seguro que quieres irte? ¡La aventura apenas comienza!",
        "¿Ya te vas? ¡Quédate un poco más, hay mucho por descubrir!",
        "¿Realmente quieres salir? ¡Te esperan increíbles desafíos!",
        "¿Abandonas tan pronto? ¡Los mejores momentos están por venir!",
        "¿Te marchas? ¡Hay secretos esperando ser descubiertos!"
    };
    
    [Header("Scene Names")]
    public string[] levelSceneNames = {"Level1", "Level2", "Level3"};
    
    [Header("Canvas Management")]
    public string[] canvasesToHideNames = {"ManejoTurnos"}; // Nombres de los canvas que deben ocultarse
    
    private enum MenuState
    {
        InterfazIn,
        MainMenu,
        InterfazOut,
        LevelsIn,
        LevelSelection,
        LevelsOut,
        Loading
    }
    
    private MenuState currentState;
    private bool canInteract = false;
    
    void Start()
    {
        // Verificar que estamos en la escena correcta del menú
        if (SceneManager.GetActiveScene().name != "menu")
        {
            Debug.Log("MenuController detectado en escena que no es menu - Destruyendo");
            Destroy(gameObject);
            return;
        }
        
        // Ocultar canvas problemáticos al inicio
        HideProblematicCanvas();
        
        // Configurar video player
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        
        // Inicializar UI
        playButton.SetActive(false);
        levelButtons.SetActive(false);
        exitButton.SetActive(false);
        confirmationPanel.SetActive(false);
        
        // Configurar eventos de botones
        SetupButtons();
        
        // Iniciar secuencia
        StartCoroutine(PlayInterfazIn());
    }
    
    void HideProblematicCanvas()
    {
        // Buscar y ocultar los canvas problemáticos en todas las escenas cargadas
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObj in rootObjects)
                {
                    foreach (string canvasName in canvasesToHideNames)
                    {
                        // Buscar por nombre exacto
                        GameObject foundCanvas = FindInChildren(rootObj, canvasName);
                        if (foundCanvas != null)
                        {
                            Canvas canvas = foundCanvas.GetComponent<Canvas>();
                            if (canvas != null)
                            {
                                canvas.enabled = false;
                                Debug.Log($"Canvas '{canvasName}' ocultado temporalmente");
                            }
                            else
                            {
                                foundCanvas.SetActive(false);
                                Debug.Log($"GameObject '{canvasName}' desactivado temporalmente");
                            }
                        }
                    }
                }
            }
        }
    }
    
    GameObject FindInChildren(GameObject parent, string name)
    {
        if (parent.name == name)
            return parent;
        
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject result = FindInChildren(parent.transform.GetChild(i).gameObject, name);
            if (result != null)
                return result;
        }
        
        return null;
    }
    
    void SetupButtons()
    {
        // Configurar botón principal
        Button mainBtn = playButton.GetComponent<Button>();
        if (mainBtn != null)
        {
            // Limpiar listeners existentes
            mainBtn.onClick.RemoveAllListeners();
            mainBtn.onClick.AddListener(OnPlayButtonClicked);
            Debug.Log("Botón principal configurado correctamente");
        }
        else
        {
            Debug.LogError("No se encontró componente Button en PlayButton!");
        }
        
        // Configurar botón de salida
        Button exitBtn = exitButton.GetComponent<Button>();
        if (exitBtn != null)
        {
            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(OnExitButtonClicked);
            Debug.Log("Botón de salida configurado correctamente");
        }
        
        // Configurar botones de confirmación
        if (confirmExitButton != null)
        {
            confirmExitButton.onClick.RemoveAllListeners();
            confirmExitButton.onClick.AddListener(ConfirmExit);
        }
        
        if (cancelExitButton != null)
        {
            cancelExitButton.onClick.RemoveAllListeners();
            cancelExitButton.onClick.AddListener(CancelExit);
        }
        
        // Configurar botones de niveles
        Button[] levelBtns = levelButtons.GetComponentsInChildren<Button>();
        Debug.Log($"Encontrados {levelBtns.Length} botones de nivel");
        
        for (int i = 0; i < levelBtns.Length; i++)
        {
            int levelIndex = i; // Captura local para el closure
            levelBtns[i].onClick.RemoveAllListeners();
            levelBtns[i].onClick.AddListener(() => OnLevelSelected(levelIndex));
        }
    }
    
    IEnumerator PlayInterfazIn()
    {
        Debug.Log("Reproduciendo Interfaz_in");
        currentState = MenuState.InterfazIn;
        canInteract = false;
        
        // Configurar y reproducir video
        videoPlayer.clip = interfazIn;
        videoPlayer.Prepare();
        
        // Esperar a que el video esté listo
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        videoPlayer.Play();
        
        // Esperar a que termine el video
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        // Pausar en el último frame y mostrar botón
        videoPlayer.Pause();
        ShowMainMenu();
    }
    
    void ShowMainMenu()
    {
        Debug.Log("Mostrando menú principal");
        currentState = MenuState.MainMenu;
        canInteract = true;
        playButton.SetActive(true);
        
        // Debug adicional
        Debug.Log($"PlayButton activo: {playButton.activeInHierarchy}");
        Button btn = playButton.GetComponent<Button>();
        if (btn != null)
        {
            Debug.Log($"Botón interactuable: {btn.interactable}");
        }
    }
    
    public void OnPlayButtonClicked()
    {
        Debug.Log($"OnPlayButtonClicked llamado - Estado: {currentState}, CanInteract: {canInteract}");
        
        if (currentState == MenuState.MainMenu && canInteract)
        {
            Debug.Log("Botón Play presionado - Iniciando transición");
            StartCoroutine(TransitionToLevels());
        }
        else
        {
            Debug.Log($"Click ignorado - Estado: {currentState}, CanInteract: {canInteract}");
        }
    }
    
    IEnumerator TransitionToLevels()
    {
        canInteract = false;
        playButton.SetActive(false);
        
        // Reproducir animación de salida del menú principal
        Debug.Log("Reproduciendo Interfaz_out");
        currentState = MenuState.InterfazOut;
        
        videoPlayer.clip = interfazOut;
        videoPlayer.Prepare();
        
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        videoPlayer.Play();
        
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        // Reproducir animación de entrada a selección de niveles
        Debug.Log("Reproduciendo Levels_in");
        currentState = MenuState.LevelsIn;
        
        videoPlayer.clip = levelsIn;
        videoPlayer.Prepare();
        
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        videoPlayer.Play();
        
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        // Mostrar selección de niveles
        videoPlayer.Pause();
        ShowLevelSelection();
    }
    
    void ShowLevelSelection()
    {
        Debug.Log("Mostrando selección de niveles");
        currentState = MenuState.LevelSelection;
        canInteract = true;
        levelButtons.SetActive(true);
        exitButton.SetActive(true); // Aquí aparece el botón de salir
    }
    
    public void OnExitButtonClicked()
    {
        Debug.Log("Botón Exit presionado");
        
        if (currentState == MenuState.LevelSelection && canInteract)
        {
            ShowExitConfirmation();
        }
    }
    
    void ShowExitConfirmation()
    {
        Debug.Log("Mostrando confirmación de salida");
        canInteract = false;
        
        // Ocultar botones de selección de niveles
        levelButtons.SetActive(false);
        exitButton.SetActive(false);
        
        // Mostrar panel de confirmación con mensaje aleatorio
        confirmationPanel.SetActive(true);
        
        if (confirmationText != null && confirmationMessages.Length > 0)
        {
            string randomMessage = confirmationMessages[Random.Range(0, confirmationMessages.Length)];
            confirmationText.text = randomMessage;
            Debug.Log($"Mensaje mostrado: {randomMessage}");
        }
        
        canInteract = true;
    }
    
    public void ConfirmExit()
    {
        Debug.Log("Confirmando salida del juego");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void CancelExit()
    {
        Debug.Log("Cancelando salida - Volviendo a selección de niveles");
        
        // Ocultar panel de confirmación
        confirmationPanel.SetActive(false);
        
        // Volver a la selección de niveles
        ShowLevelSelection();
    }
    
    public void OnLevelSelected(int levelIndex)
    {
        if (currentState == MenuState.LevelSelection && canInteract)
        {
            Debug.Log($"Nivel {levelIndex + 1} seleccionado");
            StartCoroutine(LoadLevel(levelIndex));
        }
    }
    
    IEnumerator LoadLevel(int levelIndex)
    {
        canInteract = false;
        levelButtons.SetActive(false);
        exitButton.SetActive(false);
        
        // Reproducir animación de salida de selección de niveles
        Debug.Log("Reproduciendo Levels_out");
        currentState = MenuState.LevelsOut;
        
        videoPlayer.clip = levelsOut;
        videoPlayer.Prepare();
        
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        videoPlayer.Play();
        
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        // Cargar escena del nivel
        Debug.Log($"Cargando escena: {levelSceneNames[levelIndex]}");
        currentState = MenuState.Loading;
        
        // Opcional: Agregar fade out o loading screen aquí
        yield return new WaitForSeconds(0.5f);
        
        // Cargar la escena de forma asíncrona para mejor control
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelSceneNames[levelIndex]);
        asyncLoad.allowSceneActivation = false;
        
        // Esperar a que la escena esté lista
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Destruir el MenuController antes de activar la nueva escena
        Debug.Log("Destruyendo MenuController antes de cargar nivel");
        
        // Activar la nueva escena
        asyncLoad.allowSceneActivation = true;
        
        // Esperar un frame adicional para asegurar que la escena esté completamente cargada
        yield return new WaitForEndOfFrame();
        
        // Ahora mostrar los canvas que estaban ocultos en la nueva escena
        StartCoroutine(ShowCanvasAfterLevelLoad());
        
        // Destruir este GameObject para evitar conflictos
        Destroy(gameObject);
    }
    
    IEnumerator ShowCanvasAfterLevelLoad()
    {
        // Esperar un poco más para asegurar que todo esté completamente inicializado
        yield return new WaitForSeconds(0.1f);
        
        // Buscar y mostrar los canvas que estaban ocultos
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != "menu")
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObj in rootObjects)
                {
                    foreach (string canvasName in canvasesToHideNames)
                    {
                        GameObject foundCanvas = FindInChildren(rootObj, canvasName);
                        if (foundCanvas != null)
                        {
                            Canvas canvas = foundCanvas.GetComponent<Canvas>();
                            if (canvas != null)
                            {
                                canvas.enabled = true;
                                Debug.Log($"Canvas '{canvasName}' reactivado en la escena del nivel");
                            }
                            else
                            {
                                foundCanvas.SetActive(true);
                                Debug.Log($"GameObject '{canvasName}' reactivado en la escena del nivel");
                            }
                        }
                    }
                }
            }
        }
    }
    
    // Método de debugging para saltar videos (opcional)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }
        
        // Debug de raycast
        if (Input.GetMouseButtonDown(0))
        {
            DebugRaycast();
        }
        
        // Método de emergencia - click directo
        if (currentState == MenuState.MainMenu && canInteract)
        {
            if (Input.GetKeyDown(KeyCode.Return)) // Enter key
            {
                Debug.Log("Enter presionado - forzando transición");
                OnPlayButtonClicked();
            }
        }
        
        // Tecla ESC para manejar salida
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == MenuState.LevelSelection && canInteract)
            {
                OnExitButtonClicked();
            }
        }
    }
    
    void DebugRaycast()
    {
        Vector2 mousePos = Input.mousePosition;
        
        // Hacer raycast y ver qué está bloqueando
        UnityEngine.EventSystems.PointerEventData eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventData.position = mousePos;
        
        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);
        
        Debug.Log($"Mouse position: {mousePos}");
        Debug.Log($"Objetos detectados en raycast: {results.Count}");
        
        for (int i = 0; i < results.Count; i++)
        {
            Debug.Log($"  {i}: {results[i].gameObject.name} (depth: {results[i].depth})");
        }
    }
}