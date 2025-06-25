using UnityEngine;

/// <summary>
/// Global Audio Manager that persists across scenes and manages volume settings
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float overallVolume = 1.0f;
    
    // Volume settings keys for PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string OVERALL_VOLUME_KEY = "OverallVolume";
    
    // Default volume values
    private const float DEFAULT_MUSIC_VOLUME = 0.7f;
    private const float DEFAULT_OVERALL_VOLUME = 1.0f;
    
    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize audio sources if not assigned
            SetupAudioSources();
            
            // Load saved volume settings
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupAudioSources()
    {
        // Create music source if not assigned
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        // Create SFX source if not assigned
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFXSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }
    
    private void LoadVolumeSettings()
    {
        // Load saved volumes
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
        overallVolume = PlayerPrefs.GetFloat(OVERALL_VOLUME_KEY, DEFAULT_OVERALL_VOLUME);
        
        // Apply the loaded volumes
        ApplyVolumeSettings();
        
        Debug.Log($"AudioManager: Loaded volumes - Music: {musicVolume:F2}, Overall: {overallVolume:F2}");
    }
    
    private void ApplyVolumeSettings()
    {
        // Apply music volume
        if (musicSource != null)
            musicSource.volume = musicVolume;
        
        // Apply overall volume to AudioListener
        AudioListener.volume = overallVolume;
        
        // Update all existing AudioSources in the scene
        UpdateAllAudioSources();
    }
    
    private void UpdateAllAudioSources()
    {
        // Find all AudioSources and apply appropriate volume settings
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            // Skip our own audio sources to avoid double-setting
            if (audioSource == musicSource || audioSource == sfxSource)
                continue;
            
            // Apply music volume to sources tagged as music
            if (audioSource.gameObject.name.ToLower().Contains("music") ||
                audioSource.gameObject.CompareTag("Music"))
            {
                audioSource.volume = musicVolume;
            }
        }
    }
    
    #region Public Volume Control Methods
    
    /// <summary>
    /// Set the music volume
    /// </summary>
    /// <param name="volume">Volume from 0 to 1</param>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        
        // Apply to music source
        if (musicSource != null)
            musicSource.volume = musicVolume;
        
        // Apply to all music-tagged AudioSources
        UpdateAllAudioSources();
        
        // Save the setting
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"AudioManager: Music volume set to {musicVolume:F2}");
    }
    
    /// <summary>
    /// Set the overall volume
    /// </summary>
    /// <param name="volume">Volume from 0 to 1</param>
    public void SetOverallVolume(float volume)
    {
        overallVolume = Mathf.Clamp01(volume);
        
        // Apply to AudioListener
        AudioListener.volume = overallVolume;
        
        // Save the setting
        PlayerPrefs.SetFloat(OVERALL_VOLUME_KEY, overallVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"AudioManager: Overall volume set to {overallVolume:F2}");
    }
    
    /// <summary>
    /// Get current music volume
    /// </summary>
    /// <returns>Music volume from 0 to 1</returns>
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    /// <summary>
    /// Get current overall volume
    /// </summary>
    /// <returns>Overall volume from 0 to 1</returns>
    public float GetOverallVolume()
    {
        return overallVolume;
    }
    
    #endregion
    
    #region Music Playback Methods
    
    /// <summary>
    /// Play background music
    /// </summary>
    /// <param name="musicClip">The music clip to play</param>
    /// <param name="loop">Whether to loop the music</param>
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
            
            Debug.Log($"AudioManager: Playing music - {musicClip.name}");
        }
    }
    
    /// <summary>
    /// Stop background music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            Debug.Log("AudioManager: Music stopped");
        }
    }
    
    /// <summary>
    /// Pause background music
    /// </summary>
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
            Debug.Log("AudioManager: Music paused");
        }
    }
    
    /// <summary>
    /// Resume background music
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
            Debug.Log("AudioManager: Music resumed");
        }
    }
    
    /// <summary>
    /// Check if music is currently playing
    /// </summary>
    /// <returns>True if music is playing</returns>
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }
    
    #endregion
    
    #region SFX Methods
    
    /// <summary>
    /// Play a sound effect
    /// </summary>
    /// <param name="sfxClip">The sound effect to play</param>
    /// <param name="volumeScale">Volume scale (0-1) relative to overall volume</param>
    public void PlaySFX(AudioClip sfxClip, float volumeScale = 1f)
    {
        if (sfxSource != null && sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip, volumeScale);
        }
    }
    
    /// <summary>
    /// Play a sound effect at a specific position
    /// </summary>
    /// <param name="sfxClip">The sound effect to play</param>
    /// <param name="position">World position to play the sound</param>
    /// <param name="volumeScale">Volume scale (0-1) relative to overall volume</param>
    public void PlaySFXAtPosition(AudioClip sfxClip, Vector3 position, float volumeScale = 1f)
    {
        if (sfxClip != null)
        {
            AudioSource.PlayClipAtPoint(sfxClip, position, volumeScale);
        }
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Save settings when app is paused
            SaveSettings();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Save settings when app loses focus
            SaveSettings();
        }
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(OVERALL_VOLUME_KEY, overallVolume);
        PlayerPrefs.Save();
        Debug.Log("AudioManager: Settings saved");
    }
    
    #endregion
}
