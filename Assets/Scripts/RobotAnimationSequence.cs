
using System.Collections;
using UnityEngine;

public class RobotAnimationSequence : MonoBehaviour
{
    [Header("References")]
    public Animator robotAnimator;

    [Header("Exact State Names")]
    public string pickupStateName = "RobotRig|PickupLog";
    public string carryToTruckStateName = "RobotRig|CarryLogToTruck";
    public string releaseStateName = "RobotRig|ReleaseLog";
    public string returnHomeStateName = "RobotRig|ReturnHome";
    public string idleStateName = "IDLE";

    [Header("Timing")]
    public float holdAfterPickup = 1f;

    private bool busy;

    public void StartPickup()
    {
        if (busy) return;

        if (robotAnimator == null)
        {
            Debug.LogError("Assign the robot Animator in the Inspector!");
            return;
        }

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        busy = true;
        robotAnimator.enabled = true;
        robotAnimator.speed = 1f;
        yield return PlayAndWait(pickupStateName);

        // Play the carrying animation before releasing.
        yield return PlayAndWait(carryToTruckStateName);

        yield return PlayAndWait(releaseStateName);
        yield return PlayAndWait(returnHomeStateName);

        robotAnimator.Play(idleStateName, 0, 0f);
        busy = false;
    }

    private IEnumerator PlayAndWait(string stateName)
    {
        // Use the state name directly; don't check HasState first.
        robotAnimator.Play(stateName, 0, 0f);
        robotAnimator.Update(0f);

        yield return null;

        float timer = 0f;

        while (robotAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName)
               && robotAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            timer += Time.unscaledDeltaTime;

            if (timer > 30f)
            {
                Debug.LogError("Animation timed out: " + stateName);
                yield break;
            }

            yield return null;
        }

        // Give Unity a frame to process the state change.
        yield return null;
    }
}
