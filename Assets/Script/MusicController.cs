using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicController : MonoBehaviour
{
    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private string menuSceneName = "MainMenu";

    [Header("Game Music")]
    [SerializeField] private AudioClip[] gameMusicTracks;
    [SerializeField] private string gameSceneName = "Game";
    private AudioClip currentTrack;
    private bool isSwitchingTrack;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForCurrentScene();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == gameSceneName &&
            AudioManager.Instance != null &&
            !AudioManager.Instance.IsMusicPlaying &&
            currentTrack != null &&
            !isSwitchingTrack)
        {
            Debug.Log("Current track finished, playing a new random track.");
            PlayRandomGameTrack();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Current scene: {currentScene}, Menu Scene Name: {menuSceneName}, Game Scene Name: {gameSceneName}");

        if (currentScene == menuSceneName)
        {
            if (menuMusic != null)
            {
                Debug.Log("Playing menu music.");
                AudioManager.Instance.PlayMusic(menuMusic, true);
            }
            else
            {
                Debug.LogWarning("Menu music is not assigned!");
            }
        }
        else if (currentScene == gameSceneName)
        {
            Debug.Log("Attempting to play game music.");
            PlayRandomGameTrack();
        }
        else
        {
            Debug.Log("Stopping music for unknown scene.");
            AudioManager.Instance.StopMusic(true);
        }
    }

    private void PlayRandomGameTrack()
    {
        if (gameMusicTracks.Length == 0)
        {
            Debug.LogWarning("No game music tracks assigned!");
            return;
        }

        isSwitchingTrack = true;

        AudioClip newTrack;
        do
        {
            newTrack = gameMusicTracks[Random.Range(0, gameMusicTracks.Length)];
        } while (newTrack == currentTrack && gameMusicTracks.Length > 1);

        currentTrack = newTrack;
        Debug.Log($"Playing game track: {currentTrack.name}");
        AudioManager.Instance.PlayMusic(currentTrack, true);

        isSwitchingTrack = false;
    }
}