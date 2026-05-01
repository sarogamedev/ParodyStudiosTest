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

    [Tooltip("Pushes the hologram out in front of the player so it doesn't overlap them")]
    public float hologramForwardOffset = 1.5f;

    [HideInInspector] public Vector3 currentGravityDir = Vector3.down;
    private Vector3 targetGravityDir = Vector3.down;
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
        if (Input.GetKey(KeyCode.UpArrow)) inputDir = mainCamera.forward;
        else if (Input.GetKey(KeyCode.DownArrow)) inputDir = -mainCamera.forward;
        else if (Input.GetKey(KeyCode.RightArrow)) inputDir = mainCamera.right;
        else if (Input.GetKey(KeyCode.LeftArrow)) inputDir = -mainCamera.right;

        if (inputDir != Vector3.zero)
        {
            targetGravityDir = SnapToAxis(inputDir);
            Vector3 rayOrigin = transform.position + (transform.up * 0.5f); 
        
            // Only trigger if we actually hit a valid wall/floor
            if (Physics.Raycast(rayOrigin, targetGravityDir, out RaycastHit hit, 50f, groundLayer))
            {
                isPreviewing = true;
                if (hologram != null) hologram.SetActive(true);

                Quaternion futureRotation = Quaternion.FromToRotation(transform.up, -targetGravityDir) * transform.rotation;
                Vector3 futureForward = futureRotation * Vector3.forward;

                Vector3 targetPosition = hit.point 
                                         + (hit.normal * hologramSurfaceOffset) 
                                         + (futureForward * hologramForwardOffset);

                hologram.transform.position = targetPosition;
                hologram.transform.rotation = Quaternion.Slerp(hologram.transform.rotation, futureRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                // Aiming at the void. Hide hologram and cancel preview.
                isPreviewing = false;
                if (hologram != null) hologram.SetActive(false);
            }
        }
        else if (isPreviewing && !Input.GetKeyDown(KeyCode.E))
        {
            isPreviewing = false;
            if (hologram != null) hologram.SetActive(false);
        }

        if (isPreviewing && Input.GetKeyDown(KeyCode.E))
        {
            currentGravityDir = targetGravityDir;
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