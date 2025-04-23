using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    public float ReadableHealth => currentHealth / maxHealth;

    public UnityEvent OnDeath;
    public UnityEvent OnDamage;
    public UnityEvent OnHeal;

    private CinemachineImpulseSource impulseSource;

    private void Start()
    {
        currentHealth = maxHealth;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        impulseSource.GenerateImpulse();
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnDamage.Invoke();
        if (currentHealth <= 0f)
        {
            Die();
            OnDeath.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHeal.Invoke();
    }

    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
