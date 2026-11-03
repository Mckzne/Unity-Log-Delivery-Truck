
using UnityEngine;

public class Log : MonoBehaviour
{
    [Header("Log Settings")]
    [SerializeField] private float lifetime = 60f;

    private void Start()
    {
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }
}
