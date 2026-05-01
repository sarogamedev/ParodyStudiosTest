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
    [Tooltip("Gives the camera thickness so it doesn't snag on sharp corners")]
    public float cameraCollisionRadius = 0.3f; 
    public float positionSmoothTime = 0.05f; 
    public float rotationSmoothSpeed = 10f;

    private Vector3 currentVelocity;
    private float currentDistance;

    private void Start()
    {
        currentDistance = maxDistance;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // 1. Calculate the raw ideal position behind the player
        Vector3 idealPosition = player.position - (player.forward * maxDistance) + (player.up * height);
        Vector3 focusPoint = player.position + (player.up * 1.5f); 
        
        // 2. CAMERA COLLISION (SphereCast / Spring Arm)
        Vector3 directionToCamera = (idealPosition - focusPoint).normalized;
        float expectedMaxDistance = (idealPosition - focusPoint).magnitude;
        float targetDistance = expectedMaxDistance;

        // We cast a thick sphere instead of a thin ray to slide on walls cleanly
        if (Physics.SphereCast(focusPoint, cameraCollisionRadius, directionToCamera, out RaycastHit hit, expectedMaxDistance, obstacleLayer))
        {
            targetDistance = hit.distance;
        }

        // Smoothly reel the distance in and out to prevent snapping
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 15f);

        // 3. Final Position using the smoothed distance
        Vector3 finalPosition = focusPoint + (directionToCamera * currentDistance);
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, positionSmoothTime);

        // 4. Look at the player smoothly
        Vector3 lookDirection = focusPoint - transform.position;
        if (lookDirection != Vector3.zero) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, player.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        }
    }
}