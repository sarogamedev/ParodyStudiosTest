using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GravityController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;
    public float jumpForce = 5f;
    
    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
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
        CheckGrounded();
        HandleJump();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A, D keys (Steering)
        float v = Input.GetAxisRaw("Vertical");   // W, S keys (Forward/Backward)

        // 1. Steering (A/D only). This rotates the player.
        if (h != 0)
        {
            Quaternion turnRotation = Quaternion.AngleAxis(h * turnSpeed * Time.fixedDeltaTime, transform.up);
            transform.rotation = turnRotation * transform.rotation;
        }

        // 2. Forward/Backward (W/S only). This moves the player without rotating them.
        Vector3 moveDir = (transform.forward * v).normalized;
        
        Vector3 targetVelocity = moveDir * moveSpeed;
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        
        // Strip out any velocity change applied along the gravity axis
        velocityChange -= Vector3.Project(velocityChange, gravityCtrl.currentGravityDir);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, groundCheckDistance + 0.1f, groundLayer);
    }

    private void UpdateAnimations()
    {
        if (anim != null)
        {
            Vector3 horizontalVelocity = rb.linearVelocity - Vector3.Project(rb.linearVelocity, gravityCtrl.currentGravityDir);
            anim.SetFloat("Speed", horizontalVelocity.magnitude); 
            anim.SetBool("IsGrounded", isGrounded);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            GameManager.Instance.CollectCube();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("KillVolume"))
        {
            GameManager.Instance.TriggerGameOver(false);
        }
    }
}