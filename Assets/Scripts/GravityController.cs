using UnityEngine;

/// <summary>
/// Allows the player to change the direction of gravity.
/// Projects a hologram to preview the new surface they will fall to, 
/// and handles rotating the player to match the new gravity alignment.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    [Header("References")] 
    [Tooltip("The visual indicator for where the player will land after a gravity shift.")]
    public GameObject hologram;
    [Tooltip("The camera determining relative directional inputs (e.g., Up Arrow = Forward relative to camera).")]
    public Transform mainCamera;

    [Header("Settings")] 
    [Tooltip("The strength of the custom gravity applied to the Rigidbody.")]
    public float gravityForce = 9.81f;
    [Tooltip("How fast the player or hologram rotates to align with the new gravity.")]
    public float rotationSpeed = 50f;
    [Tooltip("Layers the raycast considers solid surfaces for gravity shifting.")]
    public LayerMask groundLayer;

    [Tooltip("Increase this if the hologram clips into the floor/walls")]
    public float hologramSurfaceOffset = 0.05f;

    [Tooltip("How far the hologram pushes out based on input direction")]
    public float hologramForwardOffset = 1.5f;

    [HideInInspector] public Vector3 currentGravityDir = Vector3.down;
    private Vector3 targetGravityDir = Vector3.down;

    // Remembers the local offset direction based on the arrow key pressed
    private Vector3 targetLocalOffset = Vector3.forward;

    private Rigidbody rb;
    private bool isPreviewing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Disable built-in gravity since this script handles custom directional gravity
        rb.useGravity = false;
        
        if (hologram != null) hologram.SetActive(false);
    }

    private void Update()
    {
        HandleGravityInput();
        AlignPlayerToGravity();
    }

    private void FixedUpdate()
    {
        // Apply continuous acceleration force based on the current custom gravity direction
        rb.AddForce(currentGravityDir * gravityForce, ForceMode.Acceleration);
    }

    /// <summary>
    /// Processes arrow keys to project the hologram and 'E' key to confirm the gravity shift.
    /// </summary>
    private void HandleGravityInput()
    {
        Vector3 inputDir = Vector3.zero;

        // Determine Gravity Direction AND Local Offset Direction relative to camera
        if (Input.GetKey(KeyCode.UpArrow))
        {
            inputDir = mainCamera.forward;
            targetLocalOffset = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            inputDir = -mainCamera.forward;
            targetLocalOffset = Vector3.forward; // Use forward to project hologram away from camera
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            inputDir = mainCamera.right;
            targetLocalOffset = Vector3.right;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            inputDir = -mainCamera.right;
            targetLocalOffset = Vector3.left;
        }

        if (inputDir != Vector3.zero)
        {
            // Lock the gravity change to the nearest major world axis (X, Y, or Z)
            targetGravityDir = SnapToAxis(inputDir);
            
            // Cast a ray from slightly above the player's base in the direction of the new gravity
            Vector3 rayOrigin = transform.position + (transform.up * 0.5f);

            if (Physics.Raycast(rayOrigin, targetGravityDir, out RaycastHit hit, 50f, groundLayer))
            {
                isPreviewing = true;
                if (hologram != null) hologram.SetActive(true);

                // Calculate the rotation the player will have when they land
                Quaternion exactFutureRotation = Quaternion.FromToRotation(transform.up, -targetGravityDir) * transform.rotation;
                
                // Calculate where the hologram should sit on the surface, pushing it outwards based on the input key
                Vector3 appliedOffsetDir = exactFutureRotation * targetLocalOffset;
                Vector3 targetPosition = hit.point
                                         + (hit.normal * hologramSurfaceOffset)
                                         + (appliedOffsetDir * hologramForwardOffset);

                hologram.transform.position = targetPosition;
                // Smoothly rotate the hologram towards the intended landing rotation
                hologram.transform.rotation = Quaternion.Slerp(hologram.transform.rotation, exactFutureRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                // If there's no surface to fall to in that direction, disable the preview
                isPreviewing = false;
                if (hologram != null) hologram.SetActive(false);
            }
        }
        else if (isPreviewing && !Input.GetKeyDown(KeyCode.E))
        {
            // Turn off hologram if keys are released
            isPreviewing = false;
            if (hologram != null) hologram.SetActive(false);
        }

        // Confirm Gravity Shift & Apply Forward Force
        if (isPreviewing && Input.GetKeyDown(KeyCode.E))
        {
            currentGravityDir = targetGravityDir;

            // Give the player a push towards the surface they are falling to 
            // relative to the hologram's facing direction to maintain momentum
            float forwardPushStrength = 20f;
            rb.linearVelocity = hologram.transform.forward * forwardPushStrength;

            isPreviewing = false;
            if (hologram != null) hologram.SetActive(false);
        }
    }

    /// <summary>
    /// Smoothly rotates the actual player object to align its 'Up' vector with the opposite of gravity.
    /// </summary>
    private void AlignPlayerToGravity()
    {
        Vector3 targetUp = -currentGravityDir;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// Helper method to ensure gravity only ever points exactly along X, Y, or Z axes.
    /// </summary>
    private Vector3 SnapToAxis(Vector3 v)
    {
        float x = Mathf.Abs(v.x);
        float y = Mathf.Abs(v.y);
        float z = Mathf.Abs(v.z);
        if (x > y && x > z) return new Vector3(Mathf.Sign(v.x), 0, 0);
        if (y > x && y > z) return new Vector3(0, Mathf.Sign(v.y), 0);
        return new Vector3(0, 0, Mathf.Sign(v.z));
    }
}