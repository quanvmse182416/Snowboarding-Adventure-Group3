using UnityEngine;

/// <summary>
/// Helper component for detecting ground collision without affecting player physics
/// </summary>
public class GroundDetector : MonoBehaviour
{
    private Jump jumpComponent;
    private bool isInitialized = false;
    private int groundContactCount = 0;
    
    /// <summary>
    /// Initialize the ground detector with reference to the Jump component
    /// </summary>
    /// <param name="jump">Reference to the Jump component</param>
    public void Initialize(Jump jump)
    {
        jumpComponent = jump;
        isInitialized = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized || jumpComponent == null) return;
        
        // Check if the collided object is ground
        if (IsGround(other))
        {
            groundContactCount++;
            if (groundContactCount == 1) // First ground contact
            {
                jumpComponent.OnGroundEnter();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isInitialized || jumpComponent == null) return;
        
        // Check if the collided object is ground
        if (IsGround(other))
        {
            groundContactCount--;
            groundContactCount = Mathf.Max(0, groundContactCount); // Ensure it doesn't go below 0
            
            if (groundContactCount == 0) // No more ground contacts
            {
                jumpComponent.OnGroundExit();
            }
        }
    }
      private bool IsGround(Collider2D other)
    {
        // Multiple ways to detect ground for better compatibility
        // Check common ground tags and names
        if (other.CompareTag("Ground") || 
            other.CompareTag("Obstacle") || // Obstacles can also act as ground
            other.gameObject.name.Contains("Ground") ||
            other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            return true;
        }
        
        // Additional check for any solid, non-trigger collider that could act as ground
        // This helps with stuck situations where player might be against unnamed obstacles
        if (!other.isTrigger && other.gameObject != jumpComponent.gameObject)
        {
            // Consider it ground if it's a solid object below or around the player
            return true;
        }
        
        return false;
    }
}
