using UnityEngine;

/// <summary>
/// Component for star objects - handles collision detection
/// </summary>
public class Star : MonoBehaviour
{
    [Header("Star Settings")]
    [SerializeField] private bool onlyTriggerWithPlayer = true; // Only respond to player collisions
    
    private void Start()
    {
        // Make sure this star has a trigger collider
        Collider2D starCollider = GetComponent<Collider2D>();
        if (starCollider != null)
        {
            if (!starCollider.isTrigger)
            {
                starCollider.isTrigger = true;
                Debug.Log($"Star {gameObject.name}: Set collider to trigger mode");
            }
        }
        else
        {
            Debug.LogError($"Star {gameObject.name}: No Collider2D found! Please add a Collider2D component.");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug: Show what object collided with the star
        Debug.Log($"Star '{gameObject.name}' triggered by: '{other.name}' with tag: '{other.tag}' on GameObject: '{other.gameObject.name}'");
        
        // Check if we should only respond to player
        if (onlyTriggerWithPlayer)
        {
            // Check if the colliding object or its parent has the "Player" tag
            bool isPlayer = other.CompareTag("Player") || other.gameObject.CompareTag("Player");
            
            if (!isPlayer && other.transform.parent != null)
            {
                isPlayer = other.transform.parent.CompareTag("Player");
            }
            
            if (isPlayer)
            {
                Debug.Log($"✓ Player collected star: {gameObject.name}");
                CollectThisStar();
            }
            else
            {
                Debug.Log($"✗ Not a player - ignoring collision. Object tag: '{other.tag}', GameObject tag: '{other.gameObject.tag}'");
            }
        }
        else
        {
            // Collect star regardless of what triggered it
            Debug.Log($"✓ Star collected by: {other.name}");
            CollectThisStar();
        }
    }
    
    /// <summary>
    /// Collect this star and notify the ScoreManager
    /// </summary>
    private void CollectThisStar()
    {
        // Tell the ScoreManager to collect this star
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.CollectStar(gameObject);
        }
        else
        {
            Debug.LogError("ScoreManager not found! Make sure there's a ScoreManager in the scene.");
        }
    }
    
    /// <summary>
    /// Manual method to collect this star (for testing)
    /// </summary>
    [ContextMenu("Collect This Star")]
    public void TestCollectStar()
    {
        CollectThisStar();
    }
}
