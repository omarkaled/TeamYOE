using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float airControlMultiplier = 0.4f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;

    [Header("Grounding")]
    [SerializeField] private Transform jumpChecker;
    [SerializeField] private float jumpCheckerRadius = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpCooldown = 1f;

    [Header("References")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool canJump = true;
    private bool isRagdoll = false;
    private MovingPlatform currentPlatform;
    [SerializeField] private Transform hipsBone;
    [SerializeField] private Rigidbody[] ragdollRigidbodies;
    [SerializeField] private Animator playerAnimator;
    

    public enum PlayerStates
    {
        FullControl,
        AirControl,
        Ragdoll
    }
    public PlayerStates CurrentState = PlayerStates.FullControl;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleInput();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ActivateRagdoll();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DeactivateRagdoll();
        }

        // Ground check
        isGrounded = Physics.OverlapSphere(jumpChecker.position, jumpCheckerRadius, groundLayer).Length > 0;

        // State management
        switch (CurrentState)
        {
            case PlayerStates.Ragdoll:
                currentPlatform = null;
                isRagdoll = true;
                return;

            case PlayerStates.FullControl:
            case PlayerStates.AirControl:
                if (isGrounded)
                {
                    CurrentState = PlayerStates.FullControl;
                    DetectGroundPlatform();
                }
                else
                {
                    CurrentState = PlayerStates.AirControl;
                    currentPlatform = null;
                }
                break;
        }
    }

    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cameraTransform.right;

        Vector3 inputDir = (camRight * h + camForward * v).normalized;

        switch (CurrentState)
        {
            case PlayerStates.FullControl:
                moveDirection = inputDir;
                HandleJump();
                break;

            case PlayerStates.AirControl:
                moveDirection = inputDir * airControlMultiplier;
                break;

            case PlayerStates.Ragdoll:
                moveDirection = Vector3.zero;
                break;
        }
    }

    private void LateUpdate()
    {
        if (isRagdoll)
        {
            //transform.position = hipsBone.position;
        }
    }

    void ActivateRagdoll()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isRagdoll = true;
        // Disable player controller
        rb.isKinematic = true;

        // Enable ragdoll physics
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.linearVelocity = Vector3.zero; // <- this is important
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            Collider collider = rb.GetComponent<Collider>();
            collider.enabled = true;
        }
        playerAnimator.enabled = false;
    }

    void DeactivateRagdoll()
    {
        isRagdoll = false;
        // Disable player controller
        rb.isKinematic = false;

        // Enable ragdoll physics
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = true;
            Collider collider = rb.GetComponent<Collider>();
            collider.enabled = false;
        }
        playerAnimator.enabled = true;
    }
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpCooldownRoutine());
        }
    }

    private IEnumerator JumpCooldownRoutine()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    void FixedUpdate()
    {
        if (CurrentState == PlayerStates.Ragdoll)
            return;

        Vector3 velocity = moveDirection * moveSpeed;
        Vector3 newVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // Apply platform velocity if grounded
        if (isGrounded && currentPlatform != null)
        {
            Vector3 platformVelocity = currentPlatform.Velocity;
            newVelocity += new Vector3(platformVelocity.x, 0f, platformVelocity.z);
        }

        rb.linearVelocity = newVelocity;

        // Rotate towards move direction
        if (moveDirection.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRotation);
        }
    }

    private void DetectGroundPlatform()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.75f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f, groundLayer))
        {
            if (hit.collider.TryGetComponent(out MovingPlatform platform))
            {
                currentPlatform = platform;
                return;
            }
        }

        currentPlatform = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (jumpChecker != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(jumpChecker.position, jumpCheckerRadius);
        }
    }
}

