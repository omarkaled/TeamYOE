using System.Collections;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float timeToDestroy = 2f;

    private void Start()
    {
        StartCoroutine(TimedDestroyRoutine());
    }

    private IEnumerator TimedDestroyRoutine()
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(this.gameObject);
    }
}
