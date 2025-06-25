using UnityEngine;
using TMPro;

/// <summary>
/// Flag component that triggers win condition when player reaches it
/// </summary>
public class Flag : MonoBehaviour
{
    [Header("Win UI")]
    [SerializeField] private GameObject winCanvas; // Drag the win canvas here (can reuse game over canvas)
    [SerializeField] private TextMeshProUGUI winText; // Drag the text that will show "WIN!" (can reuse game over text)
    [SerializeField] private TextMeshProUGUI retryText; // Drag the retry button text here
    
    [Header("Win Settings")]
    [SerializeField] private string winMessage = "YOU WIN!"; // Message to show when player wins
    [SerializeField] private string retryButtonText = "NEXT LEVEL"; // Text for the retry button when winning
    [SerializeField] private bool onlyTriggerWithPlayer = true; // Only respond to player collisions
    
    [Header("Audio")]
    [SerializeField] private AudioClip winAudio; // Optional win sound
    
    private bool hasWon = false;    private void Start()
    {
        // Make sure this flag has a trigger collider
        Collider2D flagCollider = GetComponent<Collider2D>();
        if (flagCollider != null)
        {
            if (!flagCollider.isTrigger)
            {
                flagCollider.isTrigger = true;
                Debug.Log($"Flag {gameObject.name}: Set collider to trigger mode");
            }
        }
        else
        {
            Debug.LogError($"Flag {gameObject.name}: No Collider2D found! Please add a Collider2D component.");
        }
        
        // Make sure win canvas is hidden at start
        if (winCanvas != null)
        {
            winCanvas.SetActive(false);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasWon) return; // Already won
        
        // Debug: Show what object collided with the flag
        Debug.Log($"Flag '{gameObject.name}' triggered by: '{other.name}' with tag: '{other.tag}' on GameObject: '{other.gameObject.name}'");
        
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
                Debug.Log($"✓ Player reached the flag: {gameObject.name}");
                TriggerWin();
            }
            else
            {
                Debug.Log($"✗ Not a player - ignoring collision. Object tag: '{other.tag}', GameObject tag: '{other.gameObject.tag}'");
            }
        }
        else
        {
            // Trigger win regardless of what touched it
            Debug.Log($"✓ Flag triggered by: {other.name}");
            TriggerWin();
        }
    }
    
    /// <summary>
    /// Trigger the win condition
    /// </summary>
    private void TriggerWin()
    {
        if (hasWon) return;
        
        hasWon = true;
        
        Debug.Log("Player Won! Level Complete!");
        
        // Play win audio if assigned
        if (winAudio != null)
        {
            AudioSource.PlayClipAtPoint(winAudio, transform.position);
            Debug.Log("Win audio played");
        }
          // Update final score display through ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateFinalScoreDisplay();
            ScoreManager.Instance.SetLevelWon(); // Mark level as won
        }
        
        // Show win UI
        ShowWinUI();
        
        // Pause the game
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Show the win UI with customized text
    /// </summary>
    private void ShowWinUI()
    {
        // Show and enable win canvas
        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
            
            // Also enable the Canvas component if it's disabled
            Canvas canvas = winCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                canvas.sortingOrder = 100; // Put it on top
                Debug.Log("Win canvas enabled and moved to front");
            }
            
            Debug.Log("Win canvas activated");
        }
        else
        {
            Debug.LogError("Win canvas is not assigned!");
        }
        
        // Update win message text
        if (winText != null)
        {
            winText.text = winMessage;
            Debug.Log($"Win text updated: {winMessage}");
        }
        
        // Update retry button text for win condition
        if (retryText != null)
        {
            retryText.text = retryButtonText;
            Debug.Log($"Retry button text updated: {retryButtonText}");
        }
    }
    
    /// <summary>
    /// Manual method to trigger win (for testing)
    /// </summary>
    [ContextMenu("Trigger Win")]
    public void TestWin()
    {
        TriggerWin();
    }
}
