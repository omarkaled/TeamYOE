using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;

    [Header("References")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private Vector3 moveDirection;
    [SerializeField] private Transform jumpChecker;
    [SerializeField] private float jumpCheckerRadius = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpCooldown = 1f;
    [SerializeField] private bool canJumpTimeLimit = true;
    private bool canJump;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        canJump = Physics.OverlapSphere(jumpChecker.position, jumpCheckerRadius, groundLayer).Length > 0;
        // Unlock on Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetKeyDown(KeyCode.Space) && canJump && canJumpTimeLimit)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpCooldownRoutine());
        }
        // Input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Camera-relative directions
        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cameraTransform.right;

        // Movement direction (no rotation influence)
        moveDirection = (camRight * h + camForward * v).normalized;
    }

    private IEnumerator JumpCooldownRoutine()
    {
        canJumpTimeLimit = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJumpTimeLimit = true;
    }

    void FixedUpdate()
    {
        // Movement
        Vector3 movement = moveDirection * moveSpeed;
        Vector3 newVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        rb.linearVelocity = newVelocity;

        // Rotation: only if moving forward
        if (Vector3.Dot(moveDirection, cameraTransform.forward) > 0.7f)
        {
            Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            if (camForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForward);
                Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(smoothRotation);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(jumpChecker.position, jumpCheckerRadius);
    }
}
