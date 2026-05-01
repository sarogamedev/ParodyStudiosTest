using UnityEngine;

/// <summary>
/// Handles the core player movement, jumping, and interactions.
/// Relies on Rigidbody physics and respects custom gravity directions provided by the GravityController.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GravityController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Forward and backward movement speed.")]
    public float moveSpeed = 5f;
    [Tooltip("Rotation speed for steering left and right.")]
    public float turnSpeed = 150f;
    [Tooltip("Force applied upwards when jumping.")]
    public float jumpForce = 5f;
    
    [Header("Ground Check")]
    [Tooltip("Distance below the player to check for the ground.")]
    public float groundCheckDistance = 0.2f;
    [Tooltip("Layer mask defining what surfaces count as the ground.")]
    public LayerMask groundLayer;

    private Rigidbody rb;
    private GravityController gravityCtrl;
    private Animator anim;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityCtrl = GetComponent<GravityController>();
        anim = GetComponentInChildren<Animator>(); 
    }

    private void Update()
    {
        // Handle input and checks in Update
        CheckGrounded();
        HandleJump();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Handle physics movement in FixedUpdate
        HandleMovement();
    }

    /// <summary>
    /// Processes A/D for steering and W/S for moving forward/backward.
    /// Modifies rigidbody velocity while stripping out gravity interference.
    /// </summary>
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A, D keys (Steering)
        float v = Input.GetAxisRaw("Vertical");   // W, S keys (Forward/Backward)

        // 1. Steering (A/D only). This rotates the player around their local Up axis.
        if (h != 0)
        {
            Quaternion turnRotation = Quaternion.AngleAxis(h * turnSpeed * Time.fixedDeltaTime, transform.up);
            transform.rotation = turnRotation * transform.rotation;
        }

        // 2. Forward/Backward (W/S only). This calculates the desired movement direction.
        Vector3 moveDir = (transform.forward * v).normalized;
        
        Vector3 targetVelocity = moveDir * moveSpeed;
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        
        // Strip out any velocity change applied along the gravity axis to avoid fighting gravity
        velocityChange -= Vector3.Project(velocityChange, gravityCtrl.currentGravityDir);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Makes the player jump if the spacebar is pressed and they are grounded.
    /// </summary>
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Casts a ray downwards to determine if the player is touching the ground.
    /// </summary>
    private void CheckGrounded()
    {
        // Raycast starting slightly above the base of the player and shooting downwards
        isGrounded = Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, groundCheckDistance + 0.1f, groundLayer);
    }

    /// <summary>
    /// Updates the animator with current speed and grounded status.
    /// </summary>
    private void UpdateAnimations()
    {
        if (anim != null)
        {
            // Calculate horizontal speed (ignoring vertical movement due to gravity)
            Vector3 horizontalVelocity = rb.linearVelocity - Vector3.Project(rb.linearVelocity, gravityCtrl.currentGravityDir);
            anim.SetFloat("Speed", horizontalVelocity.magnitude); 
            anim.SetBool("IsGrounded", isGrounded);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle picking up collectibles
        if (other.CompareTag("Collectible"))
        {
            GameManager.Instance.CollectCube();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Handle falling off the map into kill volumes
        if (other.CompareTag("KillVolume"))
        {
            GameManager.Instance.TriggerGameOver(false);
        }
    }
}