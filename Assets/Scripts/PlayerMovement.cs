using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{    [Header("References")]
    public GameObject Ground;

    [Header("Movement Settings")]
    [SerializeField] private float torqueForce = 10f;
    [SerializeField] private float speedNormalizationRate = 2f;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float accelerationRate = 15f;
    [SerializeField] private float defaultSpeed = 12f;    [Header("Player State")]
    [SerializeField] private bool isAlive = true;// Components
    private Rigidbody2D rigidBody;
    private SurfaceEffector2D groundSurfaceEffector;

    // Input variables
    private float inputVertical;
    private float inputHorizontal;

    // Input Actions (for new Input System)
    private PlayerInput playerInput;    private InputAction moveAction;
    private InputAction rotateAction;

    private void Awake()
    {
        // Get required components
        this.rigidBody = this.GetComponent<Rigidbody2D>();
        
        // Get Surface Effector 2D from Ground GameObject
        if (this.Ground != null)
        {
            this.groundSurfaceEffector = this.Ground.GetComponent<SurfaceEffector2D>();
            
            if (this.groundSurfaceEffector == null)
            {
                Debug.LogError("No Surface Effector 2D found on Ground GameObject! Please add a Surface Effector 2D component to the Ground.");
            }
        }
        else
        {
            Debug.LogError("Ground GameObject not assigned in PlayerMovement!");
        }        // Initialize player state
        this.isAlive = true;        // Setup Input System
        SetupInputSystem();
    }

    private void SetupInputSystem()
    {
        // Try to get PlayerInput component, if it exists
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            // Use Input Actions from PlayerInput
            moveAction = playerInput.actions["Move"];
            rotateAction = playerInput.actions["Rotate"];
        }
    }    private void Update()
    {
        if (this.isAlive)
        {
            GetInput();
        }
    }

    private void GetInput()
    {
        // Try to use new Input System first, fallback to legacy
        if (playerInput != null && moveAction != null && rotateAction != null)
        {
            // New Input System
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            this.inputVertical = moveInput.y;
            this.inputHorizontal = rotateAction.ReadValue<float>();
        }
        else if (Keyboard.current != null)
        {
            // Fallback: Direct keyboard input for new Input System
            this.inputVertical = 0f;
            this.inputHorizontal = 0f;

            // Vertical input (W/S or Up/Down arrows)
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                this.inputVertical = 1f;
            }
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                this.inputVertical = -1f;
            }

            // Horizontal input (A/D or Left/Right arrows)
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                this.inputHorizontal = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                this.inputHorizontal = 1f;
            }
        }
        else
        {
            // Legacy Input System fallback
            this.inputVertical = Input.GetAxisRaw("Vertical");
            this.inputHorizontal = Input.GetAxisRaw("Horizontal");
        }
    }

    private void FixedUpdate()
    {
        if (!this.isAlive) return;

        float currentDefaultSpeed = this.isAlive ? this.defaultSpeed : 0f;

        // Player rotation using torque
        if (this.inputHorizontal != 0)
        {
            this.rigidBody.AddTorque(this.inputHorizontal * this.torqueForce);
        }        // Ensure Surface Effector component exists
        if (this.groundSurfaceEffector == null)
        {
            Debug.LogWarning("Surface Effector 2D component not found on Ground GameObject!");
            return;
        }

        // User-input induced speed change
        if (this.inputVertical != 0)
        {
            float oldSpeed = this.GetGroundSpeed();
            float newSpeed = this.GetUpdatedSpeed(
                this.inputVertical, 
                this.accelerationRate, 
                this.GetGroundSpeed(), 
                this.minSpeed, 
                this.maxSpeed
            );
            
            this.SetGroundSpeed(newSpeed);

#if UNITY_EDITOR
            Debug.Log($"PlayerMovement.GetUpdatedSpeed user input speed change [from: {oldSpeed:F2}] [to: {newSpeed:F2}]");
#endif
        }
        // Speed normalization when no input
        else if (!this.IsDefaultSpeed(this.GetGroundSpeed(), currentDefaultSpeed))
        {
            float oldSpeed = this.GetGroundSpeed();
            float normalizedSpeed = this.GetNormalizedSpeed(
                this.GetGroundSpeed(), 
                currentDefaultSpeed, 
                this.speedNormalizationRate
            );
            
            this.SetGroundSpeed(normalizedSpeed);

#if UNITY_EDITOR
            Debug.Log($"PlayerMovement.GetNormalizedSpeed [from: {oldSpeed:F2}] [to: {normalizedSpeed:F2}]");
#endif
        }

        // Reset input values
        this.inputVertical = 0f;
        this.inputHorizontal = 0f;
    }

    /// <summary>
    /// Calculate updated speed based on user input, clamped between minSpeed and maxSpeed.
    /// </summary>
    /// <param name="inputVertical">Vertical input value</param>
    /// <param name="accelerationRate">Rate of acceleration/deceleration</param>
    /// <param name="currentSpeed">Current ground speed</param>
    /// <param name="minSpeed">Minimum allowed speed</param>
    /// <param name="maxSpeed">Maximum allowed speed</param>
    /// <returns>Updated speed value</returns>
    private float GetUpdatedSpeed(float inputVertical, float accelerationRate, float currentSpeed, float minSpeed, float maxSpeed)
    {
        float speedChange = inputVertical * Time.fixedDeltaTime * accelerationRate;
        float newSpeed = currentSpeed + speedChange;
        
        return Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
    }

    /// <summary>
    /// Determines if currentSpeed is approximately equal to defaultSpeed.
    /// </summary>
    /// <param name="currentSpeed">Current speed value</param>
    /// <param name="defaultSpeed">Default speed value</param>
    /// <returns>True if the two speeds are approximately equal</returns>
    private bool IsDefaultSpeed(float currentSpeed, float defaultSpeed)
    {
        return Mathf.Approximately(currentSpeed, defaultSpeed);
    }

    /// <summary>
    /// Normalize player's speed back towards defaultSpeed over time.
    /// </summary>
    /// <param name="currentSpeed">Current speed value</param>
    /// <param name="defaultSpeed">Target default speed</param>
    /// <param name="speedNormalizationRate">Rate at which to normalize the speed</param>
    /// <returns>Normalized speed value</returns>
    private float GetNormalizedSpeed(float currentSpeed, float defaultSpeed, float speedNormalizationRate)
    {
        float speedDifferential = (currentSpeed - defaultSpeed) / speedNormalizationRate;
        float normalizedSpeed = currentSpeed - speedDifferential;
        
        // If we're very close to the default speed, just snap to it
        return Mathf.Approximately(defaultSpeed, normalizedSpeed) ? defaultSpeed : normalizedSpeed;
    }

    /// <summary>
    /// Public method to set the alive state of the player.
    /// </summary>
    /// <param name="alive">Whether the player is alive</param>
    public void SetAlive(bool alive)
    {
        this.isAlive = alive;
    }

    /// <summary>
    /// Public method to get the alive state of the player.
    /// </summary>
    /// <returns>True if the player is alive</returns>
    public bool GetAlive()
    {
        return this.isAlive;
    }

    /// <summary>
    /// Get the current ground speed from the Surface Effector 2D.
    /// </summary>
    /// <returns>Current ground speed or 0 if no Surface Effector 2D</returns>
    private float GetGroundSpeed()
    {
        return this.groundSurfaceEffector != null ? this.groundSurfaceEffector.speed : 0f;
    }

    /// <summary>
    /// Set the ground speed on the Surface Effector 2D.
    /// </summary>
    /// <param name="newSpeed">New speed value</param>
    private void SetGroundSpeed(float newSpeed)
    {
        if (this.groundSurfaceEffector != null)
        {
            this.groundSurfaceEffector.speed = Mathf.Clamp(newSpeed, this.minSpeed, this.maxSpeed);
        }    }

    /// <summary>
    /// Public method to get current ground speed from Surface Effector 2D.
    /// </summary>
    /// <returns>Current ground speed or 0 if no Surface Effector 2D</returns>
    public float GetCurrentGroundSpeed()    {
        return this.GetGroundSpeed();
    }
}
