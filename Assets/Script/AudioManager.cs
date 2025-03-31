using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private AudioSource sfxSource;
    private AudioSource musicSource;
    [SerializeField] private float globalSfxVolume = 1f;
    [SerializeField] private float globalMusicVolume = 1f;
    [SerializeField] private float fadeDuration = 1f;
    private bool isFading; // ‘лаг дл€ предотвращени€ множественных затуханий

    public bool IsMusicPlaying => musicSource.isPlaying;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
    }

    public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip != null)
        {
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume * globalSfxVolume);
        }
    }

    public void PlayMusic(AudioClip clip, bool fade = true)
    {
        if (clip == null) return;

        Debug.Log($"PlayMusic called with clip: {clip.name}, fade: {fade}");

        if (musicSource.isPlaying && fade)
        {
            if (!isFading) // ѕровер€ем, не выполн€етс€ ли уже затухание
            {
                StartCoroutine(FadeOutAndPlay(clip));
            }
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = globalMusicVolume;
            musicSource.Play();
        }
    }

    public void StopMusic(bool fade = true)
    {
        if (musicSource.isPlaying)
        {
            Debug.Log("StopMusic called");
            if (fade)
            {
                if (!isFading)
                {
                    StartCoroutine(FadeOut());
                }
            }
            else
            {
                musicSource.Stop();
            }
        }
    }

    private IEnumerator FadeOutAndPlay(AudioClip newClip)
    {
        isFading = true;
        Debug.Log($"Fading out current track to play: {newClip.name}");

        float startVolume = musicSource.volume;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, globalMusicVolume, elapsed / fadeDuration);
            yield return null;
        }

        isFading = false;
        Debug.Log($"Finished fading in: {newClip.name}");
    }

    private IEnumerator FadeOut()
    {
        isFading = true;
        Debug.Log("Fading out current track");

        float startVolume = musicSource.volume;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        isFading = false;
        Debug.Log("Finished fading out");
    }
}