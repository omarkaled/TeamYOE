using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpForce = 15f;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerRb = other.attachedRigidbody;
        PlayerController playerController = playerRb.GetComponent<PlayerController>();
        if (playerRb != null)
        {
            // Get the contact point and normal direction
            if (Physics.Raycast(other.transform.position, -transform.up, out RaycastHit hit, 2f))
            {
                Vector3 launchDirection = hit.normal.normalized;

                // Optional: Zero vertical velocity so we get a consistent launch
                playerRb.linearVelocity = Vector3.zero;

                // Apply force based on the normal
                playerRb.AddForce(launchDirection * jumpForce, ForceMode.VelocityChange);
                playerController.isOnSafePlatform = true;
            }
        }
    }
}
