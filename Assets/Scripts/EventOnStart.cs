using UnityEngine;
using UnityEngine.Events;

public class EventOnStart : MonoBehaviour
{
    public UnityEvent OnObjectStart;

    private void Start()
    {
        OnObjectStart.Invoke();
    }
}
