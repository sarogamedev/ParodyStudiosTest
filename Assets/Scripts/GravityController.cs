using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    [Header("References")] public GameObject hologram;
    public Transform mainCamera;

    [Header("Settings")] public float gravityForce = 9.81f;
    public float rotationSpeed = 50f;
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
        rb.AddForce(currentGravityDir * gravityForce, ForceMode.Acceleration);
    }

    private void HandleGravityInput()
    {
        Vector3 inputDir = Vector3.zero;

        // Determine Gravity Direction AND Local Offset Direction
        if (Input.GetKey(KeyCode.UpArrow))
        {
            inputDir = mainCamera.forward;
            targetLocalOffset = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            inputDir = -mainCamera.forward;
            targetLocalOffset = Vector3.forward;
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
            targetGravityDir = SnapToAxis(inputDir);
            Vector3 rayOrigin = transform.position + (transform.up * 0.5f);

            if (Physics.Raycast(rayOrigin, targetGravityDir, out RaycastHit hit, 50f, groundLayer))
            {
                isPreviewing = true;
                if (hologram != null) hologram.SetActive(true);

                Quaternion exactFutureRotation = Quaternion.FromToRotation(transform.up, -targetGravityDir) * transform.rotation;
                Vector3 appliedOffsetDir = exactFutureRotation * targetLocalOffset;

                Vector3 targetPosition = hit.point
                                         + (hit.normal * hologramSurfaceOffset)
                                         + (appliedOffsetDir * hologramForwardOffset);

                hologram.transform.position = targetPosition;
                hologram.transform.rotation = Quaternion.Slerp(hologram.transform.rotation, exactFutureRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                isPreviewing = false;
                if (hologram != null) hologram.SetActive(false);
            }
        }
        else if (isPreviewing && !Input.GetKeyDown(KeyCode.E))
        {
            isPreviewing = false;
            if (hologram != null) hologram.SetActive(false);
        }

        // Confirm Gravity Shift & Apply Forward Force
        if (isPreviewing && Input.GetKeyDown(KeyCode.E))
        {
            currentGravityDir = targetGravityDir;

            float forwardPushStrength = 20f;
            rb.linearVelocity = hologram.transform.forward * forwardPushStrength;

            isPreviewing = false;
            if (hologram != null) hologram.SetActive(false);
        }
    }

    private void AlignPlayerToGravity()
    {
        Vector3 targetUp = -currentGravityDir;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

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