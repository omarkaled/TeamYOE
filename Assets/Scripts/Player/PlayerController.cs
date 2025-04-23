using JetBrains.Rider.Unity.Editor;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] private GameObject _jumpVFXPrefab;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool canJump = true;
    private MovingPlatform currentPlatform;
    [SerializeField] private Transform hipsBone;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject playerMesh;

    [Header("Ragdoll")]
    [SerializeField] private Rigidbody[] ragdollRigidbodies;
    [SerializeField] private Transform cylinderCenter;
    [SerializeField] private float ragdollRecoveryTime = 2f;

    public bool isOnSafePlatform = false;
    private Coroutine ragdollCooldownRoutine;
    public bool canBeRagdolledByBullets = true;

    [Header("Events")]
    public UnityEvent OnRagdollActivate;
    public UnityEvent OnRagdollDeactivate;

    [Header("Don't worry about these")]
    [SerializeField] private float safeAreaBottomMin = 220f;
    [SerializeField] private float safeAreaBottomMax = 320f;

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
        StartCoroutine(PositionUpdater());
    }
    private IEnumerator PositionUpdater()
    {
        while(true)
        {
            yield return new WaitForSeconds(2);
            //rb.position = hipsBone.position - new Vector3(0,0.25f,0);
        }
    }
    void Update()
    {
        #region AnimationParameters
        if (isGrounded)
        {
            playerAnimator.SetBool("InAir", false);
        }
        if (!isGrounded)
        {
            playerAnimator.SetBool("InAir", true);
            playerAnimator.SetBool("IsIdle", false);
        }
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            playerAnimator.SetBool("IsRunning", true);
            playerAnimator.SetBool("IsIdle", false);
        }
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            playerAnimator.SetBool("IsRunning", false);
            playerAnimator.SetBool("IsIdle", true);
        }

        #endregion
        if (CurrentState != PlayerStates.Ragdoll)
        {
            HandleInput();
        }

        float angle = GetPlayerAngleAroundCylinder();

        // Check danger zone based on platform state
        if (IsInDangerZone(angle) && !isOnSafePlatform)
        {
            if (CurrentState != PlayerStates.Ragdoll)
            {
                ActivateRagdoll();
            }
        }

        // Ground check
        isGrounded = Physics.OverlapSphere(jumpChecker.position, jumpCheckerRadius, groundLayer).Length > 0;

        // State management
        switch (CurrentState)
        {
            case PlayerStates.Ragdoll:
                currentPlatform = null;
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
    float GetPlayerAngleAroundCylinder()
    {
        Vector3 relativePos = transform.position - cylinderCenter.position;
        Vector2 flatDirection = new Vector2(relativePos.x, relativePos.y); // X-Y plane
        float angle = Mathf.Atan2(flatDirection.y, flatDirection.x) * Mathf.Rad2Deg;
        return (angle < 0) ? angle + 360f : angle;
    }

    bool IsInDangerZone(float angle)
    {
        return angle < safeAreaBottomMin || angle > safeAreaBottomMax;
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            isOnSafePlatform = true;
            canBeRagdolledByBullets = false;
        }
        if (other.gameObject.CompareTag("Untagged"))
        {
            isOnSafePlatform = false;
            canBeRagdolledByBullets = true;
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

    public void ActivateRagdoll()
    {
        if (ragdollCooldownRoutine  != null)
        {
            StopCoroutine(ragdollCooldownRoutine);
        }
        ragdollCooldownRoutine = StartCoroutine(RagdollCooldownRoutine());
        CurrentState = PlayerStates.Ragdoll;
        OnRagdollActivate.Invoke();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Enable ragdoll physics
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            Collider collider = rb.GetComponent<Collider>();
            collider.enabled = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        playerAnimator.enabled = false;
    }
    private IEnumerator RagdollCooldownRoutine()
    {
        yield return new WaitForSeconds(ragdollRecoveryTime);
        rb.position = hipsBone.position;
        while (!isGrounded)
        {
            yield return null;
        }
        DeactivateRagdoll();
    }

    public void DeactivateRagdoll()
    {
        CurrentState = PlayerStates.FullControl;
        OnRagdollDeactivate.Invoke();
        playerMesh.SetActive(false);
        rb.position = hipsBone.position;
        // Disable player controller
        rb.isKinematic = false;

        // Enable ragdoll physics
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            Collider collider = rb.GetComponent<Collider>();
            collider.enabled = false;
            rb.isKinematic = true;
        }
        playerAnimator.enabled = true;
        playerMesh.SetActive(true);
    }

    public void AddExplosionForceToRagdoll(float explosionForce, Vector3 radiusOriginPoint, float explosionRadius)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.AddExplosionForce(explosionForce, radiusOriginPoint, explosionRadius);
        }
    }

    public void AddImpulseForceToRagdoll(Vector3 force, ForceMode forceMode)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.AddForce(force, forceMode);
        }
    }
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && canJump)
        {
            playerAnimator.SetTrigger("Jump");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpCooldownRoutine());
            _jumpVFXPrefab.gameObject.SetActive(true);
        }
    }

    private IEnumerator JumpCooldownRoutine()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
        _jumpVFXPrefab.gameObject.SetActive(false);
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

