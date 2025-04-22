using UnityEngine;
using UnityEngine.Events;

public class TriggerBox : MonoBehaviour
{
    [SerializeField] private string tagFilter;
    public UnityEvent OnEnterTrigger;
    public UnityEvent OnExitTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(tagFilter))
        {
            OnEnterTrigger.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(tagFilter))
        {
            OnExitTrigger.Invoke();
        }
    }
}
