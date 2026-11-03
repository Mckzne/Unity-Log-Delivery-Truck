
using System.Collections.Generic;
using UnityEngine;

public class LogSensor : MonoBehaviour
{
    [Header("Dashboard")]
    public InspectionDashboard dashboard;

    private int logsDetected = 0;
    private int nextInspectionID = 1;

    private readonly HashSet<GameObject> detectedObjects =
      new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        GameObject logObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        // Support a Log tag on either the root or collider.
        if (!logObject.CompareTag("Log") &&
            !other.CompareTag("Log"))
        {
            return;
        }

        // Avoid counting multiple colliders on the same log.
        if (!detectedObjects.Add(logObject))
            return;

        LogData data = logObject.GetComponent<LogData>();

        if (data == null)
            data = logObject.GetComponentInParent<LogData>();

        logsDetected++;

        string id = "LOG-" +
            nextInspectionID.ToString("D3");

        nextInspectionID++;

        string type = data != null
            ? data.logTypeName
            : "Unknown";

        float length = data != null
            ? data.LengthMeters
            : 0f;

        float diameter = data != null
            ? data.DiameterMeters
            : 0f;

        if (data != null)
            data.MarkInspected(id);

        Debug.Log(
            "SENSOR: " + id +
            " | Type: " + type +
            " | Length: " + length.ToString("F2") +
            " m | Diameter: " + diameter.ToString("F2") +
            " m | Total: " + logsDetected
        );

        if (dashboard != null)
        {
            dashboard.UpdateInspection(
                id,
                type,
                length,
                diameter,
                logsDetected
            );
        }
    }
}
