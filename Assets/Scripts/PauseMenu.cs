using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panelToActivate;
    public void OnPause()
    {
        Cursor.lockState = CursorLockMode.None;
        panelToActivate.SetActive(true);
        Time.timeScale = 0f;
    }
    public void Unpause()
    {
        Cursor.lockState = CursorLockMode.Locked;
        panelToActivate.SetActive(false);
        Time.timeScale = 1;
    }
    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
