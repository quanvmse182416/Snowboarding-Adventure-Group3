using UnityEngine;
using TMPro;

/// <summary>
/// Manages the scoring system for collecting stars
/// </summary>
public class ScoreManager : MonoBehaviour
{    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText; // Drag the in-game score text here
    [SerializeField] private TextMeshProUGUI finalScoreText; // Drag the final score text (game over screen) here
    [SerializeField] private TextMeshProUGUI highScoreText; // Drag the high score text here
    
    [Header("Audio")]
    [SerializeField] private AudioClip starAudio; // Drag the star collection sound here
    
    [Header("Star Settings")]
    [SerializeField] private string starTag = "Star"; // Tag for star objects
      // Private variables
    private int currentScore = 0;
    private int highScore = 0;
    private const string HIGH_SCORE_KEY = "HighScore"; // PlayerPrefs key for saving high score
    
    // Static instance for easy access from other scripts
    public static ScoreManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern - only one ScoreManager should exist
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }      private void Start()
    {
        // Load high score from PlayerPrefs
        LoadHighScore();
        
        // Initialize score display
        UpdateScoreDisplay();
        UpdateHighScoreDisplay();
        
        // Setup obstacle manager for automatic obstacle handling
        SetupObstacleManager();
        
        Debug.Log($"ScoreManager initialized - High Score: {highScore}");
    }
    
    /// <summary>
    /// Called when player collects a star
    /// </summary>
    /// <param name="starObject">The star object that was collected</param>
    public void CollectStar(GameObject starObject)
    {
        // Increase score
        currentScore++;
        
        // Play star collection audio
        if (starAudio != null)
        {
            AudioSource.PlayClipAtPoint(starAudio, starObject.transform.position);
            Debug.Log("Star audio played");
        }
        
        // Update score display
        UpdateScoreDisplay();
        
        // Destroy the star
        Destroy(starObject);
        
        Debug.Log($"Star collected! Score: {currentScore}");
    }
    
    /// <summary>
    /// Update the score text display
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }
      /// <summary>
    /// Update the final score display (called on game over)
    /// </summary>
    public void UpdateFinalScoreDisplay()
    {
        // Check if this is a new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            UpdateHighScoreDisplay();
            Debug.Log($"NEW HIGH SCORE: {highScore}");
        }
        
        if (finalScoreText != null)
        {
            string scoreMessage = currentScore > PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0) ? 
                $"NEW HIGH SCORE!\nFinal Score: {currentScore}" : 
                $"Final Score: {currentScore}";
            
            finalScoreText.text = scoreMessage;
            Debug.Log($"Final score displayed: {currentScore}");
        }
    }
    
    /// <summary>
    /// Get the current score
    /// </summary>
    /// <returns>Current score value</returns>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// Get the current high score
    /// </summary>
    /// <returns>Current high score value</returns>
    public int GetHighScore()
    {
        return highScore;
    }
    
    /// <summary>
    /// Reset the score (useful for restarting)
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
        Debug.Log("Score reset to 0");
    }
    
    /// <summary>
    /// Context menu method to test star collection
    /// </summary>
    [ContextMenu("Test Star Collection")]
    private void TestStarCollection()
    {
        CollectStar(this.gameObject);
    }
    
    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        Debug.Log($"High score loaded: {highScore}");
    }
    
    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
        Debug.Log($"High score saved: {highScore}");
    }
    
    /// <summary>
    /// Update the high score text display
    /// </summary>
    private void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    /// <summary>
    /// Get the star tag used for collision detection
    /// </summary>
    /// <returns>The star tag string</returns>
    public string GetStarTag()
    {
        return starTag;
    }
      /// <summary>
    /// Set the star tag used for collision detection
    /// </summary>
    /// <param name="newStarTag">The new star tag</param>
    public void SetStarTag(string newStarTag)
    {
        starTag = newStarTag;
        Debug.Log($"Star tag changed to: {starTag}");
    }
    
    /// <summary>
    /// Mark the level as won (called by Flag script)
    /// </summary>
    public void SetLevelWon()
    {
        Debug.Log("Level marked as won in ScoreManager");
        // You can add additional win logic here if needed
        // For example, saving level completion, unlocking next level, etc.
    }
    
    /// <summary>
    /// Check if player has won the level
    /// </summary>
    /// <returns>True if level is complete</returns>
    public bool HasWonLevel()
    {
        // You can implement win condition logic here
        // For now, this can be expanded based on your needs
        return false;
    }
    
    /// <summary>
    /// Initialize obstacle manager for automatic setup
    /// </summary>
    private void SetupObstacleManager()
    {
        // Check if ObstacleManager is already attached
        ObstacleManager obstacleManager = GetComponent<ObstacleManager>();
        if (obstacleManager == null)
        {
            // Add ObstacleManager component
            obstacleManager = gameObject.AddComponent<ObstacleManager>();
            Debug.Log("ScoreManager: Added ObstacleManager component for automatic obstacle setup");
        }
    }
    
    /// <summary>
    /// Context menu method to add ObstacleManager (for manual setup)
    /// </summary>
    [ContextMenu("Add Obstacle Manager")]
    private void AddObstacleManager()
    {
        SetupObstacleManager();
    }
      /// <summary>
    /// Set the speed reduction multiplier for all obstacles
    /// </summary>
    /// <param name="multiplier">Speed reduction multiplier (0.5 = 50% reduction, 0.8 = 20% reduction)</param>
    public void SetObstacleSpeedReductionMultiplier(float multiplier)
    {
        ObstacleManager obstacleManager = GetComponent<ObstacleManager>();
        if (obstacleManager != null)
        {
            obstacleManager.SetSpeedReductionMultiplierForAllObstacles(multiplier);
            Debug.Log($"ScoreManager: Set obstacle speed reduction multiplier to {multiplier}");
        }
        else
        {
            Debug.LogWarning("ScoreManager: No ObstacleManager found to set speed reduction multiplier");
        }
    }
    
    /// <summary>
    /// Set the minimum speed after collision for all obstacles
    /// </summary>
    /// <param name="minSpeed">Minimum speed after collision to prevent complete stop</param>
    public void SetObstacleMinSpeed(float minSpeed)
    {
        ObstacleManager obstacleManager = GetComponent<ObstacleManager>();
        if (obstacleManager != null)
        {
            obstacleManager.SetMinSpeedAfterCollisionForAllObstacles(minSpeed);
            Debug.Log($"ScoreManager: Set obstacle min speed to {minSpeed}");
        }
        else
        {
            Debug.LogWarning("ScoreManager: No ObstacleManager found to set min speed");
        }
    }
    
    /// <summary>
    /// Enable or disable obstacle speed maintenance
    /// </summary>
    /// <param name="enabled">Whether to maintain player speed on obstacles</param>
    public void SetObstacleSpeedMaintenance(bool enabled)
    {
        ObstacleManager obstacleManager = GetComponent<ObstacleManager>();
        if (obstacleManager != null)
        {
            obstacleManager.SetPlayerSpeedMaintenanceForAllObstacles(enabled);
            Debug.Log($"ScoreManager: Set obstacle speed maintenance to {enabled}");
        }
        else
        {
            Debug.LogWarning("ScoreManager: No ObstacleManager found to set speed maintenance");
        }
    }
    
    /// <summary>
    /// Context menu methods for easy testing
    /// </summary>
    [ContextMenu("Set Speed: Conservative (1.0x, 15 max)")]
    private void SetConservativeSpeed()
    {
        SetObstacleSpeedReductionMultiplier(0.7f); // 30% reduction (conservative)
        SetObstacleMinSpeed(3f);
    }
    
    [ContextMenu("Set Speed: Balanced (0.5x reduction, 2 min)")]
    private void SetBalancedSpeed()
    {
        SetObstacleSpeedReductionMultiplier(0.5f); // 50% reduction (balanced)
        SetObstacleMinSpeed(2f);
    }
    
    [ContextMenu("Set Speed: Aggressive (0.3x reduction, 1 min)")]
    private void SetFastSpeed()
    {
        SetObstacleSpeedReductionMultiplier(0.3f); // 70% reduction (aggressive)
        SetObstacleMinSpeed(1f);
    }
    
    /// <summary>
    /// Debug method to check obstacle setup status
    /// </summary>
    [ContextMenu("Debug: Check Obstacle Setup")]
    private void DebugObstacleSetup()
    {
        ObstacleManager obstacleManager = GetComponent<ObstacleManager>();
        if (obstacleManager == null)
        {
            Debug.LogError("ScoreManager: No ObstacleManager component found!");
            return;
        }
        
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        Debug.Log($"ScoreManager Debug: Found {obstacles.Length} objects with 'Obstacle' tag");
        
        int obstaclesWithScript = 0;
        foreach (GameObject obstacle in obstacles)
        {
            ObstaclePlayerSpeed script = obstacle.GetComponent<ObstaclePlayerSpeed>();
            if (script != null)
            {
                obstaclesWithScript++;
                Debug.Log($"ScoreManager Debug: {obstacle.name} has ObstaclePlayerSpeed script");
            }
            else
            {
                Debug.LogWarning($"ScoreManager Debug: {obstacle.name} is tagged 'Obstacle' but has no ObstaclePlayerSpeed script!");
            }
        }
        
        Debug.Log($"ScoreManager Debug: {obstaclesWithScript}/{obstacles.Length} obstacles have ObstaclePlayerSpeed script");
    }
    
    /// <summary>
    /// Force re-setup of all obstacles
    /// </summary>
    [ContextMenu("Debug: Force Obstacle Re-setup")]
    private void ForceObstacleResetup()
    {
        ObstacleManager obstacleManager = GetComponent<ObstacleManager>();
        if (obstacleManager != null)
        {
            // Force a complete re-setup
            obstacleManager.RemoveAllObstacleScripts();
            obstacleManager.SetupAllObstacles();
            Debug.Log("ScoreManager: Forced complete obstacle re-setup");
        }
        else
        {
            Debug.LogError("ScoreManager: No ObstacleManager to force re-setup");
        }
    }
}
