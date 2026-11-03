
using UnityEngine;

public class RobotLogAnimationTrigger : MonoBehaviour
{
    [Header("References")]
    public BoxCollider touchPoint;
    public Animator robotAnimator;

    [Header("Animation")]
    public string animationStateName = "RobotPickup";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag("Log"))
            return;

        // Make sure the log overlaps the assigned TOUCHPOINT collider.
        if (!touchPoint.bounds.Intersects(other.bounds))
            return;

        hasTriggered = true;
        robotAnimator.Play(animationStateName, 0, 0f);
    }
}
