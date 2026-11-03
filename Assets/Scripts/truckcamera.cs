
using UnityEngine;

public class truckcamera : MonoBehaviour
{
    [Header("Target")]
    public Transform truck;

    [Header("Camera Position")]
    public float distance = 8f;
    public float height = 3.5f;
    public float targetHeight = 1.5f;

    [Header("Follow Smoothness")]
    public float positionSmoothness = 5f;
    public float rotationSmoothness = 5f;

    [Header("Mouse Look")]
    public int dragMouseButton = 1;
    public float mouseSensitivity = 3f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;
    public float returnSpeed = 2f;
    public float returnDelay = 1f;

    [Header("Camera Collision")]
    public float collisionRadius = 0.3f;
    public float collisionPadding = 0.2f;
    public LayerMask collisionLayers = ~0;

    private Vector3 currentVelocity;
    private float yawOffset;
    private float pitchOffset;
    private float releaseTimer;
    private bool wasDragging;

    void LateUpdate()
    {
        if (truck == null)
            return;

        bool dragging = Input.GetMouseButton(dragMouseButton);

        if (dragging)
        {
            yawOffset += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitchOffset -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            pitchOffset = Mathf.Clamp(
                pitchOffset,
                minVerticalAngle,
                maxVerticalAngle
            );

            releaseTimer = 0f;
            wasDragging = true;
        }
        else if (wasDragging)
        {
            releaseTimer += Time.deltaTime;

            if (releaseTimer >= returnDelay)
            {
                yawOffset = Mathf.Lerp(
                    yawOffset, 0f,
                    1f - Mathf.Exp(-returnSpeed * Time.deltaTime)
                );

                pitchOffset = Mathf.Lerp(
                    pitchOffset, 0f,
                    1f - Mathf.Exp(-returnSpeed * Time.deltaTime)
                );

                if (Mathf.Abs(yawOffset) < 0.1f &&
                    Mathf.Abs(pitchOffset) < 0.1f)
                {
                    yawOffset = 0f;
                    pitchOffset = 0f;
                    wasDragging = false;
                }
            }
        }

        Vector3 targetPosition =
            truck.position + Vector3.up * targetHeight;

        // Camera automatically follows the truck's direction.
        float yaw = truck.eulerAngles.y + yawOffset;
        float pitch = Mathf.Clamp(
            15f + pitchOffset,
            minVerticalAngle,
            maxVerticalAngle
        );

        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition =
            targetPosition
            + orbit * (Vector3.back * distance)
            + Vector3.up * (height - targetHeight);

        Vector3 offset = desiredPosition - targetPosition;
        float desiredDistance = offset.magnitude;

        if (Physics.SphereCast(
            targetPosition,
            collisionRadius,
            offset.normalized,
            out RaycastHit hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            desiredPosition = targetPosition
                + offset.normalized
                * Mathf.Max(0.5f, hit.distance - collisionPadding);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            1f / Mathf.Max(positionSmoothness, 0.01f)
        );

        Vector3 lookDirection = targetPosition - transform.position;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(lookDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                1f - Mathf.Exp(
                    -rotationSmoothness * Time.deltaTime
                )
            );
        }
    }
}
