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
    [SerializeField] private float edgeDetectionDistance = 0.1f;    [Header("Backward Jump Settings")]
    [SerializeField] private bool enableBackwardJump = true; // Enable backward jump with S/Down keys
    [SerializeField] private float backwardJumpForce = 15f; // Force for backward jumps
    [SerializeField] private float backwardJumpRatio = 0.6f; // How much backward component (0.6 = 60% backward, 40% up)
    [SerializeField] private float backwardUpwardRatio = 0.8f; // How much upward component for backward jumps
    
    [Header("Debug")]
    // Debug visualization removed to eliminate unused variable warning
    
    // Components
    private Rigidbody2D rigidBody;
    private CircleCollider2D groundCheckCollider;
      // Jump state
    private int currentJumps = 0;
    private bool isGrounded = false;
    private bool jumpInputPressed = false;
    private bool backwardJumpInputPressed = false;
    private float lastJumpTime = 0f; // Track when last jump was performed
    private float lastAutoResetCheck = 0f; // Track automatic reset checks
    
    // Input Actions (for new Input System)
    private PlayerInput playerInput;
    private InputAction jumpAction;
    private InputAction backwardJumpAction;    private void Awake()
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
            
            // Try to get backward jump action (if it exists in the input actions)
            try
            {
                backwardJumpAction = playerInput.actions["BackwardJump"];
            }
            catch
            {
                Debug.LogWarning("BackwardJump action not found in PlayerInput. Using fallback input detection.");
            }
        }
    }
      private void Update()
    {
        GetJumpInput();
        
        if (jumpInputPressed && CanJump())
        {
            PerformJump(false); // Normal jump
        }
        else if (backwardJumpInputPressed && CanJump())
        {
            PerformJump(true); // Backward jump
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
    }
    
    private void GetJumpInput()
    {
        jumpInputPressed = false;
        backwardJumpInputPressed = false;
        
        // Try to use new Input System first, fallback to legacy
        if (playerInput != null && jumpAction != null)
        {
            // New Input System - Normal jump
            jumpInputPressed = jumpAction.WasPressedThisFrame();
        }
        else if (Keyboard.current != null)
        {
            // Fallback: Direct keyboard input for new Input System - Normal jump
            jumpInputPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        else
        {
            // Legacy Input System fallback - Normal jump
            jumpInputPressed = Input.GetKeyDown(KeyCode.Space);
        }
        
        // Backward jump input detection
        if (enableBackwardJump)
        {
            if (playerInput != null && backwardJumpAction != null)
            {
                // New Input System - Backward jump
                backwardJumpInputPressed = backwardJumpAction.WasPressedThisFrame();
            }
            else if (Keyboard.current != null)
            {
                // Fallback: Direct keyboard input for new Input System - Backward jump
                backwardJumpInputPressed = Keyboard.current.sKey.wasPressedThisFrame || 
                                         Keyboard.current.downArrowKey.wasPressedThisFrame;
            }
            else
            {
                // Legacy Input System fallback - Backward jump
                backwardJumpInputPressed = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
            }
        }
    }
      private bool CanJump()
    {
        // If player is grounded, allow unlimited jumps
        if (isGrounded)
        {
            return Time.time - lastJumpTime >= jumpCooldown; // Only cooldown restriction
        }
        
        // If player is in the air, limit to maxJumps (3)
        if (currentJumps >= maxJumps) return false;
        
        // Check if enough time has passed since last jump (cooldown)
        if (Time.time - lastJumpTime < jumpCooldown) return false;
        
        return true;
    }
      private void PerformJump(bool isBackwardJump = false)
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
        
        // Calculate actual jump force
        float effectiveJumpForce = isBackwardJump ? backwardJumpForce : jumpForce;
        float currentUpwardVelocity = Mathf.Max(0f, rigidBody.linearVelocity.y);
        
        // Reduce jump force if we're already moving upward to prevent excessive height
        if (currentUpwardVelocity > 5f)
        {
            effectiveJumpForce *= 0.7f; // Reduce subsequent jump power
        }
        
        // Calculate jump direction
        Vector2 jumpDirection;
        if (isBackwardJump)
        {
            // Backward jump: combine backward and upward movement
            jumpDirection = new Vector2(-backwardJumpRatio, backwardUpwardRatio).normalized;
        }
        else
        {
            // Normal jump direction
            jumpDirection = CalculateJumpDirection();
        }
        
        // Apply directional jump force
        rigidBody.AddForce(jumpDirection * effectiveJumpForce, ForceMode2D.Impulse);
        
        // Clamp the resulting velocity to prevent flying away
        Vector2 newVelocity = rigidBody.linearVelocity;
        newVelocity.y = Mathf.Min(newVelocity.y, maxJumpVelocity);
        rigidBody.linearVelocity = newVelocity;
        
        // Update jump tracking - only count jumps when in the air
        if (!isGrounded)
        {
            currentJumps++; // Only increment air jumps
        }
        // Ground jumps don't count toward the limit
        
        lastJumpTime = Time.time;
        
        // Log jump for debugging
        string jumpType = isBackwardJump ? "BACKWARD" : "Normal";
        string jumpContext = isGrounded ? "GROUND" : "AIR";
        Debug.Log($"Jump performed! Type: {jumpType}, Context: {jumpContext}, Direction: {jumpDirection}, Air jumps: {currentJumps}/{maxJumps}, Velocity: {rigidBody.linearVelocity}");
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
        int previousAirJumps = currentJumps;
        currentJumps = 0; // Reset air jumps when touching ground
        Debug.Log($"Player landed - air jumps reset from {previousAirJumps} to 0. Now has unlimited ground jumps. Velocity: {(rigidBody != null ? rigidBody.linearVelocity.ToString() : "N/A")}");
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
        Debug.Log($"Is Grounded: {isGrounded}");
        Debug.Log($"Air Jumps Used: {currentJumps}/{maxJumps}");
        Debug.Log($"Ground Jumps: UNLIMITED (when grounded)");
        Debug.Log($"Can Jump: {CanJump()}");
        Debug.Log($"Jump Force: {jumpForce}");
        Debug.Log($"Max Jump Velocity: {maxJumpVelocity}");
        Debug.Log($"Jump Cooldown: {jumpCooldown}s");
        Debug.Log($"Reset Velocity On Jump: {resetVelocityOnJump}");
        Debug.Log($"Use Directional Jump: {useDirectionalJump}");
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
        Debug.Log($"Enable Backward Jump: {enableBackwardJump}");
        Debug.Log($"Backward Jump Force: {backwardJumpForce}");
        Debug.Log($"Backward Jump Ratio: {backwardJumpRatio}");
        Debug.Log($"Backward Upward Ratio: {backwardUpwardRatio}");
        if (rigidBody != null)
        {
            Debug.Log($"Current Velocity: {rigidBody.linearVelocity}");
            Debug.Log($"Predicted Jump Direction: {CalculateJumpDirection()}");
            Debug.Log($"Predicted Backward Jump Direction: {new Vector2(-backwardJumpRatio, backwardUpwardRatio).normalized}");
            
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
            
            // Ground detection check
            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            Collider2D groundHit = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayerMask);
            Debug.Log($"Ground Check Result: {(groundHit != null ? groundHit.name : "No ground detected")}");
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
    
    /// <summary>
    /// Manually check if player is touching ground and reset jumps if needed
    /// </summary>
    private void ForceGroundCheck()
    {
        if (rigidBody == null) return;
        
        // Create a slightly larger check area
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
                // If touching any solid object (not the player itself), consider it ground
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
    
    [ContextMenu("Debug: Test Backward Jump")]
    private void DebugTestBackwardJump()
    {
        if (CanJump())
        {
            PerformJump(true);
            Debug.Log("Manual backward jump triggered for testing");
        }
        else
        {
            Debug.Log("Cannot perform backward jump - check jump limits and cooldown");
        }
    }
    
    [ContextMenu("Debug: Force Jump Reset Now")]
    private void DebugForceJumpResetNow()
    {
        int previousJumps = currentJumps;
        currentJumps = 0;
        isGrounded = true;
        Debug.Log($"Manual jump reset: {previousJumps} -> 0. Ground state set to true.");
    }
    
    [ContextMenu("Debug: Test Jump Reset Logic")]
    private void DebugTestJumpResetLogic()
    {
        bool shouldReset = ShouldForceJumpReset();
        Debug.Log($"Should force jump reset: {shouldReset}");
        Debug.Log($"Current jumps: {currentJumps}");
        Debug.Log($"Is grounded: {isGrounded}");
        Debug.Log($"Velocity Y: {(rigidBody != null ? rigidBody.linearVelocity.y : 0f)}");
        Debug.Log($"Time since last jump: {Time.time - lastJumpTime:F2}s");
        
        if (shouldReset)
        {
            ForceJumpReset();
        }
    }
    
    [ContextMenu("QUICK FIX: Prevent All Backward Jumps")]
    private void QuickFixPreventBackwardJumps()
    {
        preventBackwardJumps = true;
        preventBackwardSurfaceJumps = true;
        enableBackwardJump = false;
        Debug.Log("BACKWARD JUMP PREVENTION: Enabled for all jump types");
    }
    
    [ContextMenu("QUICK FIX: Allow All Jump Directions")]
    private void QuickFixAllowAllJumpDirections()
    {
        preventBackwardJumps = false;
        preventBackwardSurfaceJumps = false;
        enableBackwardJump = true;
        Debug.Log("BACKWARD JUMP PREVENTION: Disabled - all jump directions allowed");
    }
    
    [ContextMenu("Test: Backward Jump Settings - Escape Mode")]
    private void SetEscapeBackwardJumpSettings()
    {
        enableBackwardJump = true;
        backwardJumpForce = 18f; // Higher force for escaping
        backwardJumpRatio = 0.8f; // Strong backward component
        backwardUpwardRatio = 0.6f; // Moderate upward component
        Debug.Log("Backward jump set to ESCAPE MODE (strong backward force for getting unstuck)");
    }
    
    [ContextMenu("Test: Backward Jump Settings - Balanced Mode")]
    private void SetBalancedBackwardJumpSettings()
    {
        enableBackwardJump = true;
        backwardJumpForce = 15f; // Normal force
        backwardJumpRatio = 0.6f; // Moderate backward component
        backwardUpwardRatio = 0.8f; // Strong upward component
        Debug.Log("Backward jump set to BALANCED MODE (good mix of backward and upward)");
    }
    
    [ContextMenu("Test: Disable Backward Jump")]
    private void DisableBackwardJump()
    {
        enableBackwardJump = false;
        Debug.Log("Backward jump DISABLED");
    }
    
    /// <summary>
    /// Check if we should force a jump reset based on player state
    /// </summary>
    /// <returns>True if jump counter should be reset</returns>
    private bool ShouldForceJumpReset()
    {
        // Don't reset if we're clearly in the air
        if (rigidBody.linearVelocity.y > 2f) return false;
        
        // Don't reset too frequently
        if (Time.time - lastJumpTime < 0.2f) return false;
        
        // Force reset if player is clearly on ground but jumps aren't reset
        if (currentJumps > 0)
        {
            // Check if player is moving very slowly vertically (likely on ground)
            if (Mathf.Abs(rigidBody.linearVelocity.y) < 0.5f)
            {
                // Additional ground check to be sure
                Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
                Collider2D groundHit = Physics2D.OverlapCircle(checkPosition, groundCheckRadius * 1.2f, groundLayerMask);
                
                if (groundHit != null)
                {
                    return true;
                }
                
                // Also check for any solid collider below the player
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius * 2f, groundLayerMask);
                if (hit.collider != null)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Force reset the jump counter when player is clearly on ground
    /// </summary>
    private void ForceJumpReset()
    {
        if (currentJumps > 0)
        {
            Debug.Log($"Force jump reset - was {currentJumps}, now 0. Velocity: {rigidBody.linearVelocity}");
            currentJumps = 0;
            
            // Also ensure grounded state is correct
            if (!isGrounded)
            {
                isGrounded = true;
                Debug.Log("Force jump reset also set grounded state to true");
            }
        }
    }
}
