using UnityEngine;

public class MouseUnlocker : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
