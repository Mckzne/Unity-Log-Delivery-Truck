
using UnityEngine;

public class LogPickupSensor : MonoBehaviour
{
    public AutomatedLogRobot robot;

    private void OnTriggerEnter(Collider other)
    {
        if (robot == null)
            return;

        // Support logs whose collider is on a child object.
        Transform log = other.transform;

        while (log != null && !log.CompareTag("Log"))
        {
            log = log.parent;
        }

        if (log != null)
        {
            robot.TryLoadLog(log);
        }
    }
}
