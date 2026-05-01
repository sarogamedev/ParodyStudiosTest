using UnityEngine;

/// <summary>
/// A custom camera controller that follows the player, applying smoothing and handling collision.
/// Ensures the camera does not snap abruptly and maintains a specified distance and height.
/// </summary>
public class CustomCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player transform to follow.")]
    public Transform player; 

    [Header("Camera Offsets")]
    [Tooltip("The maximum distance the camera can be from the player.")]
    public float maxDistance = 4f; 
    [Tooltip("The height offset of the camera relative to the player.")]
    public float height = 2f;   
    
    [Header("Collision & Smoothing")]
    [Tooltip("Layer mask determining which objects the camera can collide with.")]
    public LayerMask obstacleLayer; 
    [Tooltip("The radius of the sphere used for camera collision detection.")]
    public float cameraCollisionRadius = 0.3f; 
    [Tooltip("Smoothing time for the camera's position movement.")]
    public float positionSmoothTime = 0.05f; 
    [Tooltip("Speed at which the camera rotates to face the target.")]
    public float rotationSmoothSpeed = 10f;

    // Internal variables for smoothing
    private Vector3 currentVelocity;
    private float currentDistance;
    
    private Vector3 smoothedLookDir;
    private Vector3 smoothedUpDir;

    private void Start()
    {
        // Initialize distances and directions
        currentDistance = maxDistance;
        smoothedLookDir = transform.forward;
        smoothedUpDir = transform.up;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // 1. POSITION & COLLISION LOGIC
        
        // Calculate the ideal position and the point the camera should focus on
        Vector3 idealPosition = player.position - (player.forward * maxDistance) + (player.up * height);
        Vector3 focusPoint = player.position + (player.up * 1.5f); 
        
        Vector3 directionToCamera = (idealPosition - focusPoint).normalized;
        float expectedMaxDistance = (idealPosition - focusPoint).magnitude;
        float targetDistance = expectedMaxDistance;

        // Check for collisions between the focus point and the ideal camera position
        if (Physics.SphereCast(focusPoint, cameraCollisionRadius, directionToCamera, out RaycastHit hit, expectedMaxDistance, obstacleLayer))
        {
            // If an obstacle is hit, adjust the target distance to the hit point
            targetDistance = hit.distance;
        }

        // Smoothly interpolate the current distance towards the target distance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 15f);

        // Apply the final calculated position
        Vector3 finalPosition = focusPoint + (directionToCamera * currentDistance);
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, positionSmoothTime);

        // 2. ANTI-SNAP ROTATION LOGIC
        
        // Calculate target looking and up directions based on the player's position and orientation
        Vector3 targetLookDir = (focusPoint - transform.position).normalized;
        Vector3 targetUpDir = player.up;
        
        if (targetLookDir == Vector3.zero) targetLookDir = player.forward;

        // Safeguard against Gimbal Lock (if looking almost directly parallel to the Up axis)
        if (Mathf.Abs(Vector3.Dot(targetLookDir, targetUpDir)) > 0.98f)
        {
            targetLookDir += player.forward * 0.05f;
            targetLookDir.Normalize();
        }

        // Initialize vectors safely if they are empty
        if (smoothedLookDir == Vector3.zero)
        {
            smoothedLookDir = transform.forward;
            smoothedUpDir = transform.up;
        }

        // Smooth the vectors independently. 
        // Slerp guarantees a clean, twist-free pan.
        smoothedLookDir = Vector3.Slerp(smoothedLookDir, targetLookDir, Time.deltaTime * rotationSmoothSpeed).normalized;
        smoothedUpDir = Vector3.Slerp(smoothedUpDir, targetUpDir, Time.deltaTime * rotationSmoothSpeed).normalized;

        // Apply final smoothed rotation
        transform.rotation = Quaternion.LookRotation(smoothedLookDir, smoothedUpDir);
    }
}