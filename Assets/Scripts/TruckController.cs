
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TruckController : MonoBehaviour
{
    [Header("Tyre Models")]
    public Transform frontLeftTyre;
    public Transform frontRightTyre;
    public Transform rearLeftTyre;
    public Transform rearRightTyre;

    [Header("Tyre Settings")]
    public float tyreRadius = 0.5f;
    public float steeringAngle = 30f;
    public float wheelRotationMultiplier = 1f;
    public Vector3 tyreSpinAxis = Vector3.right;
    public Vector3 tyreSteerAxis = Vector3.up;

    [Header("Driving")]
    public float acceleration = 6f;
    public float maxSpeed = 2f;
    public float steeringSpeed = 45f;
    public float brakeStrength = 5f;

    [Header("Terrain")]
    public float groundCheckDistance = 1.5f;
    public LayerMask groundLayers = ~0;
    public float downForce = 8f;
    public float groundAlignSpeed = 4f;

    [Header("Autonomous Control")]
    public bool autonomousMode = false;

    private Rigidbody rb;
    private float moveInput;
    private float steerInput;
    private float wheelSpin;
    private float currentSteering;
    private bool autonomousBrake;

    private Quaternion frontLeftStart;
    private Quaternion frontRightStart;
    private Quaternion rearLeftStart;
    private Quaternion rearRightStart;

    private RaycastHit groundHit;
    private bool grounded;

    public Rigidbody Body => rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.mass = 3000f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;
        rb.centerOfMass = new Vector3(0f, -1f, 0f);
        rb.linearDamping = 0.2f;
        rb.angularDamping = 3f;
        rb.maxAngularVelocity = 2f;
    }

    void Start()
    {
        if (frontLeftTyre != null)
            frontLeftStart = frontLeftTyre.localRotation;

        if (frontRightTyre != null)
            frontRightStart = frontRightTyre.localRotation;

        if (rearLeftTyre != null)
            rearLeftStart = rearLeftTyre.localRotation;

        if (rearRightTyre != null)
            rearRightStart = rearRightTyre.localRotation;
    }

    // Called by the text-command system.
    public void SetAutonomousInput(
        float throttle,
        float steering,
        bool brake = false)
    {
        autonomousMode = true;

        moveInput = Mathf.Clamp(throttle, -1f, 1f);
        steerInput = Mathf.Clamp(steering, -1f, 1f);
        autonomousBrake = brake;
    }

    // Return to keyboard control.
    public void EnablePlayerControl()
    {
        autonomousMode = false;
        autonomousBrake = false;
        moveInput = 0f;
        steerInput = 0f;
    }

    public void StopAutonomousMovement()
    {
        SetAutonomousInput(0f, 0f, true);
    }

    void Update()
    {
        if (!autonomousMode)
        {
            moveInput = Input.GetAxisRaw("Vertical");
            steerInput = Input.GetAxisRaw("Horizontal");
            autonomousBrake = false;
        }

        currentSteering = Mathf.Lerp(
            currentSteering,
            steerInput * steeringAngle,
            8f * Time.deltaTime
        );

        AnimateTyres();
    }

    void FixedUpdate()
    {
        grounded = Physics.Raycast(
            transform.position + Vector3.up * 0.2f,
            Vector3.down,
            out groundHit,
            groundCheckDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );

        if (!grounded)
        {
            if (autonomousMode)
            {
                moveInput = 0f;
                steerInput = 0f;
            }

            return;
        }

        Drive();

        bool shouldBrake = autonomousMode
            ? autonomousBrake
            : Input.GetKey(KeyCode.Space);

        if (shouldBrake)
        {
            Vector3 velocity = rb.linearVelocity;

            rb.linearVelocity = Vector3.Lerp(
                velocity,
                Vector3.zero,
                brakeStrength * Time.fixedDeltaTime
            );
        }
    }

    void Drive()
    {
        Vector3 forward = Vector3.ProjectOnPlane(
            transform.forward,
            groundHit.normal
        ).normalized;

        float speed = Vector3.Dot(rb.linearVelocity, forward);

        if (Mathf.Abs(speed) < maxSpeed ||
            (Mathf.Abs(moveInput) > 0.01f &&
             Mathf.Sign(moveInput) != Mathf.Sign(speed)))
        {
            rb.AddForce(
                forward * moveInput * acceleration,
                ForceMode.Acceleration
            );
        }

        Vector3 velocity = rb.linearVelocity;
        float forwardSpeed = Vector3.Dot(velocity, forward);

        if (Mathf.Abs(forwardSpeed) > maxSpeed)
        {
            float clampedSpeed = Mathf.Clamp(
                forwardSpeed,
                -maxSpeed,
                maxSpeed
            );

            rb.linearVelocity +=
                forward * (clampedSpeed - forwardSpeed);
        }

        if (Mathf.Abs(speed) > 0.5f)
        {
            float turn = steerInput * steeringSpeed *
                Mathf.Clamp01(Mathf.Abs(speed) / 2f);

            turn *= Mathf.Sign(speed);

            Quaternion rotation = Quaternion.AngleAxis(
                turn * Time.fixedDeltaTime,
                Vector3.up
            );

            rb.MoveRotation(rb.rotation * rotation);
        }
    }

    void AnimateTyres()
    {
        if (rb == null)
            return;

        float direction = Vector3.Dot(
            rb.linearVelocity,
            transform.forward
        );

        wheelSpin +=
            direction / Mathf.Max(tyreRadius, 0.01f)
            * Mathf.Rad2Deg
            * wheelRotationMultiplier
            * Time.deltaTime;

        UpdateTyre(frontLeftTyre, frontLeftStart,
            currentSteering, wheelSpin);

        UpdateTyre(frontRightTyre, frontRightStart,
            currentSteering, wheelSpin);

        UpdateTyre(rearLeftTyre, rearLeftStart, 0f, wheelSpin);
        UpdateTyre(rearRightTyre, rearRightStart, 0f, wheelSpin);
    }

    void UpdateTyre(
        Transform tyre,
        Quaternion startRotation,
        float steering,
        float spin)
    {
        if (tyre == null)
            return;

        Quaternion steerRotation = Quaternion.AngleAxis(
            steering,
            tyreSteerAxis.normalized
        );

        Quaternion spinRotation = Quaternion.AngleAxis(
            spin,
            tyreSpinAxis.normalized
        );

        tyre.localRotation =
            startRotation * steerRotation * spinRotation;
    }
}
