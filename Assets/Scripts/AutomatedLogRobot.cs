
using System.Collections;
using UnityEngine;

public class AutomatedLogRobot : MonoBehaviour
{
    [Header("Moving Robot Joints - Assign Your Existing Meshes")]
    public Transform basePivot;       // TurretMesh
    public Transform shoulderPivot;  // UpperArmMesh
    public Transform elbowPivot;     // ForearmMesh
    public Transform wristPivot;     // WristMesh

    [Header("Local Rotation Axes")]
    public Vector3 baseAxis = Vector3.up;
    public Vector3 shoulderAxis = Vector3.right;
    public Vector3 elbowAxis = Vector3.right;
    public Vector3 wristAxis = Vector3.right;

    [Header("Gripper")]
    public Transform gripperAttachPoint; // TOUCHPOINT
    public Transform leftFingerPivot;
    public Transform rightFingerPivot;

    [Header("Targets")]
    public Transform truckDropPoint;

    [Header("Movement")]
    [Min(1f)] public float jointMoveSpeed = 90f;
    [Min(0.01f)] public float gripperMoveTime = 0.5f;
    [Min(0.01f)] public float reachTolerance = 0.08f;
    [Min(1)] public int iterationsPerFrame = 8;
    [Min(0.1f)] public float reachTimeout = 5f;
    [Min(0f)] public float releaseDelay = 0.25f;

    [Header("Gripper Local Euler Angles")]
    public Vector3 leftOpenAngles;
    public Vector3 leftClosedAngles;
    public Vector3 rightOpenAngles;
    public Vector3 rightClosedAngles;

    [Header("Multiple Log Placement")]
    public Vector3 truckLocalSpacing = new Vector3(0f, 0f, 1.1f);

    private Quaternion homeBase;
    private Quaternion homeShoulder;
    private Quaternion homeElbow;
    private Quaternion homeWrist;

    private Quaternion leftOpen;
    private Quaternion leftClosed;
    private Quaternion rightOpen;
    private Quaternion rightClosed;

    private bool isBusy;
    private int logsLoaded;

    public bool IsBusy => isBusy;

    private void Awake()
    {
        if (!ReferencesValid())
        {
            enabled = false;
            return;
        }

        homeBase = basePivot.localRotation;
        homeShoulder = shoulderPivot.localRotation;
        homeElbow = elbowPivot.localRotation;
        homeWrist = wristPivot.localRotation;

        leftOpen = Quaternion.Euler(leftOpenAngles);
        leftClosed = Quaternion.Euler(leftClosedAngles);
        rightOpen = Quaternion.Euler(rightOpenAngles);
        rightClosed = Quaternion.Euler(rightClosedAngles);

        SetGripper(leftOpen, rightOpen);
    }

    private bool ReferencesValid()
    {
        bool valid =
            basePivot != null &&
            shoulderPivot != null &&
            elbowPivot != null &&
            wristPivot != null &&
            gripperAttachPoint != null &&
            leftFingerPivot != null &&
            rightFingerPivot != null &&
            truckDropPoint != null;

        if (!valid)
            Debug.LogError(
                "AutomatedLogRobot: Assign all joint, gripper and truck references.",
                this
            );

        return valid;
    }

    // Call this from your existing log pickup sensor.
    public void TryLoadLog(Transform log)
    {
        if (!enabled || isBusy || log == null)
            return;

        if (!log.CompareTag("Log") && !log.root.CompareTag("Log"))
        {
            Debug.LogWarning("The detected object is not tagged Log.", log);
            return;
        }

        StartCoroutine(LoadingCycle(log));
    }

