using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private Health trackedHealth; 
    [SerializeField] private Image healthFillImage;

    private void Awake()
    {
        trackedHealth = GameObject.Find("Player").GetComponent<Health>();
    }
    private void LateUpdate()
    {
        if (trackedHealth != null && healthFillImage != null)
        {
            healthFillImage.fillAmount = trackedHealth.ReadableHealth;
        }
    }
}
