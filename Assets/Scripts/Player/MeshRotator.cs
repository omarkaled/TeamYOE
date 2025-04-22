using UnityEngine;

public class MeshRotator : MonoBehaviour
{
    [Header("References")]
    public Transform playerMesh;         // Assign the mesh (child of the player)
    public Transform cameraTransform;    // Assign the main camera

    [Header("Settings")]
    public float rotationSpeed = 10f;

    private Vector3 moveInput;

    [SerializeField] PlayerController playerController;

    void Update()
    {
        if (playerController.CurrentState == PlayerController.PlayerStates.Ragdoll)
        {
            return;
        }
        // Get input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Camera-relative input
        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;

        moveInput = (camForward * v + camRight * h).normalized;

        // Rotate the mesh toward movement direction
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