    private IEnumerator LoadingCycle(Transform log)
    {
        isBusy = true;

        yield return MoveGripper(leftOpen, rightOpen);

        // Reach the detected log's current position.
        yield return MoveToTarget(log.position);

        if (log == null)
        {
            yield return ReturnHome();
            isBusy = false;
            yield break;
        }

        // Grip the log.
        yield return MoveGripper(leftClosed, rightClosed);

        Rigidbody logBody = log.GetComponent<Rigidbody>();
        if (logBody == null)
            logBody = log.GetComponentInParent<Rigidbody>();

        bool previousKinematic = false;

        if (logBody != null)
        {
            previousKinematic = logBody.isKinematic;
            logBody.isKinematic = true;
        }

        log.SetParent(gripperAttachPoint, true);

        // Move the gripper toward the truck.
        yield return MoveToTarget(truckDropPoint.position);

        if (log != null)
        {
            Vector3 offset = truckDropPoint.TransformVector(
                Vector3.Scale(
                    truckLocalSpacing,
                    new Vector3(logsLoaded, logsLoaded, logsLoaded)
                )
            );

            Vector3 dropPosition = truckDropPoint.position + offset;
            Quaternion dropRotation = truckDropPoint.rotation;

            // Keep the released log with the truck.
            Transform truckRoot = truckDropPoint.parent;

            log.SetParent(truckRoot, true);
            log.SetPositionAndRotation(dropPosition, dropRotation);

            if (logBody != null)
                logBody.isKinematic = true;

            logsLoaded++;
        }

        yield return MoveGripper(leftOpen, rightOpen);

        if (releaseDelay > 0f)
            yield return new WaitForSeconds(releaseDelay);

        yield return ReturnHome();

        isBusy = false;

        Debug.Log("Robot loading cycle complete. Total logs: " + logsLoaded);
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        float elapsed = 0f;

        while (elapsed < reachTimeout)
        {
            if (gripperAttachPoint == null)
                yield break;

            float distance = Vector3.Distance(
                gripperAttachPoint.position,
                targetPosition
            );

            if (distance <= reachTolerance)
                break;

            for (int i = 0; i < iterationsPerFrame; i++)
                SolveCCD(targetPosition);

            elapsed += Time.deltaTime;
            yield return null;
        }

        float finalDistance = Vector3.Distance(
            gripperAttachPoint.position,
            targetPosition
        );

        if (finalDistance > reachTolerance)
        {
            Debug.LogWarning(
                "IK could not reach the target completely. Remaining distance: "
                + finalDistance,
                this
            );
        }
    }

    private void SolveCCD(Vector3 targetPosition)
    {
        // Adjust distal joints first, then move back toward the base.
        SolveJoint(wristPivot, wristAxis, targetPosition);
        SolveJoint(elbowPivot, elbowAxis, targetPosition);
        SolveJoint(shoulderPivot, shoulderAxis, targetPosition);
       
    }

    private void SolveJoint(
        Transform joint,
        Vector3 localAxis,
        Vector3 targetPosition)
    {
        if (joint == null || localAxis.sqrMagnitude < 0.0001f)
            return;

        Vector3 axisWorld = joint.TransformDirection(localAxis.normalized);

        Vector3 toEnd = gripperAttachPoint.position - joint.position;
        Vector3 toTarget = targetPosition - joint.position;

        // Only rotate around the joint's permitted axis.
        Vector3 projectedEnd = Vector3.ProjectOnPlane(toEnd, axisWorld);
        Vector3 projectedTarget = Vector3.ProjectOnPlane(toTarget, axisWorld);

        if (projectedEnd.sqrMagnitude < 0.000001f ||
            projectedTarget.sqrMagnitude < 0.000001f)
            return;

        float angle = Vector3.SignedAngle(
            projectedEnd,
            projectedTarget,
            axisWorld
        );

        // Limit each correction to keep movement gradual.
        angle = Mathf.Clamp(
            angle,
            -jointMoveSpeed * Time.deltaTime,
            jointMoveSpeed * Time.deltaTime
        );

        joint.rotation =
            Quaternion.AngleAxis(angle, axisWorld) * joint.rotation;
    }

    private IEnumerator MoveGripper(
        Quaternion leftTarget,
        Quaternion rightTarget)
    {
        Quaternion leftStart = leftFingerPivot.localRotation;
        Quaternion rightStart = rightFingerPivot.localRotation;

        float duration = Mathf.Max(0.01f, gripperMoveTime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            SetGripper(
                Quaternion.Slerp(leftStart, leftTarget, t),
                Quaternion.Slerp(rightStart, rightTarget, t)
            );

            yield return null;
        }

        SetGripper(leftTarget, rightTarget);
    }

    private void SetGripper(Quaternion left, Quaternion right)
    {
        leftFingerPivot.localRotation = left;
        rightFingerPivot.localRotation = right;
    }

    private IEnumerator ReturnHome()
    {
        float elapsed = 0f;
        float duration = 1.5f;

        Quaternion startBase = basePivot.localRotation;
        Quaternion startShoulder = shoulderPivot.localRotation;
        Quaternion startElbow = elbowPivot.localRotation;
        Quaternion startWrist = wristPivot.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            basePivot.localRotation =
                Quaternion.Slerp(startBase, homeBase, t);

            shoulderPivot.localRotation =
                Quaternion.Slerp(startShoulder, homeShoulder, t);

            elbowPivot.localRotation =
                Quaternion.Slerp(startElbow, homeElbow, t);

            wristPivot.localRotation =
                Quaternion.Slerp(startWrist, homeWrist, t);

            yield return null;
        }

        basePivot.localRotation = homeBase;
        shoulderPivot.localRotation = homeShoulder;
        elbowPivot.localRotation = homeElbow;
        wristPivot.localRotation = homeWrist;
    }
}
