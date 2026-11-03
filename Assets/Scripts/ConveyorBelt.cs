
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Direction")]
    public Transform directionReference;

    [Header("Movement")]
    [Min(0f)]
    public float beltSpeed = 2f;

    [Min(0f)]
    public float acceleration = 8f;

    [Header("Options")]
    public bool affectOnlyLogs = true;
    public string logTag = "Log";
    public bool preventLogRotation = true;

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;

        if (rb == null || rb.isKinematic)
            return;

        if (affectOnlyLogs &&
            !collision.transform.root.CompareTag(logTag))
            return;

        // Stop the log from rotating.
        if (preventLogRotation)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotation;
        }

        // Get the forward direction of the conveyor.
        Vector3 direction = directionReference != null
            ? directionReference.forward
            : transform.forward;

        // Keep movement horizontal.
        direction = Vector3.ProjectOnPlane(
            direction, Vector3.up
        ).normalized;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // Accelerate the log toward the target belt speed.
        Vector3 velocity = rb.linearVelocity;

        float currentSpeed = Vector3.Dot(velocity, direction);
        float speedDifference = beltSpeed - currentSpeed;

        float accelerationStep = Mathf.Clamp(
            speedDifference,
            -acceleration * Time.fixedDeltaTime,
            acceleration * Time.fixedDeltaTime
        );

        rb.AddForce(
            direction * (accelerationStep / Time.fixedDeltaTime),
            ForceMode.Acceleration
        );
    }
}
