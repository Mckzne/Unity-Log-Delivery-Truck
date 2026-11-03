
using UnityEngine;
using TMPro;

public class InspectionDashboard : MonoBehaviour
{
    [Header("Dashboard Text References")]
    public TMP_Text logIDText;
    public TMP_Text logTypeText;
    public TMP_Text lengthText;
    public TMP_Text diameterText;
    public TMP_Text counterText;
    public TMP_Text statusText;

    public void UpdateInspection(
        string id,
        string type,
        float length,
        float diameter,
        int totalDetected)
    {
        if (logIDText != null)
            logIDText.text = "Last Log: " + id;

        if (logTypeText != null)
            logTypeText.text = "Type: " + type;

        if (lengthText != null)
            lengthText.text = "Length: " +
                length.ToString("F2") + " m";

        if (diameterText != null)
            diameterText.text = "Diameter: " +
                diameter.ToString("F2") + " m";

        if (counterText != null)
            counterText.text = "Logs Detected: " +
                totalDetected;

        if (statusText != null)
            statusText.text = "INSPECTION COMPLETE";
    }
}
