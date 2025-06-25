using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu controller with background music, buttons, and settings
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioClip backgroundMusic;
    
    [Header("Main Menu UI")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Options Menu UI")]
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider overallVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI overallVolumeText;
    
    [Header("Settings")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    
    // Volume values
    [Range(0f, 1f)]
    [SerializeField] private float currentMusicVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float currentOverallVolume = 1.0f;
    
    // Volume settings keys for PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string OVERALL_VOLUME_KEY = "OverallVolume";
    
    // Default volume values
    private const float DEFAULT_MUSIC_VOLUME = 0.7f;
    private const float DEFAULT_OVERALL_VOLUME = 1.0f;
    
    private void Start()
    {
        // Validate UI references
        ValidateUIReferences();
        
        // Setup button listeners
        SetupButtons();
        
        // Setup audio
        SetupBackgroundMusic();
        
        // Load saved volume settings
        LoadVolumeSettings();
        
        // Setup volume sliders
        SetupVolumeSliders();
        
        // Make sure time scale is normal (in case we came from paused game)
        Time.timeScale = 1f;
    }
    
    private void ValidateUIReferences()
    {
        // Check main menu canvas
        if (mainMenuCanvas == null)
            Debug.LogError("Main Menu Canvas is not assigned in the inspector!");
        
        // Check options canvas
        if (optionsCanvas == null)
            Debug.LogError("Options Canvas is not assigned in the inspector!");
        
        // Check buttons
        if (startButton == null)
            Debug.LogWarning("Start Button is not assigned in the inspector!");
        
        if (optionsButton == null)
            Debug.LogWarning("Options Button is not assigned in the inspector!");
        
        if (quitButton == null)
            Debug.LogWarning("Quit Button is not assigned in the inspector!");
        
        if (backButton == null)
            Debug.LogWarning("Back Button is not assigned in the inspector!");
        
        // Check sliders
        if (musicVolumeSlider == null)
            Debug.LogWarning("Music Volume Slider is not assigned in the inspector!");
        
        if (overallVolumeSlider == null)
            Debug.LogWarning("Overall Volume Slider is not assigned in the inspector!");
    }
    
    private void SetupButtons()
    {
        // Main menu buttons
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        if (optionsButton != null)
            optionsButton.onClick.AddListener(ShowOptions);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        // Options menu buttons
        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);
    }
    
    private void SetupBackgroundMusic()
    {
        // Check if AudioManager exists and use it
        if (AudioManager.Instance != null)
        {
            if (backgroundMusic != null)
            {
                AudioManager.Instance.PlayMusic(backgroundMusic, true);
                Debug.Log("Background music started via AudioManager");
            }
        }
        else
        {
            // Fallback: Create AudioSource if not assigned
            if (backgroundMusicSource == null)
            {
                backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure background music
            if (backgroundMusic != null)
            {
                backgroundMusicSource.clip = backgroundMusic;
                backgroundMusicSource.loop = true;
                backgroundMusicSource.playOnAwake = true;
                backgroundMusicSource.Play();
                Debug.Log("Background music started (fallback mode)");
            }
            else
            {
                Debug.LogWarning("Background music clip not assigned!");
            }
        }
    }
    
    private void SetupVolumeSliders()
    {
        // Music volume slider
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        // Overall volume slider
        if (overallVolumeSlider != null)
        {
            overallVolumeSlider.minValue = 0f;
            overallVolumeSlider.maxValue = 1f;
            overallVolumeSlider.onValueChanged.AddListener(OnOverallVolumeChanged);
        }
        
        // Update UI to show current values
        UpdateVolumeUI();
    }
    
    private void LoadVolumeSettings()
    {
        float musicVolume, overallVolume;
        
        // Check if AudioManager exists and get volumes from it
        if (AudioManager.Instance != null)
        {
            musicVolume = AudioManager.Instance.GetMusicVolume();
            overallVolume = AudioManager.Instance.GetOverallVolume();
        }
        else
        {
            // Fallback: Load from PlayerPrefs directly
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
            overallVolume = PlayerPrefs.GetFloat(OVERALL_VOLUME_KEY, DEFAULT_OVERALL_VOLUME);
        }
        
        // Apply loaded volumes
        SetMusicVolume(musicVolume);
        SetOverallVolume(overallVolume);
        
        Debug.Log($"Loaded volume settings - Music: {musicVolume:F2}, Overall: {overallVolume:F2}");
    }
    
    private void SaveVolumeSettings()
    {
        // Save current volume settings using the stored variables
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, currentMusicVolume);
        PlayerPrefs.SetFloat(OVERALL_VOLUME_KEY, currentOverallVolume);
        
        PlayerPrefs.Save();
        Debug.Log($"Volume settings saved - Music: {currentMusicVolume:F2}, Overall: {currentOverallVolume:F2}");
    }
    
    #region Volume Control Methods
    
    private void OnMusicVolumeChanged(float volume)
    {
        SetMusicVolume(volume);
        UpdateVolumeUI();
        SaveVolumeSettings();
        
        // Update AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
    }
    
    private void OnOverallVolumeChanged(float volume)
    {
        SetOverallVolume(volume);
        UpdateVolumeUI();
        SaveVolumeSettings();
        
        // Update AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetOverallVolume(volume);
        }
    }
    
    private void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        currentMusicVolume = volume;
        
        // Update music volume slider
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = volume;
        
        // Apply to background music
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = volume;
        
        // Apply to all AudioSources with "Music" tag or specific naming
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource.gameObject.name.ToLower().Contains("music") ||
                audioSource.gameObject.CompareTag("Music"))
            {
                audioSource.volume = volume;
            }
        }
    }
    
    private void SetOverallVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        currentOverallVolume = volume;
        
        // Update overall volume slider
        if (overallVolumeSlider != null)
            overallVolumeSlider.value = volume;
        
        // Apply overall volume to AudioListener
        AudioListener.volume = volume;
    }
    
    private void UpdateVolumeUI()
    {
        // Update music volume percentage text
        if (musicVolumeText != null && musicVolumeSlider != null)
        {
            int percentage = Mathf.RoundToInt(musicVolumeSlider.value * 100);
            musicVolumeText.text = $"{percentage}%";
        }
        
        // Update overall volume percentage text
        if (overallVolumeText != null && overallVolumeSlider != null)
        {
            int percentage = Mathf.RoundToInt(overallVolumeSlider.value * 100);
            overallVolumeText.text = $"{percentage}%";
        }
    }
    
    #endregion
    
    #region Button Actions
    
    public void StartGame()
    {
        Debug.Log("Starting game...");
        
        // Make sure time scale is normal
        Time.timeScale = 1f;
        
        // Load gameplay scene
        SceneManager.LoadScene(gameplaySceneName);
    }
    
    public void ShowOptions()
    {
        Debug.Log("Showing options menu");
        
        // Show options menu
        if (optionsCanvas != null)
        {
            optionsCanvas.SetActive(true);
            
            // Also make sure the Canvas component is enabled
            Canvas canvas = optionsCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Force overlay mode
                canvas.sortingOrder = 100; // Put options on top
                Debug.Log($"Options canvas enabled with sorting order: {canvas.sortingOrder}");
                Debug.Log($"Options canvas render mode: {canvas.renderMode}");
            }
            
            // Check if the canvas has a CanvasScaler
            CanvasScaler scaler = optionsCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Debug.Log($"Canvas scaler UI scale mode: {scaler.uiScaleMode}");
            }
            
            // Log the canvas position and size
            RectTransform rectTransform = optionsCanvas.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"Options canvas position: {rectTransform.anchoredPosition}");
                Debug.Log($"Options canvas size: {rectTransform.sizeDelta}");
            }
        }
        else
        {
            Debug.LogError("Options Canvas is not assigned!");
        }
        
        // Update volume UI to show current settings
        UpdateVolumeUI();
        
        // Additional debug: Check if sliders are active
        if (musicVolumeSlider != null)
        {
            Debug.Log($"Music slider active: {musicVolumeSlider.gameObject.activeInHierarchy}");
            Debug.Log($"Music slider position: {musicVolumeSlider.transform.position}");
        }
        if (overallVolumeSlider != null)
        {
            Debug.Log($"Overall slider active: {overallVolumeSlider.gameObject.activeInHierarchy}");
            Debug.Log($"Overall slider position: {overallVolumeSlider.transform.position}");
        }
        
        // Check if back button is active
        if (backButton != null)
        {
            Debug.Log($"Back button active: {backButton.gameObject.activeInHierarchy}");
            Debug.Log($"Back button position: {backButton.transform.position}");
        }
        
        // Log all child objects of the options canvas
        if (optionsCanvas != null)
        {
            Debug.Log($"Options canvas has {optionsCanvas.transform.childCount} children:");
            for (int i = 0; i < optionsCanvas.transform.childCount; i++)
            {
                Transform child = optionsCanvas.transform.GetChild(i);
                Debug.Log($"  Child {i}: {child.name} - Active: {child.gameObject.activeInHierarchy}");
            }
        }
    }
    
    public void ShowMainMenu()
    {
        Debug.Log("Showing main menu");
        
        // Hide options menu
        if (optionsCanvas != null)
        {
            optionsCanvas.SetActive(false);
            Debug.Log("Options canvas hidden");
        }
        
        // Save settings when going back to main menu
        SaveVolumeSettings();
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // Save settings before quitting
        SaveVolumeSettings();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Save settings when app is paused
            SaveVolumeSettings();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Save settings when app loses focus
            SaveVolumeSettings();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (startButton != null)
            startButton.onClick.RemoveAllListeners();
        
        if (optionsButton != null)
            optionsButton.onClick.RemoveAllListeners();
        
        if (quitButton != null)
            quitButton.onClick.RemoveAllListeners();
        
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
        
        if (overallVolumeSlider != null)
            overallVolumeSlider.onValueChanged.RemoveAllListeners();
    }
    
    #endregion
    
    #region Public Methods for Testing
    
    /// <summary>
    /// Public method to set music volume (for external scripts or testing)
    /// </summary>
    /// <param name="volume">Volume from 0 to 1</param>
    public void SetMusicVolumePublic(float volume)
    {
        SetMusicVolume(volume);
        UpdateVolumeUI();
        SaveVolumeSettings();
    }
    
    /// <summary>
    /// Public method to set overall volume (for external scripts or testing)
    /// </summary>
    /// <param name="volume">Volume from 0 to 1</param>
    public void SetOverallVolumePublic(float volume)
    {
        SetOverallVolume(volume);
        UpdateVolumeUI();
        SaveVolumeSettings();
    }
    
    /// <summary>
    /// Get current music volume
    /// </summary>
    /// <returns>Music volume from 0 to 1</returns>
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
    }
    
    /// <summary>
    /// Get current overall volume
    /// </summary>
    /// <returns>Overall volume from 0 to 1</returns>
    public float GetOverallVolume()
    {
        return PlayerPrefs.GetFloat(OVERALL_VOLUME_KEY, DEFAULT_OVERALL_VOLUME);
    }
    
    #endregion
    
    // Legacy methods for backward compatibility
    private void Play()
    {
        StartGame();
    }

    private void Quit()
    {
        QuitGame();
    }
}
