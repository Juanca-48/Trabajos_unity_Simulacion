using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    
    [Header("Scene Names")]
    public string[] levelSceneNames = {"Level1", "Level2", "Level3"};
    
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
        // Configurar video player
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        
        // Inicializar UI
        playButton.SetActive(false);
        levelButtons.SetActive(false);
        
        // Configurar eventos de botones
        SetupButtons();
        
        // Iniciar secuencia
        StartCoroutine(PlayInterfazIn());
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
        
        SceneManager.LoadScene(levelSceneNames[levelIndex]);
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
    }
}