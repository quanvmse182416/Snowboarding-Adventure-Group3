using UnityEngine;

/// <summary>
/// Manager that automatically applies ObstaclePlayerSpeed to all obstacles in the scene
/// Attach this to any GameObject in the scene (like the ScoreManager)
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [Header("Auto-Setup Settings")]
    [SerializeField] private bool autoSetupOnStart = true; // Automatically setup all obstacles at start
    [SerializeField] private string obstacleTag = "Obstacle"; // Tag to search for obstacles
      [Header("Player Speed Settings")]
    [SerializeField] private bool enablePlayerSpeedMaintenance = false; // Enable old speed maintenance behavior (false = new speed reduction behavior)
    [SerializeField] private float speedReductionMultiplier = 0.5f; // Speed reduction when hitting obstacles (0.5 = 50% reduction, 0.8 = 20% reduction)
    [SerializeField] private float minSpeedAfterCollision = 2f; // Minimum speed after collision to prevent complete stop
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // Show debug information
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupAllObstacles();
        }
    }
    
    /// <summary>
    /// Find all obstacles and add ObstaclePlayerSpeed script to them
    /// </summary>
    [ContextMenu("Setup All Obstacles")]
    public void SetupAllObstacles()
    {
        // Find all GameObjects with the obstacle tag
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        
        if (obstacles.Length == 0)
        {
            if (showDebugLogs)
                Debug.LogWarning($"ObstacleManager: No GameObjects found with tag '{obstacleTag}'");
            return;
        }
        
        int setupCount = 0;
        
        foreach (GameObject obstacle in obstacles)
        {
            // Remove old ObstacleGroundSpeed script if it exists
            ObstacleGroundSpeed oldScript = obstacle.GetComponent<ObstacleGroundSpeed>();
            if (oldScript != null)
            {
                DestroyImmediate(oldScript);
                if (showDebugLogs)
                    Debug.Log($"ObstacleManager: Removed old ObstacleGroundSpeed from {obstacle.name}");
            }
            
            // Check if the obstacle already has the ObstaclePlayerSpeed script
            ObstaclePlayerSpeed existingScript = obstacle.GetComponent<ObstaclePlayerSpeed>();
              if (existingScript == null)
            {
                // Add the ObstaclePlayerSpeed script
                ObstaclePlayerSpeed newScript = obstacle.AddComponent<ObstaclePlayerSpeed>();
                
                // Configure the script
                newScript.SetMaintainPlayerSpeed(enablePlayerSpeedMaintenance);
                newScript.SetSpeedReductionMultiplier(speedReductionMultiplier);
                newScript.SetMinSpeedAfterCollision(minSpeedAfterCollision);
                
                setupCount++;
                
                if (showDebugLogs)
                    Debug.Log($"ObstacleManager: Added ObstaclePlayerSpeed to {obstacle.name}");
            }
            else
            {
                // Update existing script settings
                existingScript.SetMaintainPlayerSpeed(enablePlayerSpeedMaintenance);
                existingScript.SetSpeedReductionMultiplier(speedReductionMultiplier);
                existingScript.SetMinSpeedAfterCollision(minSpeedAfterCollision);
                
                if (showDebugLogs)
                    Debug.Log($"ObstacleManager: Updated existing ObstaclePlayerSpeed on {obstacle.name}");
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstacleManager: Setup complete! Processed {obstacles.Length} obstacles, added script to {setupCount} new obstacles.");
    }
    
    /// <summary>
    /// Remove ObstaclePlayerSpeed script from all obstacles
    /// </summary>
    [ContextMenu("Remove All Obstacle Scripts")]
    public void RemoveAllObstacleScripts()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        int removeCount = 0;
        
        foreach (GameObject obstacle in obstacles)
        {
            ObstaclePlayerSpeed script = obstacle.GetComponent<ObstaclePlayerSpeed>();
            if (script != null)
            {
                DestroyImmediate(script);
                removeCount++;
                
                if (showDebugLogs)
                    Debug.Log($"ObstacleManager: Removed ObstaclePlayerSpeed from {obstacle.name}");
            }
            
            ObstacleGroundSpeed oldScript = obstacle.GetComponent<ObstacleGroundSpeed>();
            if (oldScript != null)
            {
                DestroyImmediate(oldScript);
                removeCount++;
                
                if (showDebugLogs)
                    Debug.Log($"ObstacleManager: Removed old ObstacleGroundSpeed from {obstacle.name}");
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstacleManager: Removed obstacle scripts from {removeCount} obstacles.");
    }
      /// <summary>
    /// Enable or disable player speed maintenance for all obstacles
    /// </summary>
    /// <param name="enabled">Whether to enable player speed maintenance</param>
    public void SetPlayerSpeedMaintenanceForAllObstacles(bool enabled)
    {
        enablePlayerSpeedMaintenance = enabled;
        
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        
        foreach (GameObject obstacle in obstacles)
        {
            ObstaclePlayerSpeed script = obstacle.GetComponent<ObstaclePlayerSpeed>();
            if (script != null)
            {
                script.SetMaintainPlayerSpeed(enabled);
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstacleManager: Set player speed maintenance to {enabled} for {obstacles.Length} obstacles.");
    }
    
    /// <summary>
    /// Set the minimum speed after collision for all obstacles
    /// </summary>
    /// <param name="minSpeed">Minimum speed after collision to prevent complete stop</param>
    public void SetMinSpeedAfterCollisionForAllObstacles(float minSpeed)
    {
        minSpeedAfterCollision = minSpeed;
        
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        
        foreach (GameObject obstacle in obstacles)
        {
            ObstaclePlayerSpeed script = obstacle.GetComponent<ObstaclePlayerSpeed>();
            if (script != null)
            {
                script.SetMinSpeedAfterCollision(minSpeed);
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstacleManager: Set min speed after collision to {minSpeed} for {obstacles.Length} obstacles.");
    }
      /// <summary>
    /// Set the speed reduction multiplier for all obstacles
    /// </summary>
    /// <param name="multiplier">Speed reduction multiplier (0.5 = 50% reduction, 0.8 = 20% reduction)</param>
    public void SetSpeedReductionMultiplierForAllObstacles(float multiplier)
    {
        speedReductionMultiplier = multiplier;
        
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        
        if (obstacles.Length == 0)
        {
            Debug.LogWarning($"ObstacleManager: No obstacles found with tag '{obstacleTag}' to set speed reduction multiplier!");
            return;
        }
        
        int updatedCount = 0;
        foreach (GameObject obstacle in obstacles)
        {
            ObstaclePlayerSpeed script = obstacle.GetComponent<ObstaclePlayerSpeed>();
            if (script != null)
            {
                script.SetSpeedReductionMultiplier(multiplier);
                updatedCount++;
                if (showDebugLogs)
                    Debug.Log($"ObstacleManager: Updated speed reduction multiplier to {multiplier} for {obstacle.name}");
            }
            else
            {
                Debug.LogWarning($"ObstacleManager: {obstacle.name} tagged '{obstacleTag}' but has no ObstaclePlayerSpeed script!");
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstacleManager: Set speed boost multiplier to {multiplier} for {updatedCount}/{obstacles.Length} obstacles.");
    }
    
    /// <summary>
    /// Debug method to show current settings
    /// </summary>
    [ContextMenu("Debug: Show Current Settings")]
    private void DebugShowCurrentSettings()
    {
        Debug.Log($"=== ObstacleManager Settings ===");
        Debug.Log($"Auto Setup On Start: {autoSetupOnStart}");
        Debug.Log($"Obstacle Tag: '{obstacleTag}'");
        Debug.Log($"Enable Player Speed Maintenance: {enablePlayerSpeedMaintenance}");
        Debug.Log($"Speed Reduction Multiplier: {speedReductionMultiplier}");
        Debug.Log($"Min Speed After Collision: {minSpeedAfterCollision}");
        Debug.Log($"Show Debug Logs: {showDebugLogs}");
        
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        Debug.Log($"Found {obstacles.Length} objects with tag '{obstacleTag}'");
        
        foreach (GameObject obstacle in obstacles)
        {
            ObstaclePlayerSpeed script = obstacle.GetComponent<ObstaclePlayerSpeed>();
            if (script != null)
            {
                Debug.Log($"✓ {obstacle.name} has ObstaclePlayerSpeed script");
            }
            else
            {
                Debug.Log($"✗ {obstacle.name} missing ObstaclePlayerSpeed script");
            }
        }
    }
    
    /// <summary>
    /// Test method to apply current settings to all obstacles
    /// </summary>
    [ContextMenu("Debug: Apply Current Settings to All Obstacles")]
    private void DebugApplyCurrentSettings()
    {
        Debug.Log($"ObstacleManager: Applying current settings - Speed Reduction: {speedReductionMultiplier}, Min Speed: {minSpeedAfterCollision}");
        SetSpeedReductionMultiplierForAllObstacles(speedReductionMultiplier);
        SetMinSpeedAfterCollisionForAllObstacles(minSpeedAfterCollision);
        SetPlayerSpeedMaintenanceForAllObstacles(enablePlayerSpeedMaintenance);
    }
}
