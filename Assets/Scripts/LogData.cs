
using UnityEngine;

public class LogData : MonoBehaviour
{
    [Header("Log Information")]
    public string logTypeName = "Standard Log";

    [Tooltip("Unique ID assigned when the sensor detects this log.")]
    public string inspectionID = "";

    public bool HasBeenInspected { get; private set; }

    public float LengthMeters
    {
        get
        {
            Collider col = GetComponentInChildren<Collider>();
            if (col == null) return 0f;

            // Assumes the log's length aligns with world Z.
            return col.bounds.size.z;
        }
    }

    public float DiameterMeters
    {
        get
        {
            Collider col = GetComponentInChildren<Collider>();
            if (col == null) return 0f;

            return Mathf.Min(
                col.bounds.size.x,
                col.bounds.size.y
            );
        }
    }

    public bool MarkInspected(string id)
    {
        if (HasBeenInspected)
            return false;

        HasBeenInspected = true;
        inspectionID = id;
        return true;
    }
}
