using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Simple player death handler - shows game over screen with retry, menu, and quit buttons
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Header Collider")]
    [SerializeField] private CircleCollider2D playerHeader; // Drag the header circle collider here    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverCanvas; // Drag the game over canvas here
    [SerializeField] private TMPro.TextMeshProUGUI highScoreText; // Drag the high score text here (optional)
    [SerializeField] private TMPro.TextMeshProUGUI GameoverScore; // Drag the game over score text here
      [Header("Pause UI")]
    [SerializeField] private GameObject pauseCanvas; // Drag the pause canvas here
    [SerializeField] private Button pauseButton; // Drag the pause button here (to open pause menu)
    [SerializeField] private Button resumeButton; // Drag the resume button here
    [SerializeField] private Button pauseMenuButton; // Drag the pause menu button here
    [SerializeField] private Button pauseQuitButton; // Drag the pause quit button here
    
    [Header("Audio")]
    [SerializeField] private AudioClip deathAudio; // Drag the death sound here
    
    [Header("UI Buttons")]
    [SerializeField] private Button retryButton; // Drag the retry button here
    [SerializeField] private Button menuButton; // Drag the menu button here
    [SerializeField] private Button quitButton; // Drag the quit button here    [Header("Scene Settings")]
    [SerializeField] private string menuSceneName = "Scenes/Menu"; // Name of the menu scene to load
    
    [Header("Death Settings")]
    [SerializeField] private string[] deathTags = { "Untagged" }; // Temporary: Use existing tag for testing
      // Private variables
    private bool isDead = false;
    private bool isPaused = false;    private void Start()
    {
        SetupHeaderCollider();
        SetupButtons();
        
        // Make sure pause button is enabled at start
        if (pauseButton != null)
        {
            pauseButton.interactable = true;
            Debug.Log("Pause button enabled at start");
        }
        
        // Make sure game over canvas is hidden at start
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
            
            // Also disable the Canvas component if needed
            Canvas canvas = gameOverCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = false;
                Debug.Log("Game Over canvas disabled at start");
            }
        }
        
        // Make sure pause canvas is hidden at start
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
            
            // Also disable the Canvas component if needed
            Canvas pauseCanvasComponent = pauseCanvas.GetComponent<Canvas>();
            if (pauseCanvasComponent != null)
            {
                pauseCanvasComponent.enabled = false;
                Debug.Log("Pause canvas disabled at start");
            }
        }
        
        // Debug: Show if header collider is properly assigned
        if (playerHeader != null)
        {
            Debug.Log($"PlayerDeathHandler: Header collider assigned! Is Trigger: {playerHeader.isTrigger}");
        }
        else
        {
            Debug.LogError("PlayerDeathHandler: Header collider NOT assigned in inspector!");
        }
    }
      /// <summary>
    /// Setup the header collider for death detection
    /// </summary>
    private void SetupHeaderCollider()
    {
        if (playerHeader == null)
        {
            Debug.LogError("PlayerDeathHandler: Player header collider not assigned!");
            return;
        }
        
        // Debug info about the header collider
        Debug.Log($"Setting up header collider on object: {playerHeader.gameObject.name}");
        Debug.Log($"Header collider position: {playerHeader.transform.position}");
        Debug.Log($"Header collider is active: {playerHeader.gameObject.activeInHierarchy}");
        
        // Ensure the header collider is set as a trigger
        if (!playerHeader.isTrigger)
        {
            playerHeader.isTrigger = true;
            Debug.Log("Set header collider to trigger mode");
        }
        
        // Add the death trigger component to the header collider
        DeathTrigger deathTrigger = playerHeader.GetComponent<DeathTrigger>();
        if (deathTrigger == null)
        {
            deathTrigger = playerHeader.gameObject.AddComponent<DeathTrigger>();
            Debug.Log("Added DeathTrigger component to header collider");
        }
        else
        {
            Debug.Log("DeathTrigger component already exists on header collider");
        }
        
        deathTrigger.Initialize(this);
        
        Debug.Log("PlayerDeathHandler: Header collider setup complete!");
    }    /// <summary>
    /// Setup all the UI buttons
    /// </summary>
    private void SetupButtons()
    {
        // Game Over buttons
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RestartGame);
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(GoToMenu);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
          // Pause buttons
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseGame);
        }
        
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }
        
        if (pauseMenuButton != null)
        {
            pauseMenuButton.onClick.AddListener(GoToMenu);
        }
        
        if (pauseQuitButton != null)
        {
            pauseQuitButton.onClick.AddListener(QuitGame);
        }
    }
      /// <summary>
    /// Called when the header collider triggers with something
    /// </summary>
    /// <param name="other">The collider that triggered the death</param>
    public void OnHeaderTrigger(Collider2D other)
    {
        if (isDead) return; // Already dead
        
        // Check if the object has a death tag
        foreach (string deathTag in deathTags)
        {
            try
            {
                if (other.CompareTag(deathTag))
                {
                    Debug.Log($"Player head hit object with tag: {deathTag}");
                    TriggerGameOver();
                    return;
                }
            }
            catch (UnityException)
            {
                Debug.LogWarning($"Tag '{deathTag}' is not defined in Unity! Please add it in Project Settings → Tags and Layers.");
            }
        }
        
        // Debug: Show what tag the collider actually has
        Debug.Log($"Player head hit object with tag: {other.tag} (not a death tag)");
    }    /// <summary>
    /// Trigger game over
    /// </summary>
    private void TriggerGameOver()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log("Player died! Game Over!");
        
        // Disable pause button when dead
        if (pauseButton != null)
        {
            pauseButton.interactable = false;
            Debug.Log("Pause button disabled");
        }
        
        // Play death audio if assigned
        if (deathAudio != null)
        {
            AudioSource.PlayClipAtPoint(deathAudio, transform.position);
            Debug.Log("Death audio played");
        }
          // Show and enable game over canvas
        if (gameOverCanvas != null)
        {
            // Enable the canvas GameObject
            gameOverCanvas.SetActive(true);
            
            // Also enable the Canvas component if it's disabled
            Canvas canvas = gameOverCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                canvas.sortingOrder = 100; // Put it on top
                Debug.Log("Canvas component enabled and moved to front");
            }            // Update final score display
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.UpdateFinalScoreDisplay();
                
                // Update game over score display if we have a text field for it
                if (GameoverScore != null)
                {
                    int currentScore = ScoreManager.Instance.GetCurrentScore();
                    GameoverScore.text = $"Score: {currentScore}";
                    Debug.Log($"Game over score displayed: {currentScore}");
                }
                
                // Also update high score display if we have a text field for it
                if (highScoreText != null)
                {
                    int highScore = ScoreManager.Instance.GetHighScore();
                    highScoreText.text = $"High Score: {highScore}";
                    Debug.Log($"High score displayed: {highScore}");
                }
            }
            
            Debug.Log("Game Over canvas activated");
        }
        else
        {
            Debug.LogError("Game Over canvas is not assigned!");
        }
        
        // Stop the game after showing UI
        Time.timeScale = 0f;
    }
      /// <summary>
    /// Restart the game when retry button is pressed
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restarting game by reloading scene...");
        
        // Resume time before scene reload
        Time.timeScale = 1f;
        
        // Reload the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }    /// <summary>
    /// Go to the main menu when menu button is pressed
    /// </summary>
    public void GoToMenu()
    {
        Debug.Log("Going to menu...");
        
        // Try multiple scene loading methods
        bool sceneLoaded = false;
        
        // Method 1: Try loading by build index first (most reliable)
        try
        {
            Debug.Log("Trying to load menu scene by build index 1...");
            SceneManager.LoadScene(1); // Menu scene is at index 1
            sceneLoaded = true;
            Debug.Log("Successfully loaded scene by index 1");
            // Resume time ONLY after successful scene load
            Time.timeScale = 1f;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene by index 1: {e.Message}");
            sceneLoaded = false;
        }
        
        // Method 2: Try loading by scene name if index failed
        if (!sceneLoaded && !string.IsNullOrEmpty(menuSceneName))
        {
            try
            {
                Debug.Log($"Trying to load scene by name: {menuSceneName}");
                SceneManager.LoadScene(menuSceneName);
                sceneLoaded = true;
                Debug.Log($"Successfully loaded scene: {menuSceneName}");
                Time.timeScale = 1f;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load scene '{menuSceneName}': {e.Message}");
                sceneLoaded = false;
            }
        }
        
        // Method 3: Try loading just "Menu" if full path failed
        if (!sceneLoaded)
        {
            try
            {
                Debug.Log("Trying to load scene by name: Menu");
                SceneManager.LoadScene("Menu");
                sceneLoaded = true;
                Debug.Log("Successfully loaded scene: Menu");
                Time.timeScale = 1f;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load scene 'Menu': {e.Message}");
                sceneLoaded = false;
            }
        }
        
        // If all methods failed, show error and keep game paused
        if (!sceneLoaded)
        {
            Debug.LogError("All scene loading methods failed! Game will remain paused.");
            // Keep Time.timeScale = 0f so the game doesn't continue after death
            // The player will need to restart the application
        }
    }
    
    /// <summary>
    /// Quit the game when quit button is pressed
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // Resume time before quitting
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
            // If we're in the editor, stop playing
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // If we're in a build, quit the application
            Application.Quit();
        #endif
    }    /// <summary>
    /// Context menu method to test game over (only available in editor)
    /// </summary>
    [ContextMenu("Test Game Over")]
    private void TestGameOver()
    {
        TriggerGameOver();
    }
    
    /// <summary>
    /// Context menu method to test pause (only available in editor)
    /// </summary>
    [ContextMenu("Test Pause")]
    private void TestPause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }    private void Update()
    {
        // Handle ESC key for pause (only if not dead) - using Input System only
        if (!isDead && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESC key detected!");
            
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Pause the game when ESC is pressed
    /// </summary>
    public void PauseGame()
    {
        if (isDead) return; // Can't pause if dead
        
        isPaused = true;
        
        Debug.Log("Game paused");
        
        // Show and enable pause canvas
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(true);
            
            Canvas canvas = pauseCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                canvas.sortingOrder = 50; // Below game over but above game UI
                Debug.Log("Pause canvas enabled and moved to front");
            }
            
            Debug.Log("Pause canvas activated");
        }
        else
        {
            Debug.LogError("Pause canvas is not assigned!");
        }
        
        // Pause the game
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        
        Debug.Log("Game resumed");
        
        // Hide pause canvas
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
            
            Canvas canvas = pauseCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
        
        // Resume the game (only if not dead)
        if (!isDead)
        {
            Time.timeScale = 1f;
        }
    }
}

/// <summary>
/// Helper component that gets added to the header collider to detect triggers
/// </summary>
public class DeathTrigger : MonoBehaviour
{
    private PlayerDeathHandler deathHandler;
    
    public void Initialize(PlayerDeathHandler handler)
    {
        deathHandler = handler;
    }
      private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"DeathTrigger: Trigger detected with {other.name} (tag: {other.tag})");
        
        if (deathHandler != null)
        {
            deathHandler.OnHeaderTrigger(other);
        }
        else
        {
            Debug.LogError("DeathTrigger: No death handler assigned!");
        }
    }
}
