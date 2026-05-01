using UnityEngine;

public class CustomCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player; 

    [Header("Camera Offsets")]
    public float maxDistance = 4f; 
    public float height = 2f;   
    
    [Header("Collision & Smoothing")]
    public LayerMask obstacleLayer; 
    public float cameraCollisionRadius = 0.3f; 
    public float positionSmoothTime = 0.05f; 
    public float rotationSmoothSpeed = 10f;

    private Vector3 currentVelocity;
    private float currentDistance;
    
    private Vector3 smoothedLookDir;
    private Vector3 smoothedUpDir;

    private void Start()
    {
        currentDistance = maxDistance;
        smoothedLookDir = transform.forward;
        smoothedUpDir = transform.up;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // 1. POSITION & COLLISION LOGIC
        Vector3 idealPosition = player.position - (player.forward * maxDistance) + (player.up * height);
        Vector3 focusPoint = player.position + (player.up * 1.5f); 
        
        Vector3 directionToCamera = (idealPosition - focusPoint).normalized;
        float expectedMaxDistance = (idealPosition - focusPoint).magnitude;
        float targetDistance = expectedMaxDistance;

        if (Physics.SphereCast(focusPoint, cameraCollisionRadius, directionToCamera, out RaycastHit hit, expectedMaxDistance, obstacleLayer))
        {
            targetDistance = hit.distance;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 15f);

        Vector3 finalPosition = focusPoint + (directionToCamera * currentDistance);
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, positionSmoothTime);

        // 2. ANTI-SNAP ROTATION LOGIC
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
        // .Slerp and guarantees a clean, twist-free pan.
        smoothedLookDir = Vector3.Slerp(smoothedLookDir, targetLookDir, Time.deltaTime * rotationSmoothSpeed).normalized;
        smoothedUpDir = Vector3.Slerp(smoothedUpDir, targetUpDir, Time.deltaTime * rotationSmoothSpeed).normalized;

        transform.rotation = Quaternion.LookRotation(smoothedLookDir, smoothedUpDir);
    }
}