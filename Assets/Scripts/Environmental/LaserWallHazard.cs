using UnityEngine;

public class LaserWallHazard : MonoBehaviour
{
    [SerializeField] private float explosionForce = 50f;
    [SerializeField] private float explosionRadius = 2f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null && player.CurrentState != PlayerController.PlayerStates.Ragdoll)
        {
            Vector3 closestPoint = GetComponent<Collider>().ClosestPoint(player.transform.position);
            player.ActivateRagdoll();
            player.AddExplosionForceToRagdoll(explosionForce, closestPoint, explosionRadius);
        }
    }
}
