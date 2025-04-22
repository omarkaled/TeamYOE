using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }

    private Vector3 lastPosition;

    [SerializeField] private PhysicsMaterial highFrictionMaterial;
    [SerializeField] private PhysicsMaterial lowFrictionMaterial;
    private PlayerController playerController;

    private void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        lastPosition = transform.position;
        FrictionFloorSwitch();
    }

    private void OnEnable()
    {
        playerController.OnRagdollActivate.AddListener(SlipperyFloorSwitch);
        playerController.OnRagdollDeactivate.AddListener(FrictionFloorSwitch);
    }

    private void OnDisable()
    {
        playerController.OnRagdollActivate.RemoveListener(SlipperyFloorSwitch);
        playerController.OnRagdollDeactivate.RemoveListener(FrictionFloorSwitch);
    }

    void Update()
    {
        Velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    private void FrictionFloorSwitch()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.material = highFrictionMaterial;
        }
    }

    private void SlipperyFloorSwitch()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.material = lowFrictionMaterial;
        }
    }
}
