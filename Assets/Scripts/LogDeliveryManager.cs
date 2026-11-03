
using UnityEngine;
using TMPro;

public class LogDeliveryManager : MonoBehaviour
{
    [System.Serializable]
    public class DeliveryMission
    {
        [Header("Delivery Setup")]
        public GameObject requiredLogPrefab;
        public DeliveryZone deliverySphere;
    }

    [Header("Truck")]
    public Transform truck;

    [Header("Log Spawner")]
    public LogSpawner logSpawner;

    [Header("Missions - Set in Order")]
    public DeliveryMission[] missions;

    [Header("UI")]
    public TMP_Text messageText;

    private int currentMission = 0;
    private bool waitingForReturn = false;
    public bool IsWaitingForReturn => waitingForReturn;
    public bool IsGameComplete => allComplete;
    private bool allComplete = false;

    private void Start()
    {
        if (truck == null ||
            missions == null ||
            missions.Length == 0)
        {
            Debug.LogError(
                "Assign the truck and at least one mission!",
                this
            );

            enabled = false;
            return;
        }

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        foreach (DeliveryMission mission in missions)
        {
            if (mission != null &&
                mission.deliverySphere != null)
            {
                mission.deliverySphere.SetManager(this);
            }
        }

        ActivateMission();
    }

    public bool IsTruck(Transform other)
    {
        return truck != null &&
               (other == truck ||
                other.IsChildOf(truck) ||
                other.root == truck.root);
    }

    public GameObject GetRequiredLogPrefabForZone(
        DeliveryZone zone)
    {
        if (allComplete ||
            waitingForReturn ||
            currentMission < 0 ||
            currentMission >= missions.Length)
        {
            return null;
        }

        DeliveryMission mission = missions[currentMission];

        if (mission == null ||
            mission.deliverySphere != zone)
        {
            return null;
        }

        return mission.requiredLogPrefab;
    }

    public void TryDeliver(
        DeliveryZone zone,
        GameObject deliveredLog)
    {
        if (waitingForReturn || allComplete)
            return;

        if (currentMission < 0 ||
            currentMission >= missions.Length)
        {
            return;
        }

        DeliveryMission mission = missions[currentMission];

        if (mission == null ||
            zone != mission.deliverySphere)
        {
            return;
        }

        if (deliveredLog == null ||
            mission.requiredLogPrefab == null)
        {
            ShowMessage(
                "Check the required log prefab in the Inspector!"
            );
            return;
        }

        LogIdentity identity =
            deliveredLog.GetComponent<LogIdentity>();

        if (identity == null ||
            identity.SourcePrefab != mission.requiredLogPrefab)
        {
            ShowMessage("Wrong log! Bring the required type.");
            return;
        }

        Debug.Log("Correct log delivered: " + deliveredLog.name);

        // Remove the delivered log.
        Destroy(deliveredLog);

        // Tell the spawner a delivery is complete.
        if (logSpawner != null)
            logSpawner.NotifyDeliveryCompleted();

        // Mark the delivery sphere as completed.
        zone.MarkDelivered();

        // Wait until a Tyre-tagged wheel enters the ReturnPoint.
        waitingForReturn = true;

        ShowMessage(
            "GREAT JOB!\n" +
            "Correct log delivered!\n" +
            "Return to base for the next mission."
        );
    }

    // Called by ReturnPoint when a Tyre-tagged wheel
    // belonging to this truck enters the return trigger.
    public void TruckReturned()
    {
        if (!waitingForReturn || allComplete)
            return;

        waitingForReturn = false;

        // All missions are finished after returning from
        // the final delivery.
        if (currentMission >= missions.Length - 1)
        {
            FinishGame();
            return;
        }

        // Move to the next mission.
        currentMission++;

        ActivateMission();

        // Spawn the next log after the return.
        if (logSpawner != null)
        {
            logSpawner.SpawnAfterReturn();
        }
        else
        {
            Debug.LogWarning("Assign LogSpawner in the Inspector!", this);
        }
    }

    private void ActivateMission()
    {
        if (currentMission < 0 ||
            currentMission >= missions.Length)
        {
            FinishGame();
            return;
        }

        DeliveryMission current = missions[currentMission];

        foreach (DeliveryMission mission in missions)
        {
            if (mission != null &&
                mission.deliverySphere != null)
            {
                mission.deliverySphere.SetActive(
                    mission == current
                );
            }
        }

        string logName =
            current != null && current.requiredLogPrefab != null
                ? current.requiredLogPrefab.name
                : "Unassigned log prefab";

        ShowMessage("NEW MISSION!\nDeliver: " + logName);
    }

    private void FinishGame()
    {
        allComplete = true;
        waitingForReturn = false;

        foreach (DeliveryMission mission in missions)
        {
            if (mission != null &&
                mission.deliverySphere != null)
            {
                mission.deliverySphere.SetActive(false);
            }
        }

        ShowMessage(
            "GAME OVER!\n" +
            "GREAT JOB!\n" +
            "All deliveries completed!"
        );

        Debug.Log("GAME OVER! All delivery missions completed!");
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.Log(message);
            return;
        }

        messageText.text = message;
        messageText.gameObject.SetActive(true);
    }

    public DeliveryZone CurrentDeliveryZone
    {
        get
        {
            if (missions == null ||
                currentMission < 0 ||
                currentMission >= missions.Length)
            {
                return null;
            }

            return missions[currentMission]?.deliverySphere;
        }
    }

    public GameObject CurrentRequiredLogPrefab
    {
        get
        {
            if (missions == null ||
                currentMission < 0 ||
                currentMission >= missions.Length)
            {
                return null;
            }

            return missions[currentMission]?.requiredLogPrefab;
        }
    }
}
