using UnityEngine;

/// <summary>
/// Manages ground speed for obstacles to prevent player from getting stuck
/// This script should be attached to obstacle GameObjects
/// </summary>
public class ObstacleGroundSpeed : MonoBehaviour
{
    [Header("Ground Speed Settings")]
    [SerializeField] private bool autoDetectPlayer = true; // Automatically find the player
    [SerializeField] private PlayerMovement playerMovement; // Manual reference if auto-detect fails
    
    [Header("Movement Settings")]
    [SerializeField] private bool moveWithGround = true; // Enable/disable ground speed matching
    [SerializeField] private float speedMultiplier = 1f; // Multiply the ground speed (1 = same speed as ground)
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false; // Show debug information
    
    private Rigidbody2D obstacleRigidbody;
    private float currentGroundSpeed = 0f;    private void Start()
    {
        // Check what type of collider this obstacle has
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        bool hasEdgeCollider = edgeCollider != null;
        
        if (hasEdgeCollider && showDebugLogs)
        {
            Debug.Log($"ObstacleGroundSpeed on {gameObject.name}: Edge Collider 2D detected - using Transform movement (no Rigidbody2D needed)");
        }
        
        // Get the Rigidbody2D component
        obstacleRigidbody = GetComponent<Rigidbody2D>();
        
        // For Edge Collider obstacles, we DON'T need Rigidbody2D - they work better as static colliders
        if (obstacleRigidbody == null && !hasEdgeCollider)
        {
            Debug.LogWarning($"ObstacleGroundSpeed on {gameObject.name}: No Rigidbody2D found and no Edge Collider! Adding Rigidbody2D for non-edge-collider obstacles.");
            obstacleRigidbody = gameObject.AddComponent<Rigidbody2D>();
            
            // Configure the Rigidbody2D for non-edge-collider obstacles
            obstacleRigidbody.bodyType = RigidbodyType2D.Kinematic;
            obstacleRigidbody.gravityScale = 0f;
            obstacleRigidbody.linearDamping = 0f;
            obstacleRigidbody.angularDamping = 0f;
            obstacleRigidbody.freezeRotation = true;
        }
        else if (obstacleRigidbody != null && hasEdgeCollider)
        {
            // If Edge Collider obstacle already has Rigidbody2D, we can remove it for better performance
            if (showDebugLogs)
                Debug.Log($"ObstacleGroundSpeed on {gameObject.name}: Edge Collider detected with existing Rigidbody2D - keeping it but using Transform movement");
        }
        
        // Auto-detect player if needed
        if (autoDetectPlayer && playerMovement == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerMovement = playerObject.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    if (showDebugLogs)
                        Debug.Log($"ObstacleGroundSpeed on {gameObject.name}: Auto-detected player movement");
                }
                else
                {
                    Debug.LogWarning($"ObstacleGroundSpeed on {gameObject.name}: Player found but no PlayerMovement script!");
                }
            }
            else
            {
                Debug.LogWarning($"ObstacleGroundSpeed on {gameObject.name}: No GameObject with 'Player' tag found!");
            }
        }
    }    private void FixedUpdate()
    {
        if (!moveWithGround || playerMovement == null)
            return;
        
        // Get current ground speed from player
        float newGroundSpeed = playerMovement.GetCurrentGroundSpeed() * speedMultiplier;
        
        // Only update if speed changed (for performance)
        if (Mathf.Abs(newGroundSpeed - currentGroundSpeed) > 0.01f)
        {
            currentGroundSpeed = newGroundSpeed;
            
            // Check if this is an Edge Collider obstacle (terrain-like)
            EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
            
            if (edgeCollider != null)
            {
                // For Edge Collider obstacles, use Transform movement (more stable for terrain)
                Vector3 movement = new Vector3(currentGroundSpeed * Time.fixedDeltaTime, 0f, 0f);
                transform.position += movement;
                
                if (showDebugLogs)
                    Debug.Log($"ObstacleGroundSpeed on {gameObject.name}: Updated position using Transform movement - speed {currentGroundSpeed:F2}");
            }
            else if (obstacleRigidbody != null)
            {
                // For non-Edge Collider obstacles, use Rigidbody movement
                if (obstacleRigidbody.bodyType == RigidbodyType2D.Kinematic)
                {
                    Vector2 currentPosition = obstacleRigidbody.position;
                    Vector2 movement = new Vector2(currentGroundSpeed * Time.fixedDeltaTime, 0f);
                    obstacleRigidbody.MovePosition(currentPosition + movement);
                }
                else
                {
                    Vector2 velocity = obstacleRigidbody.linearVelocity;
                    velocity.x = currentGroundSpeed;
                    obstacleRigidbody.linearVelocity = velocity;
                }
                
                if (showDebugLogs)
                    Debug.Log($"ObstacleGroundSpeed on {gameObject.name}: Updated using Rigidbody2D {obstacleRigidbody.bodyType} movement - speed {currentGroundSpeed:F2}");
            }
        }
    }
    
    /// <summary>
    /// Manually set the player movement reference
    /// </summary>
    /// <param name="player">PlayerMovement component reference</param>
    public void SetPlayerMovement(PlayerMovement player)
    {
        playerMovement = player;
        if (showDebugLogs)
            Debug.Log($"ObstacleGroundSpeed on {gameObject.name}: Player movement manually set");
    }
    
    /// <summary>
    /// Enable or disable ground speed matching
    /// </summary>
    /// <param name="enabled">Whether to move with ground</param>
    public void SetMoveWithGround(bool enabled)
    {
        moveWithGround = enabled;
        if (!enabled && obstacleRigidbody != null)
        {
            // Stop movement if disabled
            Vector2 velocity = obstacleRigidbody.linearVelocity;
            velocity.x = 0f;
            obstacleRigidbody.linearVelocity = velocity;
        }
        
        if (showDebugLogs)
            Debug.Log($"ObstacleGroundSpeed on {gameObject.name}: Move with ground set to {enabled}");
    }
    
    /// <summary>
    /// Get current ground speed being applied to this obstacle
    /// </summary>
    /// <returns>Current ground speed</returns>
    public float GetCurrentGroundSpeed()
    {
        return currentGroundSpeed;
    }
}
