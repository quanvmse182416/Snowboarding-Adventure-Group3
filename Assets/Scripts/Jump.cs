using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private int maxJumps = 3;
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    [SerializeField] private float jumpCooldown = 0.1f; // Minimum time between jumps
    [SerializeField] private float maxJumpVelocity = 20f; // Maximum upward velocity from jumping
    [SerializeField] private bool resetVelocityOnJump = true; // Reset upward velocity before jumping    [Header("Directional Jump Settings")]
    [SerializeField] private bool useDirectionalJump = false; // Jump in movement direction (DISABLED for straight up)
    [SerializeField] private float upwardJumpRatio = 0.8f; // How much of jump goes upward (0.8 = 80% up, 20% forward)
    [SerializeField] private float forwardJumpRatio = 0.4f; // How much of jump goes forward when moving
    [SerializeField] private float minHorizontalSpeed = 2f; // Minimum speed to consider for directional jump
    [SerializeField] private bool preventBackwardJumps = true; // Prevent jumping backward regardless of movement direction
    
    [Header("Surface Rotation Jump Settings")]
    [SerializeField] private bool useSurfaceRotationJump = false; // Jump perpendicular to surface when against walls (DISABLED for straight up)
    [SerializeField] private float surfaceDetectionDistance = 1f; // How far to check for surfaces
    [SerializeField] private float minSurfaceAngle = 45f; // Minimum surface angle to trigger rotation jump (degrees)
    [SerializeField] private LayerMask wallLayerMask = 1; // What layers count as walls/surfaces
    [SerializeField] private bool preventBackwardSurfaceJumps = true; // Prevent surface jumps from going backward
      [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0, -0.5f);
    [SerializeField] private float edgeDetectionDistance = 0.1f;    [Header("Stuck Prevention")]
    [SerializeField] private bool enableStuckDetection = true; // Enable enhanced stuck detection
    [SerializeField] private float stuckSpeedThreshold = 0.5f; // Speed below which player is considered potentially stuck
    [SerializeField] private float stuckTimeThreshold = 2f; // Time player must be slow before considered stuck
    [SerializeField] private float forceGroundCheckInterval = 1f; // How often to force ground check when stuck
    
    [Header("Debug")]
    // Debug visualization removed to eliminate unused variable warning
    
    // Components
    private Rigidbody2D rigidBody;
    private CircleCollider2D groundCheckCollider;
      // Jump state
    private int currentJumps = 0;
    private bool isGrounded = false;
    private bool jumpInputPressed = false;
    private float lastJumpTime = 0f; // Track when last jump was performed
    
    // Input Actions (for new Input System)
    private PlayerInput playerInput;
    private InputAction jumpAction;    private void Awake()
    {
        // Debug: Make sure this script is on the player, not the camera
        Debug.Log($"Jump script is attached to: {this.gameObject.name}");
        
        // Get required components
        rigidBody = GetComponent<Rigidbody2D>();
        
        if (rigidBody == null)
        {
            Debug.LogError($"Jump script on {this.gameObject.name} could not find Rigidbody2D! Make sure this script is attached to the player GameObject.");
        }
        
        // Setup physics to prevent edge sticking
        SetupPhysics();
        
        // Create ground check collider
        SetupGroundCheckCollider();
        
        // Setup Input System
        SetupInputSystem();
    }
    
    private void SetupPhysics()
    {
        if (rigidBody != null)
        {
            // Prevent sleeping to avoid sticking issues
            rigidBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
            
            // Adjust collision detection for better edge handling
            rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        // Setup physics material for smooth sliding
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            PhysicsMaterial2D smoothMaterial = new PhysicsMaterial2D("SmoothPlayer");
            smoothMaterial.friction = 0.1f; // Low friction to prevent sticking
            smoothMaterial.bounciness = 0f; // No bouncing
            playerCollider.sharedMaterial = smoothMaterial;
        }
    }    private void SetupGroundCheckCollider()
    {
        // Create a new GameObject for ground detection
        GameObject groundChecker = new GameObject("GroundChecker");
        
        // FORCE parent to be THIS gameObject (the player), never the camera
        groundChecker.transform.SetParent(this.transform, false);
        
        // Set local position relative to player
        groundChecker.transform.localPosition = groundCheckOffset;
        
        // Add CircleCollider2D for ground detection
        groundCheckCollider = groundChecker.AddComponent<CircleCollider2D>();
        groundCheckCollider.radius = groundCheckRadius;
        groundCheckCollider.isTrigger = true; // Make it a trigger so it doesn't affect physics
        
        // Set the layer to avoid conflicts
        groundChecker.layer = this.gameObject.layer;
        
        // Add the GroundDetector component
        GroundDetector detector = groundChecker.AddComponent<GroundDetector>();
        detector.Initialize(this);
        
        Debug.Log($"Ground checker created for: {this.gameObject.name} at local position: {groundCheckOffset}");
    }
    
    private void SetupInputSystem()
    {
        // Try to get PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            // Try to get jump action from PlayerInput
            try
            {
                jumpAction = playerInput.actions["Jump"];
            }
            catch
            {
                Debug.LogWarning("Jump action not found in PlayerInput. Using fallback input detection.");
            }
        }
    }
      private void Update()
    {
        GetJumpInput();
        
        // Enhanced stuck detection and ground checking
        if (enableStuckDetection)
        {
            CheckForStuckSituation();
        }
        
        if (jumpInputPressed && CanJump())
        {
            PerformJump();
        }
    }
    
    private void FixedUpdate()
    {
        // Prevent sticking to ground edges by applying small downward force when moving horizontally
        if (rigidBody != null && isGrounded && Mathf.Abs(rigidBody.linearVelocity.x) > 0.1f)
        {
            // Apply small downward force to help with edge transitions
            rigidBody.AddForce(Vector2.down * edgeDetectionDistance, ForceMode2D.Force);
        }
        
        // Check for stuck prevention
        if (enableStuckDetection && isGrounded)
        {
            CheckAndHandleStuck();
        }
    }
    
    private void GetJumpInput()
    {
        jumpInputPressed = false;
        
        // Try to use new Input System first, fallback to legacy
        if (playerInput != null && jumpAction != null)
        {
            // New Input System
            jumpInputPressed = jumpAction.WasPressedThisFrame();
        }
        else if (Keyboard.current != null)
        {
            // Fallback: Direct keyboard input for new Input System
            jumpInputPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        else
        {
            // Legacy Input System fallback
            jumpInputPressed = Input.GetKeyDown(KeyCode.Space);
        }
    }
      private bool CanJump()
    {
        // Check if we haven't exceeded max jumps
        if (currentJumps >= maxJumps) return false;
        
        // Check if enough time has passed since last jump (cooldown)
        if (Time.time - lastJumpTime < jumpCooldown) return false;
        
        return true;
    }
      private void PerformJump()
    {
        if (rigidBody == null) return;
        
        // Reset or limit vertical velocity to prevent stacking
        if (resetVelocityOnJump)
        {
            // Reset upward velocity before applying jump force
            Vector2 velocity = rigidBody.linearVelocity;
            velocity.y = Mathf.Max(0f, velocity.y); // Keep positive velocity but cap it
            rigidBody.linearVelocity = velocity;
        }
        
        // Calculate actual jump force based on current velocity
        float currentUpwardVelocity = Mathf.Max(0f, rigidBody.linearVelocity.y);
        float effectiveJumpForce = jumpForce;
        
        // Reduce jump force if we're already moving upward to prevent excessive height
        if (currentUpwardVelocity > 5f)
        {
            effectiveJumpForce *= 0.7f; // Reduce subsequent jump power
        }
        
        // Calculate jump direction
        Vector2 jumpDirection = CalculateJumpDirection();
        
        // Apply directional jump force
        rigidBody.AddForce(jumpDirection * effectiveJumpForce, ForceMode2D.Impulse);
        
        // Clamp the resulting velocity to prevent flying away
        Vector2 newVelocity = rigidBody.linearVelocity;
        newVelocity.y = Mathf.Min(newVelocity.y, maxJumpVelocity);
        rigidBody.linearVelocity = newVelocity;
        
        // Update jump tracking
        currentJumps++;
        lastJumpTime = Time.time;
        
        // Log jump for debugging
        Debug.Log($"Jump performed! Direction: {jumpDirection}, Current jumps: {currentJumps}/{maxJumps}, Velocity: {rigidBody.linearVelocity}");
    }    /// <summary>
    /// Calculate the direction for the jump based on player movement and surface contact
    /// </summary>
    /// <returns>Normalized jump direction vector</returns>
    private Vector2 CalculateJumpDirection()
    {
        // First, check if we should use surface rotation jump
        if (useSurfaceRotationJump)
        {
            Vector2 surfaceJumpDirection = GetSurfaceBasedJumpDirection();
            if (surfaceJumpDirection != Vector2.zero)
            {
                return surfaceJumpDirection;
            }
        }
        
        if (!useDirectionalJump)
        {
            // Traditional straight up jump
            return Vector2.up;
        }
        
        // Get current horizontal velocity
        float horizontalVelocity = rigidBody.linearVelocity.x;
        
        // If moving too slowly horizontally, jump straight up
        if (Mathf.Abs(horizontalVelocity) < minHorizontalSpeed)
        {
            return Vector2.up;
        }
        
        // Calculate directional jump
        // Determine forward direction based on velocity
        float forwardDirection = Mathf.Sign(horizontalVelocity);
        
        // Apply backward jump prevention
        if (preventBackwardJumps && forwardDirection < 0)
        {
            // If player is moving left/backward, but we want to prevent backward jumps
            // Either jump straight up or slightly forward
            return Vector2.up;
        }
        
        // Calculate jump components
        float upwardComponent = upwardJumpRatio;
        float forwardComponent = forwardJumpRatio * forwardDirection;
        
        // Create jump direction vector
        Vector2 jumpDirection = new Vector2(forwardComponent, upwardComponent);
        
        // Final safety check: ensure no backward component if prevention is enabled
        if (preventBackwardJumps && jumpDirection.x < 0)
        {
            jumpDirection.x = 0; // Remove backward component
        }
        
        // Normalize to ensure consistent jump force magnitude
        return jumpDirection.normalized;
    }
      /// <summary>
    /// Detect nearby surfaces and calculate jump direction perpendicular to them
    /// </summary>
    /// <returns>Surface-based jump direction, or Vector2.zero if no suitable surface found</returns>
    private Vector2 GetSurfaceBasedJumpDirection()
    {
        Vector2 playerPos = transform.position;
        
        // Check for surfaces in multiple directions around the player
        Vector2[] checkDirections = {
            Vector2.right,      // Right
            Vector2.left,       // Left
            new Vector2(0.7f, 0.7f),   // Up-right diagonal
            new Vector2(-0.7f, 0.7f),  // Up-left diagonal
            new Vector2(0.7f, -0.7f),  // Down-right diagonal
            new Vector2(-0.7f, -0.7f)  // Down-left diagonal
        };
        
        foreach (Vector2 direction in checkDirections)
        {
            // Cast a ray to detect surfaces
            RaycastHit2D hit = Physics2D.Raycast(playerPos, direction, surfaceDetectionDistance, wallLayerMask);
            
            if (hit.collider != null)
            {
                // Get the surface normal
                Vector2 surfaceNormal = hit.normal;
                
                // Calculate the angle of the surface relative to horizontal
                float surfaceAngle = Vector2.Angle(Vector2.up, surfaceNormal);
                
                // Only use this surface if it's steep enough
                if (surfaceAngle > minSurfaceAngle)
                {
                    // Jump perpendicular to the surface (along the normal)
                    // Ensure the jump direction has some upward component
                    Vector2 jumpDirection = surfaceNormal;
                    
                    // If the normal points downward, flip it
                    if (jumpDirection.y < 0)
                    {
                        jumpDirection.y = Mathf.Abs(jumpDirection.y);
                    }
                    
                    // Ensure minimum upward component
                    if (jumpDirection.y < 0.3f)
                    {
                        jumpDirection.y = 0.3f;
                        jumpDirection = jumpDirection.normalized;
                    }
                    
                    // Apply backward jump prevention for surface jumps
                    if (preventBackwardSurfaceJumps && jumpDirection.x < 0)
                    {
                        // If surface jump would go backward, either go straight up or slightly forward
                        jumpDirection.x = Mathf.Max(0, jumpDirection.x * 0.1f); // Greatly reduce or eliminate backward component
                        jumpDirection = jumpDirection.normalized;
                    }
                    
                    Debug.Log($"Surface jump detected! Surface angle: {surfaceAngle:F1}°, Jump direction: {jumpDirection}");
                    return jumpDirection.normalized;
                }
            }
        }
        
        // No suitable surface found
        return Vector2.zero;
    }
    
    /// <summary>
    /// Called by GroundDetector when player touches ground
    /// </summary>
    public void OnGroundEnter()
    {
        isGrounded = true;
        currentJumps = 0; // Reset jumps when touching ground
        Debug.Log("Player landed - jumps reset");
    }
    
    /// <summary>
    /// Called by GroundDetector when player leaves ground
    /// </summary>
    public void OnGroundExit()
    {
        isGrounded = false;
        Debug.Log("Player left ground");
    }
    
    /// <summary>
    /// Public method to get current jump count
    /// </summary>
    /// <returns>Current number of jumps used</returns>
    public int GetCurrentJumps()
    {
        return currentJumps;
    }
    
    /// <summary>
    /// Public method to get maximum jumps allowed
    /// </summary>
    /// <returns>Maximum number of jumps</returns>
    public int GetMaxJumps()
    {
        return maxJumps;
    }
    
    /// <summary>
    /// Public method to check if player is grounded
    /// </summary>
    /// <returns>True if player is on ground</returns>
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    /// <summary>
    /// Public method to set jump force
    /// </summary>
    /// <param name="force">New jump force value</param>
    public void SetJumpForce(float force)
    {
        jumpForce = Mathf.Max(0f, force);
    }
      /// <summary>
    /// Public method to set max jumps
    /// </summary>
    /// <param name="maxJumpsCount">New max jumps value</param>
    public void SetMaxJumps(int maxJumpsCount)
    {
        maxJumps = Mathf.Max(1, maxJumpsCount);
    }
    
    /// <summary>
    /// Public method to set jump cooldown
    /// </summary>
    /// <param name="cooldown">Minimum time between jumps in seconds</param>
    public void SetJumpCooldown(float cooldown)
    {
        jumpCooldown = Mathf.Max(0f, cooldown);
    }
    
    /// <summary>
    /// Public method to set maximum jump velocity
    /// </summary>
    /// <param name="maxVelocity">Maximum upward velocity from jumping</param>
    public void SetMaxJumpVelocity(float maxVelocity)
    {
        maxJumpVelocity = Mathf.Max(5f, maxVelocity);
    }
    
    /// <summary>
    /// Public method to enable/disable velocity reset on jump
    /// </summary>
    /// <param name="reset">Whether to reset velocity before jumping</param>
    public void SetResetVelocityOnJump(bool reset)
    {
        resetVelocityOnJump = reset;
    }
    
    /// <summary>
    /// Public method to enable/disable directional jumping
    /// </summary>
    /// <param name="enabled">Whether to use directional jumping</param>
    public void SetDirectionalJump(bool enabled)
    {
        useDirectionalJump = enabled;
    }
    
    /// <summary>
    /// Public method to set upward jump ratio
    /// </summary>
    /// <param name="ratio">How much of jump goes upward (0.0 to 1.0)</param>
    public void SetUpwardJumpRatio(float ratio)
    {
        upwardJumpRatio = Mathf.Clamp01(ratio);
    }
    
    /// <summary>
    /// Public method to set forward jump ratio
    /// </summary>
    /// <param name="ratio">How much of jump goes forward when moving</param>
    public void SetForwardJumpRatio(float ratio)
    {
        forwardJumpRatio = Mathf.Clamp(ratio, 0f, 1f);
    }
    
    /// <summary>
    /// Public method to set minimum horizontal speed for directional jump
    /// </summary>
    /// <param name="speed">Minimum speed to consider for directional jump</param>
    public void SetMinHorizontalSpeed(float speed)
    {
        minHorizontalSpeed = Mathf.Max(0f, speed);
    }
    
    /// <summary>
    /// Public method to enable/disable surface rotation jumping
    /// </summary>
    /// <param name="enabled">Whether to use surface rotation jumping</param>
    public void SetSurfaceRotationJump(bool enabled)
    {
        useSurfaceRotationJump = enabled;
    }
    
    /// <summary>
    /// Public method to set surface detection distance
    /// </summary>
    /// <param name="distance">How far to check for surfaces</param>
    public void SetSurfaceDetectionDistance(float distance)
    {
        surfaceDetectionDistance = Mathf.Max(0.1f, distance);
    }
    
    /// <summary>
    /// Public method to set minimum surface angle
    /// </summary>
    /// <param name="angle">Minimum surface angle to trigger rotation jump (degrees)</param>
    public void SetMinSurfaceAngle(float angle)
    {
        minSurfaceAngle = Mathf.Clamp(angle, 0f, 90f);
    }    /// <summary>
    /// Context menu methods for testing jump settings
    /// </summary>
    [ContextMenu("Test: Conservative Jump Settings")]
    private void SetConservativeJumpSettings()
    {
        jumpForce = 12f;
        maxJumpVelocity = 15f;
        jumpCooldown = 0.15f;
        resetVelocityOnJump = true;
        useDirectionalJump = true;
        upwardJumpRatio = 0.9f; // Mostly upward
        forwardJumpRatio = 0.2f; // Small forward component
        useSurfaceRotationJump = true;
        surfaceDetectionDistance = 0.8f; // Close detection
        minSurfaceAngle = 60f; // Only very steep surfaces
        Debug.Log("Jump settings set to Conservative (low power, mostly upward, steep surface detection)");
    }
    
    [ContextMenu("Test: Balanced Jump Settings")]
    private void SetBalancedJumpSettings()
    {
        jumpForce = 15f;
        maxJumpVelocity = 20f;
        jumpCooldown = 0.1f;
        resetVelocityOnJump = true;
        useDirectionalJump = true;
        upwardJumpRatio = 0.8f; // Good balance
        forwardJumpRatio = 0.4f; // Moderate forward component
        useSurfaceRotationJump = true;
        surfaceDetectionDistance = 1f; // Standard detection
        minSurfaceAngle = 45f; // Moderate surface angle
        Debug.Log("Jump settings set to Balanced (moderate power, directional, good surface detection)");
    }
    
    [ContextMenu("Test: High Jump Settings")]
    private void SetHighJumpSettings()
    {
        jumpForce = 18f;
        maxJumpVelocity = 25f;
        jumpCooldown = 0.05f;
        resetVelocityOnJump = false;
        useDirectionalJump = true;
        upwardJumpRatio = 0.7f; // Less upward
        forwardJumpRatio = 0.6f; // More forward momentum
        useSurfaceRotationJump = true;
        surfaceDetectionDistance = 1.2f; // Longer detection
        minSurfaceAngle = 30f; // Even gentle slopes
        Debug.Log("Jump settings set to High (higher power, more directional, sensitive surface detection)");
    }
      [ContextMenu("Test: Traditional Straight Up Jumps")]
    private void SetTraditionalJumpSettings()
    {
        jumpForce = 15f;
        maxJumpVelocity = 20f;
        jumpCooldown = 0.1f;
        resetVelocityOnJump = true;
        useDirectionalJump = false; // Disable directional jumping
        useSurfaceRotationJump = false; // Disable surface rotation
        Debug.Log("Jump settings set to Traditional (straight up jumps only) - DEFAULT BEHAVIOR");
    }
    
    [ContextMenu("QUICK FIX: Disable All Directional Features")]
    private void DisableAllDirectionalFeatures()
    {
        useDirectionalJump = false;
        useSurfaceRotationJump = false;
        Debug.Log("✅ DISABLED: All directional jump features - now jumps straight up only!");
    }
    
    [ContextMenu("QUICK FIX: Enable All Directional Features")]
    private void EnableAllDirectionalFeatures()
    {
        useDirectionalJump = true;
        useSurfaceRotationJump = true;
        Debug.Log("✅ ENABLED: All directional jump features - now uses smart jumping!");
    }
    
    [ContextMenu("Debug: Show Current Jump State")]
    private void DebugShowJumpState()
    {
        Debug.Log($"=== Jump System Debug ===");
        Debug.Log($"Current Jumps: {currentJumps}/{maxJumps}");
        Debug.Log($"Is Grounded: {isGrounded}");
        Debug.Log($"Jump Force: {jumpForce}");
        Debug.Log($"Max Jump Velocity: {maxJumpVelocity}");
        Debug.Log($"Jump Cooldown: {jumpCooldown}s");
        Debug.Log($"Reset Velocity On Jump: {resetVelocityOnJump}");        Debug.Log($"Use Directional Jump: {useDirectionalJump}");
        Debug.Log($"Prevent Backward Jumps: {preventBackwardJumps}");
        Debug.Log($"Upward Jump Ratio: {upwardJumpRatio}");
        Debug.Log($"Forward Jump Ratio: {forwardJumpRatio}");
        Debug.Log($"Min Horizontal Speed: {minHorizontalSpeed}");
        Debug.Log($"Use Surface Rotation Jump: {useSurfaceRotationJump}");
        Debug.Log($"Prevent Backward Surface Jumps: {preventBackwardSurfaceJumps}");
        Debug.Log($"Surface Detection Distance: {surfaceDetectionDistance}");
        Debug.Log($"Min Surface Angle: {minSurfaceAngle}°");
        Debug.Log($"Last Jump Time: {lastJumpTime}");
        Debug.Log($"Time Since Last Jump: {Time.time - lastJumpTime:F2}s");
        if (rigidBody != null)
        {
            Debug.Log($"Current Velocity: {rigidBody.linearVelocity}");
            Debug.Log($"Predicted Jump Direction: {CalculateJumpDirection()}");
            
            // Check for current surface detection
            Vector2 surfaceJump = GetSurfaceBasedJumpDirection();
            if (surfaceJump != Vector2.zero)
            {
                Debug.Log($"Surface-based jump detected! Direction: {surfaceJump}");
            }
            else
            {
                Debug.Log("No surface detected for rotation jump");
            }
        }
    }
      // Gizmos for debugging ground check area - DISABLED
    private void OnDrawGizmosSelected()
    {
        // Ground check gizmo disabled - remove red circle
        // if (showGroundCheck)
        // {
        //     Gizmos.color = isGrounded ? Color.green : Color.red;
        //     
        //     // Draw the ground check circle at the PLAYER position + offset, not camera position
        //     Vector3 checkPosition = this.transform.position + (Vector3)groundCheckOffset;
        //     Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        //     
        //     // Draw a line from player to ground check position for clarity
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawLine(this.transform.position, checkPosition);
        // }
    }
    
    // Stuck detection variables
    private float timeAtLowSpeed = 0f;
    private float lastForceGroundCheck = 0f;
    
    private void CheckAndHandleStuck()
    {
        // Check if player is moving slowly
        if (Mathf.Abs(rigidBody.linearVelocity.magnitude) < stuckSpeedThreshold)
        {
            timeAtLowSpeed += Time.fixedDeltaTime;
        }
        else
        {
            timeAtLowSpeed = 0f; // Reset timer if moving
        }
        
        // If stuck for too long, force a ground check
        if (timeAtLowSpeed >= stuckTimeThreshold)
        {
            // Perform a ground check
            Collider2D[] colliders = Physics2D.OverlapCircleAll((Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayerMask);
            bool wasGrounded = isGrounded;
            isGrounded = colliders.Length > 0;
            
            // If we were grounded and now we're not, we might be stuck
            if (wasGrounded && !isGrounded)
            {
                // Apply a small force upwards to unstick
                rigidBody.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
                Debug.Log("Player unstuck - applied upward force");
            }
            
            // Reset the timer
            timeAtLowSpeed = 0f;
        }
    }
    
    /// <summary>
    /// Check if player is stuck and force ground detection if needed
    /// </summary>
    private void CheckForStuckSituation()
    {
        if (rigidBody == null) return;
        
        // Check if player is moving very slowly
        float currentSpeed = rigidBody.linearVelocity.magnitude;
        
        if (currentSpeed < stuckSpeedThreshold)
        {
            timeAtLowSpeed += Time.deltaTime;
            
            // If player has been stuck for a while, force a ground check
            if (timeAtLowSpeed >= stuckTimeThreshold && 
                Time.time - lastForceGroundCheck >= forceGroundCheckInterval)
            {
                ForceGroundCheck();
                lastForceGroundCheck = Time.time;
                Debug.Log("Player appears stuck - forced ground check performed");
            }
        }
        else
        {
            timeAtLowSpeed = 0f; // Reset stuck timer if player is moving
        }
    }
    
    /// <summary>
    /// Manually check if player is touching ground and reset jumps if needed
    /// </summary>
    private void ForceGroundCheck()
    {
        if (rigidBody == null) return;
        
        // Create a slightly larger check area for stuck situations
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        float expandedRadius = groundCheckRadius * 1.5f; // 50% larger check area
        
        // Check for ground collision in expanded area
        Collider2D groundHit = Physics2D.OverlapCircle(checkPosition, expandedRadius, groundLayerMask);
        
        if (groundHit != null)
        {
            // Player is definitely on ground - ensure ground state is correct
            if (!isGrounded)
            {
                Debug.Log("Force ground check detected ground - resetting jumps");
                OnGroundEnter(); // Reset ground state and jumps
            }
        }
        
        // Additional check: if player has low vertical velocity and is touching any collider, consider it ground
        if (Mathf.Abs(rigidBody.linearVelocity.y) < 1f)
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(checkPosition, expandedRadius);
            
            foreach (Collider2D collider in nearbyColliders)
            {
                // If touching any solid object (not the player itself), consider it ground for stuck situations
                if (collider.gameObject != this.gameObject && 
                    !collider.isTrigger &&
                    (collider.CompareTag("Ground") || collider.CompareTag("Obstacle") || collider.name.Contains("Ground")))
                {
                    if (!isGrounded)
                    {
                        Debug.Log($"Force ground check detected contact with {collider.name} - resetting jumps");
                        OnGroundEnter(); // Reset ground state and jumps
                    }
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Manually reset jump counter - useful for debugging or special situations
    /// </summary>
    public void ResetJumps()
    {
        currentJumps = 0;
        Debug.Log("Jump counter manually reset");
    }
    
    /// <summary>
    /// Force a ground check - useful for debugging stuck situations
    /// </summary>
    public void ForceGroundCheckDebug()
    {
        ForceGroundCheck();
    }
    
    [ContextMenu("Debug: Reset Jump Counter")]
    private void DebugResetJumps()
    {
        ResetJumps();
    }
    
    [ContextMenu("Debug: Force Ground Check")]
    private void DebugForceGroundCheck()
    {
        ForceGroundCheckDebug();
    }
    
    [ContextMenu("Debug: Show Stuck Detection State")]
    private void DebugShowStuckState()
    {
        Debug.Log($"=== Stuck Detection Debug ===");
        Debug.Log($"Enable Stuck Detection: {enableStuckDetection}");
        Debug.Log($"Current Speed: {(rigidBody != null ? rigidBody.linearVelocity.magnitude : 0f):F2}");
        Debug.Log($"Stuck Speed Threshold: {stuckSpeedThreshold}");
        Debug.Log($"Time At Low Speed: {timeAtLowSpeed:F2}s");
        Debug.Log($"Stuck Time Threshold: {stuckTimeThreshold}s");
        Debug.Log($"Last Force Ground Check: {lastForceGroundCheck:F2}");
        Debug.Log($"Time Since Last Force Check: {Time.time - lastForceGroundCheck:F2}s");
        Debug.Log($"Force Ground Check Interval: {forceGroundCheckInterval}s");
    }
    
    [ContextMenu("QUICK FIX: Prevent All Backward Jumps")]
    private void QuickFixPreventBackwardJumps()
    {
        preventBackwardJumps = true;
        preventBackwardSurfaceJumps = true;
        Debug.Log("BACKWARD JUMP PREVENTION: Enabled for both directional and surface jumps");
    }
    
    [ContextMenu("QUICK FIX: Allow All Jump Directions")]
    private void QuickFixAllowAllJumpDirections()
    {
        preventBackwardJumps = false;
        preventBackwardSurfaceJumps = false;
        Debug.Log("BACKWARD JUMP PREVENTION: Disabled - all jump directions allowed");
    }
}
