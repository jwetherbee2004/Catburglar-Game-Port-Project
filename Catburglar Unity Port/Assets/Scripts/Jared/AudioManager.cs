using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource musicSource;
    private Dictionary<string, AudioClip> levelMusicTracks = new Dictionary<string, AudioClip>();
    
    private string currentLevel;
    private float pausedMusicTime = 0f;
    private bool isMusicPaused = false;
    private Coroutine fadeCoroutine;

    [Header("Level Music Tracks")]
    [SerializeField] private AudioClip levelOneMusic;
    [SerializeField] private AudioClip levelTwoMusic;
    [SerializeField] private AudioClip levelThreeMusic;
    [SerializeField] private float fadeInDuration = 1f;

    private void Awake()
    {
        // Check if instance already exists FIRST
        if (Instance != null && Instance != this)
        {
            Debug.Log("[AudioManager] Instance already exists, destroying duplicate");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // If this is a child of another object, unparent it first
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        levelMusicTracks["LevelOne"] = levelOneMusic;
        levelMusicTracks["LevelTwo"] = levelTwoMusic;
        levelMusicTracks["LevelThree"] = levelThreeMusic;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        // Extra safety check - if a duplicate AudioManager is trying to enable, destroy it
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        if (sceneName == currentLevel && isMusicPaused)
        {
            ResumeMusicAfterReset();
        }
        else if (levelMusicTracks.ContainsKey(sceneName))
        {
            PlayLevelMusic(sceneName);
        }
        else
        {
        }
    }

    public void PlayLevelMusic(string levelName)
    {
        if (!levelMusicTracks.ContainsKey(levelName))
        {
            return;
        }

        if (levelName == currentLevel && musicSource.isPlaying)
        {
            return;
        }

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        currentLevel = levelName;
        musicSource.clip = levelMusicTracks[levelName];
        musicSource.time = 0f;
        musicSource.volume = 1f;
        musicSource.Play();
        isMusicPaused = false;
        pausedMusicTime = 0f;
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            pausedMusicTime = musicSource.time;
            musicSource.Pause();
            isMusicPaused = true;
        }
        else
        {
            Debug.LogWarning("[AudioManager] Cannot pause music - music source is not playing!");
        }
    }

    public void ResumeMusic()
    {
        if (isMusicPaused && musicSource.clip != null)
        {
            musicSource.time = pausedMusicTime;
            musicSource.Play();
            isMusicPaused = false;
        }
    }

    private void ResumeMusicAfterReset()
    {
        if (musicSource.clip != null)
        {
            // Stop any existing fade coroutine
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            musicSource.time = pausedMusicTime;
            musicSource.volume = 0f; // Start silent
            musicSource.Play();
            isMusicPaused = false;
            fadeCoroutine = StartCoroutine(FadeInMusic());
        }
        else
        {
            Debug.LogWarning("[AudioManager] Cannot resume - music source has no clip!");
        }
    }

    private IEnumerator FadeInMusic()
    {
        float elapsed = 0f;
        float targetVolume = 1f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    public void StopMusic()
    {
        musicSource.Stop();
        isMusicPaused = false;
    }

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }
}
