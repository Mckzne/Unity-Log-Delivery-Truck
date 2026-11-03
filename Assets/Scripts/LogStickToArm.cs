
using System.Collections;
using UnityEngine;

public class LogStickToArm : MonoBehaviour
{
    [Header("Settings")]
    public string armTag = "Arm";
    public float stickDuration = 3f;

    private bool isStuck;
    private Transform originalParent;
    private Rigidbody logRigidbody;

    private void Awake()
    {
        originalParent = transform.parent;
        logRigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryStick(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryStick(other);
    }

    private void TryStick(Collider other)
    {
        if (isStuck || !other.CompareTag(armTag))
            return;

        StartCoroutine(StickForSeconds(other.transform));
    }

    private IEnumerator StickForSeconds(Transform arm)
    {
        isStuck = true;

        if (logRigidbody != null)
        {
            logRigidbody.linearVelocity = Vector3.zero;
            logRigidbody.angularVelocity = Vector3.zero;
            logRigidbody.isKinematic = true;
        }

        transform.SetParent(arm, true);

        yield return new WaitForSeconds(stickDuration);

        transform.SetParent(originalParent, true);

        if (logRigidbody != null)
            logRigidbody.isKinematic = false;

        isStuck = false;
    }
}
