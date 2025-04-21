using UnityEngine;

public class ObjectSpinner : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinSpeed = 90f; // Degrees per second
    public Vector3 spinAxis = Vector3.up; // Default: Y axis

    void Update()
    {
        // Rotate the object over time along the chosen axis
        transform.Rotate(spinAxis.normalized * spinSpeed * Time.deltaTime, Space.Self);
    }
}
