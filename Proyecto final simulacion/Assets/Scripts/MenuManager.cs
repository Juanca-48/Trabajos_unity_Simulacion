// Este script gestiona la secuencia de videos para la interfaz de inicio
// Colócalo en un GameObject vacío llamado "MenuManager" en la escena de inicio

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public GameObject playButton;
    public GameObject levelSelector; // UI de selección de nivel (puede ser panel)

    public VideoClip interfazIn;
    public VideoClip interfazOut;
    public VideoClip levelsIn;
    public VideoClip levelsOut;

    private enum MenuState { Intro, Idle, TransitionToLevels, LevelSelection, TransitionToGame }
    private MenuState currentState = MenuState.Intro;

    void Start()
    {
        Debug.Log("START"); 
        playButton.SetActive(false);
        levelSelector.SetActive(false);

        videoPlayer.loopPointReached += OnVideoEnd;
        PlayVideo(interfazIn);
    }

    void PlayVideo(VideoClip clip)
    {
        Debug.Log("Reproduciendo: " + clip.name);
        videoPlayer.clip = clip;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        switch (currentState)
        {
            case MenuState.Intro:
                currentState = MenuState.Idle;
                //videoDisplay.texture = vp.texture;
                playButton.SetActive(true);
                break;

            case MenuState.TransitionToLevels:
                currentState = MenuState.LevelSelection;
                PlayVideo(levelsIn);
                break;

            case MenuState.LevelSelection:
                //videoDisplay.texture = vp.texture;
                levelSelector.SetActive(true);
                break;

            case MenuState.TransitionToGame:
                SceneManager.LoadScene("SimpleScene");
                break;
        }
    }

    public void OnPlayButtonClicked()
    {
        currentState = MenuState.TransitionToLevels;
        playButton.SetActive(false);
        PlayVideo(interfazOut);
    }

    public void OnLevelSelected()
    {
        currentState = MenuState.TransitionToGame;
        levelSelector.SetActive(false);
        PlayVideo(levelsOut);
    }
}
