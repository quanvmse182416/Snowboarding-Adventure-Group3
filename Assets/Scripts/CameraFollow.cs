using UnityEngine;

/// <summary>
/// Simple camera follow script that follows a target player with configurable zoom
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget; // Drag the player GameObject here
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -10);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float zoomOutSize = 10f;
    
    [Header("Options")]
    [SerializeField] private bool smoothFollow = true;
    
    private Camera cam;
    
    private void Awake()
    {
        // Get the camera component
        cam = Camera.main;        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
        }
        
        if (cam == null)
        {
            Debug.LogError("CameraFollow: No camera found!");
            return;
        }
        
        // Set initial zoom
        if (cam.orthographic)
        {
            cam.orthographicSize = zoomOutSize;
        }
    }
    
    private void Start()
    {
        // Snap to player position immediately on start
        if (playerTarget != null && cam != null)
        {
            SnapToPlayer();
        }
    }
    
    private void LateUpdate()
    {
        if (playerTarget == null || cam == null) return;
        
        FollowPlayer();
        
        // Always keep camera rotation straight
        cam.transform.rotation = Quaternion.identity;
        
        // Ensure zoom stays at desired size
        if (cam.orthographic)
        {
            cam.orthographicSize = zoomOutSize;
        }
    }    /// <summary>
    /// Follow the player with the camera
    /// </summary>
    private void FollowPlayer()
    {
        // Calculate target position with offset
        Vector3 playerPosition = playerTarget.position;
        Vector3 targetPosition = new Vector3(
            playerPosition.x + cameraOffset.x,  // Player X + offset X
            playerPosition.y + cameraOffset.y,  // Player Y + offset Y  
            cameraOffset.z                      // Use offset Z directly (should be -10)
        );
        
        if (smoothFollow)
        {
            Vector3 newPosition = Vector3.Lerp(
                cam.transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );
            cam.transform.position = newPosition;
        }
        else
        {
            cam.transform.position = targetPosition;
        }
        
        // Debug log to see actual positions every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Player: {playerPosition}, Camera Target: {targetPosition}, Camera Actual: {cam.transform.position}");
        }
    }    /// <summary>
    /// Instantly snap camera to player position
    /// </summary>
    public void SnapToPlayer()
    {
        if (playerTarget != null && cam != null)
        {
            Vector3 playerPosition = playerTarget.position;
            Vector3 snapPosition = new Vector3(
                playerPosition.x + cameraOffset.x,  // Player X + offset X
                playerPosition.y + cameraOffset.y,  // Player Y + offset Y
                cameraOffset.z                      // Use offset Z directly (should be -10)
            );
            
            cam.transform.position = snapPosition;
            cam.transform.rotation = Quaternion.identity;
            
            Debug.Log($"Camera snapped! Player: {playerPosition}, Camera: {snapPosition}");
        }
    }
    
    /// <summary>
    /// Set the zoom out size at runtime
    /// </summary>
    /// <param name="newSize">New camera size</param>
    public void SetZoomOutSize(float newSize)
    {
        zoomOutSize = newSize;
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = zoomOutSize;
        }
    }
    
    /// <summary>
    /// Set the camera offset at runtime
    /// </summary>
    /// <param name="newOffset">New camera offset</param>
    public void SetCameraOffset(Vector3 newOffset)
    {
        cameraOffset = newOffset;
    }
    
    /// <summary>
    /// Set the player target at runtime
    /// </summary>
    /// <param name="newTarget">New player target to follow</param>
    public void SetPlayerTarget(Transform newTarget)
    {
        playerTarget = newTarget;
    }

    /// <summary>
    /// Validate camera setup and provide debugging info
    /// </summary>
    private void OnValidate()
    {
        // This runs in the editor when values change
        if (Application.isPlaying && playerTarget != null)
        {
            Debug.Log($"CameraFollow Setup: Offset={cameraOffset}, ZoomSize={zoomOutSize}");
            
            // Check if Z offset is appropriate for 2D
            if (cameraOffset.z >= 0)
            {
                Debug.LogWarning("Camera Z offset should be negative for 2D games! Recommended: -10");
            }
        }
    }
    
    /// <summary>
    /// Public method to manually update camera position - useful for testing
    /// </summary>
    [ContextMenu("Update Camera Position Now")]
    public void UpdateCameraPositionNow()
    {
        if (playerTarget != null && cam != null)
        {
            Vector3 playerPos = playerTarget.position;
            Vector3 newCameraPos = new Vector3(
                playerPos.x + cameraOffset.x,
                playerPos.y + cameraOffset.y, 
                cameraOffset.z
            );
            
            cam.transform.position = newCameraPos;
            Debug.Log($"Manual Update - Player: {playerPos}, Camera set to: {newCameraPos}");
        }
    }
}
