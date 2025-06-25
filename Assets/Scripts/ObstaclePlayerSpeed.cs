using UnityEngine;

/// <summary>
/// Applied to obstacles to reduce player speed when colliding with them
/// This makes obstacles act as impediments that slow down the player
/// </summary>
public class ObstaclePlayerSpeed : MonoBehaviour
{    [Header("Speed Settings")]
    [SerializeField] private bool maintainPlayerSpeed = false; // Enable/disable speed maintenance (now defaults to false)
    [SerializeField] private float speedReductionMultiplier = 0.5f; // Reduce player speed (0.5 = 50% reduction, 0.8 = 20% reduction)
    [SerializeField] private float minSpeedAfterCollision = 2f; // Minimum speed after collision to prevent complete stop
    
    [Header("Physics Settings")]
    [SerializeField] private bool increaseObstacleFriction = true; // Increase friction on obstacle to slow player
    [SerializeField] private float obstacleFriction = 0.8f; // High friction value for realistic collision
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false; // Show debug information
    
    private PhysicsMaterial2D originalMaterial;
    private PhysicsMaterial2D highFrictionMaterial;
    
    private void Start()
    {
        SetupObstacle();
    }
    
    /// <summary>
    /// Setup the obstacle for realistic player collision
    /// </summary>
    private void SetupObstacle()
    {
        Collider2D obstacleCollider = GetComponent<Collider2D>();
        if (obstacleCollider == null)
        {
            Debug.LogError($"ObstaclePlayerSpeed on {gameObject.name}: No Collider2D found!");
            return;
        }
        
        // Store original material
        originalMaterial = obstacleCollider.sharedMaterial;
        
        if (increaseObstacleFriction)
        {
            // Create or find high friction material
            highFrictionMaterial = Resources.Load<PhysicsMaterial2D>("HighFriction");
            if (highFrictionMaterial == null)
            {
                // Create high friction material
                highFrictionMaterial = new PhysicsMaterial2D("HighFriction");
                highFrictionMaterial.friction = obstacleFriction;
                highFrictionMaterial.bounciness = 0f;
            }
            
            // Apply high friction material
            obstacleCollider.sharedMaterial = highFrictionMaterial;
            
            if (showDebugLogs)
                Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Applied high friction material ({obstacleFriction})");
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Setup complete - will reduce player speed on collision");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject, true);
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject, false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerCollision(other.gameObject, true);
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        HandlePlayerCollision(other.gameObject, false);
    }
      /// <summary>
    /// Handle player collision with this obstacle
    /// </summary>
    /// <param name="collisionObject">The object that collided</param>
    /// <param name="isEntering">Whether this is initial collision or ongoing</param>
    private void HandlePlayerCollision(GameObject collisionObject, bool isEntering)
    {
        // Check if it's the player
        if (!collisionObject.CompareTag("Player")) return;
        
        // Get player's movement component
        PlayerMovement playerMovement = collisionObject.GetComponent<PlayerMovement>();
        if (playerMovement == null) return;
        
        // Get player's rigidbody
        Rigidbody2D playerRigidbody = collisionObject.GetComponent<Rigidbody2D>();
        if (playerRigidbody == null) return;
        
        // Get current velocity
        Vector2 velocity = playerRigidbody.linearVelocity;
        
        if (maintainPlayerSpeed)
        {
            // Old behavior: maintain speed (kept for backward compatibility)
            float currentGroundSpeed = playerMovement.GetCurrentGroundSpeed();
            float targetSpeed = currentGroundSpeed;
            
            if (Mathf.Abs(velocity.x) < Mathf.Abs(targetSpeed * 0.8f))
            {
                float newSpeedX = Mathf.Lerp(velocity.x, targetSpeed, 0.3f);
                velocity.x = newSpeedX;
                playerRigidbody.linearVelocity = velocity;
                
                if (showDebugLogs && isEntering)
                    Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Maintained player speed at {newSpeedX:F2}");
            }
        }
        else
        {
            // New behavior: reduce player speed when hitting obstacles
            if (isEntering) // Only reduce speed on initial collision, not during ongoing contact
            {
                float currentSpeed = Mathf.Abs(velocity.x);
                float reducedSpeed = currentSpeed * speedReductionMultiplier;
                
                // Ensure minimum speed to prevent complete stop
                reducedSpeed = Mathf.Max(reducedSpeed, minSpeedAfterCollision);
                
                // Apply speed reduction while maintaining direction
                float direction = Mathf.Sign(velocity.x);
                velocity.x = direction * reducedSpeed;
                
                // Also slightly reduce vertical velocity to make collision feel more impactful
                velocity.y *= 0.8f;
                
                playerRigidbody.linearVelocity = velocity;
                
                if (showDebugLogs)
                    Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Reduced player speed from {currentSpeed:F2} to {reducedSpeed:F2}");
            }
        }
    }
    
    /// <summary>
    /// Enable or disable speed maintenance (legacy behavior)
    /// </summary>
    /// <param name="enabled">Whether to maintain player speed</param>
    public void SetMaintainPlayerSpeed(bool enabled)
    {
        maintainPlayerSpeed = enabled;
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Maintain player speed set to {enabled}");
    }
      /// <summary>
    /// Set the speed reduction multiplier (new behavior)
    /// </summary>
    /// <param name="multiplier">Speed reduction multiplier (0.5 = 50% reduction, 0.8 = 20% reduction)</param>
    public void SetSpeedReductionMultiplier(float multiplier)
    {
        speedReductionMultiplier = Mathf.Clamp01(multiplier); // Ensure it's between 0 and 1
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Speed reduction multiplier set to {speedReductionMultiplier}");
    }
    
    /// <summary>
    /// Set the minimum speed after collision
    /// </summary>
    /// <param name="minSpeed">Minimum speed to prevent complete stop</param>
    public void SetMinSpeedAfterCollision(float minSpeed)
    {
        minSpeedAfterCollision = Mathf.Max(0f, minSpeed);
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Min speed after collision set to {minSpeedAfterCollision}");
    }

    // Backward compatibility methods for ObstacleManager
    /// <summary>
    /// Legacy method: Set speed boost multiplier (now maps to speed reduction for backward compatibility)
    /// </summary>
    /// <param name="multiplier">Speed multiplier - now inverted for speed reduction</param>

    public void SetSpeedBoostMultiplier(float multiplier)
    {
        // Convert boost multiplier to reduction multiplier for backward compatibility
        // If boost was 1.1 (10% boost), convert to 0.9 (10% reduction)
        // If boost was 1.0 (no boost), convert to 0.5 (50% reduction)
        float reductionMultiplier = multiplier > 1.0f ? (2.0f - multiplier) : (1.0f - multiplier * 0.5f);
        reductionMultiplier = Mathf.Clamp01(reductionMultiplier);
        
        speedReductionMultiplier = reductionMultiplier;
        
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Legacy speed boost {multiplier} converted to reduction {speedReductionMultiplier}");
    }
    
    /// <summary>
    /// Legacy method: Set max allowed speed (now maps to minimum speed for backward compatibility)
    /// </summary>
    /// <param name="maxSpeed">Max speed - now used as minimum speed threshold</param>

    public void SetMaxAllowedSpeed(float maxSpeed)
    {
        // Use a fraction of the max speed as minimum speed
        minSpeedAfterCollision = maxSpeed * 0.1f; // 10% of max becomes minimum
        
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Legacy max speed {maxSpeed} converted to min speed {minSpeedAfterCollision}");
    }
}
