using UnityEngine;

/// <summary>
/// Applied to obstacles to ensure player maintains speed when colliding with them
/// This prevents the player from getting stuck on obstacles
/// </summary>
public class ObstaclePlayerSpeed : MonoBehaviour
{    [Header("Speed Settings")]
    [SerializeField] private bool maintainPlayerSpeed = true; // Enable/disable speed maintenance
    [SerializeField] private float speedBoostMultiplier = 1.0f; // Multiply player speed (1.0 = normal, 1.1 = 10% boost)
    [SerializeField] private float maxAllowedSpeed = 20f; // Maximum speed to prevent flying away
    
    [Header("Physics Settings")]
    [SerializeField] private bool reduceObstacleFriction = true; // Reduce friction on obstacle
    [SerializeField] private float obstacleFriction = 0.1f; // Low friction value for smooth sliding
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false; // Show debug information
    
    private PhysicsMaterial2D originalMaterial;
    private PhysicsMaterial2D lowFrictionMaterial;
    
    private void Start()
    {
        SetupObstacle();
    }
    
    /// <summary>
    /// Setup the obstacle for smooth player interaction
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
        
        if (reduceObstacleFriction)
        {
            // Create or find low friction material
            lowFrictionMaterial = Resources.Load<PhysicsMaterial2D>("LowFriction");
            if (lowFrictionMaterial == null)
            {
                // Create low friction material
                lowFrictionMaterial = new PhysicsMaterial2D("LowFriction");
                lowFrictionMaterial.friction = obstacleFriction;
                lowFrictionMaterial.bounciness = 0f;
            }
            
            // Apply low friction material
            obstacleCollider.sharedMaterial = lowFrictionMaterial;
            
            if (showDebugLogs)
                Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Applied low friction material ({obstacleFriction})");
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Setup complete - maintains player speed on collision");
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
        if (!maintainPlayerSpeed) return;
        
        // Check if it's the player
        if (!collisionObject.CompareTag("Player")) return;
        
        // Get player's movement component
        PlayerMovement playerMovement = collisionObject.GetComponent<PlayerMovement>();
        if (playerMovement == null) return;
        
        // Get player's rigidbody
        Rigidbody2D playerRigidbody = collisionObject.GetComponent<Rigidbody2D>();
        if (playerRigidbody == null) return;
          // Get current ground speed
        float currentGroundSpeed = playerMovement.GetCurrentGroundSpeed();
        float targetSpeed = Mathf.Min(currentGroundSpeed * speedBoostMultiplier, maxAllowedSpeed);
        
        // Get current velocity
        Vector2 velocity = playerRigidbody.linearVelocity;
        
        // Only maintain minimum speed to prevent getting stuck
        // Don't boost if player is already moving fast enough
        if (Mathf.Abs(velocity.x) < Mathf.Abs(targetSpeed * 0.8f)) // Only if significantly slower
        {
            // Smoothly adjust velocity instead of setting it directly
            float newSpeedX = Mathf.Lerp(velocity.x, targetSpeed, 0.3f); // Smooth transition
            
            // Ensure we don't exceed max allowed speed
            newSpeedX = Mathf.Clamp(newSpeedX, -maxAllowedSpeed, maxAllowedSpeed);
            
            velocity.x = newSpeedX;
            playerRigidbody.linearVelocity = velocity;
            
            if (showDebugLogs && isEntering)
                Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Adjusted player speed to {newSpeedX:F2} (target: {targetSpeed:F2})");
        }
        else if (showDebugLogs && isEntering)
        {
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Player speed OK ({velocity.x:F2}), no adjustment needed");
        }
    }
    
    /// <summary>
    /// Enable or disable speed maintenance
    /// </summary>
    /// <param name="enabled">Whether to maintain player speed</param>
    public void SetMaintainPlayerSpeed(bool enabled)
    {
        maintainPlayerSpeed = enabled;
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Maintain player speed set to {enabled}");
    }
      /// <summary>
    /// Set the speed boost multiplier
    /// </summary>
    /// <param name="multiplier">Speed multiplier (1.0 = normal, 1.1 = 10% boost)</param>
    public void SetSpeedBoostMultiplier(float multiplier)
    {
        speedBoostMultiplier = multiplier;
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Speed boost multiplier set to {multiplier}");
    }
    
    /// <summary>
    /// Set the maximum allowed speed
    /// </summary>
    /// <param name="maxSpeed">Maximum speed to prevent flying away</param>
    public void SetMaxAllowedSpeed(float maxSpeed)
    {
        maxAllowedSpeed = maxSpeed;
        if (showDebugLogs)
            Debug.Log($"ObstaclePlayerSpeed on {gameObject.name}: Max allowed speed set to {maxSpeed}");
    }
}
