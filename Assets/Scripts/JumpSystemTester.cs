using UnityEngine;

/// <summary>
/// Helper script to test and verify the jump system behavior
/// </summary>
public class JumpSystemTester : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private KeyCode testGroundedKey = KeyCode.G;
    [SerializeField] private KeyCode testAirborneKey = KeyCode.A;
    [SerializeField] private KeyCode resetJumpsKey = KeyCode.R;
    
    private Jump jumpScript;
    private Rigidbody2D rb;
    
    private void Start()
    {
        jumpScript = GetComponent<Jump>();
        rb = GetComponent<Rigidbody2D>();
        
        if (jumpScript == null)
        {
            Debug.LogError("JumpSystemTester: No Jump script found on this GameObject!");
        }
        
        if (rb == null)
        {
            Debug.LogError("JumpSystemTester: No Rigidbody2D found on this GameObject!");
        }
        
        Debug.Log("=== JUMP SYSTEM TESTER INITIALIZED ===");
        Debug.Log($"Press {testGroundedKey} to test grounded state");
        Debug.Log($"Press {testAirborneKey} to test airborne state");
        Debug.Log($"Press {resetJumpsKey} to reset jump counter");
        Debug.Log("Press SPACE for normal jumps, S for backward jumps");
    }
    
    private void Update()
    {
        if (jumpScript == null) return;
        
        // Display current status
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            DisplayCurrentStatus();
        }
        
        // Test grounded state
        if (Input.GetKeyDown(testGroundedKey))
        {
            TestGroundedJumps();
        }
        
        // Test airborne state
        if (Input.GetKeyDown(testAirborneKey))
        {
            TestAirborneJumps();
        }
        
        // Reset jump counter
        if (Input.GetKeyDown(resetJumpsKey))
        {
            jumpScript.ResetJumps();
            Debug.Log("Jump counter reset via tester");
        }
    }
    
    private void DisplayCurrentStatus()
    {
        Debug.Log("=== CURRENT JUMP SYSTEM STATUS ===");
        Debug.Log($"Is Grounded: {jumpScript.IsGrounded()}");
        Debug.Log($"Current Air Jumps: {jumpScript.GetCurrentJumps()}/{jumpScript.GetMaxJumps()}");
        Debug.Log($"Velocity: {rb.linearVelocity}");
        Debug.Log($"Can Jump: {jumpScript.IsGrounded() || jumpScript.GetCurrentJumps() < jumpScript.GetMaxJumps()}");
        Debug.Log("=====================================");
    }
    
    private void TestGroundedJumps()
    {
        Debug.Log("=== TESTING GROUNDED JUMPS ===");
        
        // Simulate being grounded
        jumpScript.OnGroundEnter();
        
        Debug.Log("Player set to grounded state");
        Debug.Log("Should be able to jump unlimited times (with cooldown)");
        Debug.Log("Try pressing SPACE multiple times rapidly!");
    }
    
    private void TestAirborneJumps()
    {
        Debug.Log("=== TESTING AIRBORNE JUMPS ===");
        
        // Simulate being airborne
        jumpScript.OnGroundExit();
        
        // Make sure we're in the air by adding upward velocity
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 5f);
        }
        
        Debug.Log("Player set to airborne state");
        Debug.Log("Should be limited to 3 air jumps total");
        Debug.Log("Try pressing SPACE - should work 3 times then stop!");
    }
    
    [ContextMenu("Force Test Grounded State")]
    private void ForceTestGrounded()
    {
        TestGroundedJumps();
    }
    
    [ContextMenu("Force Test Airborne State")]
    private void ForceTestAirborne()
    {
        TestAirborneJumps();
    }
    
    [ContextMenu("Display Status")]
    private void ForceDisplayStatus()
    {
        DisplayCurrentStatus();
    }
}
