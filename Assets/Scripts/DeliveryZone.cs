
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DeliveryZone : MonoBehaviour
{
    [Header("Delivery Label")]
    public TMP_Text floatingText;

    private LogDeliveryManager manager;
    private bool delivered;

    private readonly HashSet<GameObject> checkedLogs =
        new HashSet<GameObject>();

    public void SetManager(LogDeliveryManager deliveryManager)
    {
        manager = deliveryManager;
        delivered = false;
        checkedLogs.Clear();

        if (floatingText != null)
            floatingText.text = "LOG DELIVERY";
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void MarkDelivered()
    {
        delivered = true;

        if (floatingText != null)
            floatingText.text = "DELIVERED!";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (delivered || manager == null)
            return;

        // Find the identity on the log or one of its parents.
        LogIdentity identity =
            other.GetComponentInParent<LogIdentity>();

        if (identity == null || identity.SourcePrefab == null)
            return;

        // Get the prefab type required by this mission.
        GameObject requiredPrefab =
            manager.GetRequiredLogPrefabForZone(this);

        if (requiredPrefab == null)
            return;

        // Reject the wrong log type.
        if (identity.SourcePrefab != requiredPrefab)
        {
            Debug.Log(
                "Wrong log type: " + identity.SourcePrefab.name +
                ". Required: " + requiredPrefab.name,
                this
            );
            return;
        }

        GameObject actualLog = identity.gameObject;

        // Prevent repeated attempts for the same spawned log.
        if (!checkedLogs.Add(actualLog))
            return;

        // Manager verifies that the correct log is loaded on the truck.
        manager.TryDeliver(this, actualLog);
    }

    private void OnTriggerExit(Collider other)
    {
        LogIdentity identity =
            other.GetComponentInParent<LogIdentity>();

        if (identity != null)
            checkedLogs.Remove(identity.gameObject);
    }
}

