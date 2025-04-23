using System.Collections;
using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    private PlayerController _target;
    [Header("HomingSettings")]
    [SerializeField] private bool _isHoming = true;
    [SerializeField] private float _rotationSpeed = 5f;
    [Tooltip("If it's a homing bullet then this will determine how long before it breaks the tracking to the player")]
    [SerializeField] private float _timeBeforeBreakingTracking = 5f;
    [SerializeField] private float _distanceToPlayerBeforeUnlocking = 3f;
    [SerializeField] private float _initialVelocity = 6f;
    [SerializeField] private Rigidbody _bulletRB;

    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionForce = 50f;
    [SerializeField] private float impulseForce = 20f;
    [SerializeField] private float explosionDamage = 10f;
    [SerializeField] private LayerMask playerLayer;
    private void Start()
    {
        _target = FindFirstObjectByType<PlayerController>();

        _bulletRB.linearVelocity = transform.forward * _initialVelocity;

        if (_isHoming)
        {
            StartCoroutine(TrackPlayerRoutine());
        }
    }
    private IEnumerator TrackPlayerRoutine()
    {
        float time = 0;

        while (_isHoming && _target != null)
        {
            time += Time.deltaTime;
            if (time >= _timeBeforeBreakingTracking)
            {
                _isHoming = false;
                break;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _target.transform.position);
            if (distanceToPlayer < _distanceToPlayerBeforeUnlocking)
            {
                _isHoming = false;
                break;
            }

            // Recalculate direction toward the target
            Vector3 directionToTarget = (_target.transform.position + new Vector3(0, 0.8f, 0) - transform.position).normalized;

            // Optionally rotate to face the direction (for visuals)
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            // Apply velocity directly toward the target
            float speed = _bulletRB.linearVelocity.magnitude; // Maintain current speed
            if (speed == 0f) speed = 20f; // fallback speed if never set
            _bulletRB.linearVelocity = directionToTarget * speed;

            yield return null;
        }
    }
    private IEnumerator DelayedImpulse()
    {
        Vector3 explosionCenter = transform.position;

        // Detect all colliders in the explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, explosionRadius, playerLayer);
        foreach (Collider hit in hitColliders)
        {
            PlayerController player = hit.GetComponentInParent<PlayerController>();
            if (player != null && player.CurrentState != PlayerController.PlayerStates.Ragdoll)
            {
                // Apply damage if Health script is present
                Health health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(explosionDamage);
                }

                // Trigger ragdoll
                player.ActivateRagdoll();
                Debug.Log("BulletActivatingRagdoll");
                yield return new WaitForFixedUpdate();

                // Add explosion force to ragdoll
                player.AddExplosionForceToRagdoll(explosionForce, explosionCenter, explosionRadius);
                Vector3 direction = (player.transform.position - explosionCenter).normalized;
                direction.y = Mathf.Abs(direction.y) + 0.5f; // force upward if needed
                player.AddImpulseForceToRagdoll(direction.normalized * impulseForce, ForceMode.Impulse);
            }

            // Destroy the bullet after impact
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(DelayedImpulse());
    }
}
