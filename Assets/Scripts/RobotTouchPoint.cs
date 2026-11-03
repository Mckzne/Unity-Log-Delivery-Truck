
using UnityEngine;

public class RobotTouchPoint : MonoBehaviour
{
    public RobotAnimationSequence robotSequence;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Log"))
            return;

        Debug.Log("Log touched TOUCHPOINT!");

        if (robotSequence != null)
        {
            robotSequence.StartPickup();
        }
        else
        {
            Debug.LogError("Robot Sequence is not assigned!");
        }
    }
}
