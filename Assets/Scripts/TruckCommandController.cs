
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;
using TMPro;

public class TruckCommandController : MonoBehaviour
{
    [Header("References")]
    public TruckController truckController;
    public LogDeliveryManager deliveryManager;
    public TMP_InputField commandInput;
    public TMP_Text messageText;
    private float nextTargetDebugTime = 0f;
    [Header("Load Detection")]
    public Transform loadAnchor;
    public LoadAnchor loadDetector;
    public float collectDistance = 8f;

    [Header("Driving")]
    public float driveThrottle = 1f;
    public float turnSteering = 1f;
    public float arrivalDistance = 3f;

    private Coroutine timedCommand;
    private bool drivingToDelivery;
    private bool wasWaitingForReturn;

    public bool IsTruckLoaded()
    {
        if (loadAnchor == null || loadDetector == null)
            return false;

        // Verify the actual hierarchy instead of trusting an old reference.
        return loadDetector.HasLog;
    }

    private void Start()
    {
        if (commandInput != null)
            commandInput.onSubmit.AddListener(ProcessCommand);

        if (loadAnchor == null)
            Debug.LogError("Assign Load Anchor in the Inspector.", this);

        if (loadDetector == null)
            Debug.LogError("Assign Load Detector in the Inspector.", this);

        if (truckController == null)
            Debug.LogError("Assign Truck Controller in the Inspector.", this);

        if (deliveryManager == null)
            Debug.LogError("Assign Delivery Manager in the Inspector.", this);

        ShowMessage("Ready! Type 'help' to see commands.");
    }

    private void OnDestroy()
    {
        if (commandInput != null)
            commandInput.onSubmit.RemoveListener(ProcessCommand);
    }

    private void Update()
    {
        if (deliveryManager == null)
            return;

        if (deliveryManager.IsWaitingForReturn && !wasWaitingForReturn)
        {
            CancelTimedCommand();
            drivingToDelivery = false;

            if (truckController != null)
                truckController.StopAutonomousMovement();

            ShowMessage("Delivery complete! Return to base.");
        }

        wasWaitingForReturn = deliveryManager.IsWaitingForReturn;
    }

    public void ProcessCommand(string rawCommand)
    {
        if (string.IsNullOrWhiteSpace(rawCommand))
            return;

        string command = rawCommand.Trim().ToLowerInvariant();

        if (commandInput != null)
            commandInput.text = "";

        if (deliveryManager != null && deliveryManager.IsGameComplete)
        {
            ShowMessage("All deliveries are complete!");
            return;
        }

        if (ContainsAny(command, "help", "commands"))
        {
            ShowHelp();
            return;
        }

        if (ContainsAny(command, "status", "truck status"))
        {
            ShowStatus();
            return;
        }

        if (ContainsAny(command, "stop", "halt", "freeze", "brake"))
        {
            StopTruck();
            ShowMessage("Truck stopped.");
            return;
        }

        if (truckController == null)
        {
            ShowMessage("ERROR: Assign Truck Controller.");
            return;
        }

        if (ContainsAny(command,
            "collect log", "pick up log", "pickup log",
            "get the log", "load log", "pick log"))
        {
            CollectLog();
            return;
        }

        if (ContainsAny(command,
            "deliver log", "deliver the log", "go to delivery",
            "deliver it"))
        {
            DeliverLog();
            return;
        }

        if (ContainsAny(command,
            "return to base", "go home", "return home", "back to base"))
        {
            ReturnToBase();
            return;
        }

        Match timedMatch = Regex.Match(
            command,
            @"^(forward|drive forward|reverse|backward|back up|turn left|turn right)\s+for\s+(\d+(?:\.\d+)?)\s*(?:seconds|second|secs|sec|s)$"
        );

        if (timedMatch.Success)
        {
            float seconds = float.Parse(
                timedMatch.Groups[2].Value,
                CultureInfo.InvariantCulture
            );

            seconds = Mathf.Clamp(seconds, 0.1f, 60f);

            string action = timedMatch.Groups[1].Value;
            float throttle = 0f;
            float steering = 0f;

            if (action == "forward" || action == "drive forward")
                throttle = driveThrottle;
            else if (action == "reverse" ||
                     action == "backward" ||
                     action == "back up")
                throttle = -driveThrottle;
            else if (action == "turn left")
            {
                throttle = driveThrottle;
                steering = -turnSteering;
            }
            else if (action == "turn right")
            {
                throttle = driveThrottle;
                steering = turnSteering;
            }

            StartTimedCommand(throttle, steering, seconds);
            ShowMessage("Executing: " + command);
            return;
        }

        if (ContainsAny(command, "turn left", "steer left", "go left"))
        {
            Drive(driveThrottle, -turnSteering);
            ShowMessage("Turning left.");
            return;
        }

        if (ContainsAny(command, "turn right", "steer right", "go right"))
        {
            Drive(driveThrottle, turnSteering);
            ShowMessage("Turning right.");
            return;
        }

        if (ContainsAny(command, "forward left", "drive left"))
        {
            Drive(driveThrottle, -turnSteering);
            ShowMessage("Driving forward and left.");
            return;
        }

        if (ContainsAny(command, "forward right", "drive right"))
        {
            Drive(driveThrottle, turnSteering);
            ShowMessage("Driving forward and right.");
            return;
        }

        if (ContainsAny(command, "straight", "steer straight"))
        {
            Drive(driveThrottle, 0f);
            ShowMessage("Driving straight.");
            return;
        }

        if (ContainsAny(command,
            "reverse", "backward", "back up", "drive backwards"))
        {
            Drive(-driveThrottle, 0f);
            ShowMessage("Reversing.");
            return;
        }

        if (ContainsAny(command,
            "forward", "drive", "move ahead", "go ahead", "start"))
        {
            Drive(driveThrottle, 0f);
            ShowMessage("Driving forward.");
            return;
        }

        ShowMessage("Unknown command. Type 'help' for commands.");
    }

    private void CollectLog()
    {
        if (deliveryManager == null ||
            loadAnchor == null ||
            loadDetector == null)
        {
            ShowMessage(
                "ERROR: Assign Delivery Manager, Load Anchor and Load Detector."
            );
            return;
        }

        if (IsTruckLoaded())
        {
            ShowMessage("Truck already has a log!");
            return;
        }

        if (deliveryManager.IsWaitingForReturn)
        {
            ShowMessage("Return to base before the next mission.");
            return;
        }

        GameObject requiredPrefab =
            deliveryManager.CurrentRequiredLogPrefab;

        if (requiredPrefab == null)
        {
            ShowMessage("No required log prefab is assigned.");
            return;
        }

        LogIdentity[] logs = FindObjectsOfType<LogIdentity>();

        LogIdentity nearestLog = null;
        float nearestDistance = float.MaxValue;

        foreach (LogIdentity identity in logs)
        {
            if (identity == null || identity.SourcePrefab == null)
                continue;

            if (identity.SourcePrefab != requiredPrefab)
                continue;

            // Ignore logs already on the loading anchor.
            if (identity.transform == loadAnchor ||
                identity.transform.IsChildOf(loadAnchor))
                continue;

            float distance = Vector3.Distance(
                transform.position,
                identity.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestLog = identity;
            }
        }

        if (nearestLog == null)
        {
            ShowMessage(
                "No matching log found. Check the spawned prefab and LogIdentity."
            );
            return;
        }

        if (nearestDistance > collectDistance)
        {
            ShowMessage(
                "Move closer to the log. Distance: " +
                nearestDistance.ToString("F1") + " m"
            );
            return;
        }

        GameObject log = nearestLog.gameObject;

        // Stop physics before attaching the log.
        Rigidbody logBody = log.GetComponent<Rigidbody>();

        if (logBody != null)
        {
            logBody.linearVelocity = Vector3.zero;
            logBody.angularVelocity = Vector3.zero;
            logBody.isKinematic = true;
        }

        // Attach the original log; do not destroy or replace it.
        log.transform.SetParent(loadAnchor, false);
        log.transform.localPosition = Vector3.zero;
        log.transform.localRotation = Quaternion.identity;

        Debug.Log(
            "Collected log: " + log.name +
            " | Parent: " + log.transform.parent.name +
            " | LogIdentity: " +
            (log.GetComponent<LogIdentity>() != null),
            log
        );

        // Refresh and verify the anchor's detection.
        loadDetector.RefreshDetection();

        if (IsTruckLoaded())
        {
            ShowMessage("LOG DETECTED! Truck loaded. Ready to deliver.");
        }
        else
        {
            ShowMessage(
                "Load detection failed. Check LoadAnchorDetector and LogIdentity."
            );
        }
    }



    private void DeliverLog()
    {
        if (deliveryManager == null)
        {
            ShowMessage("ERROR: Delivery Manager is missing.");
            return;
        }

        if (deliveryManager.IsGameComplete)
        {
            ShowMessage("All deliveries completed!");
            return;
        }

        if (deliveryManager.IsWaitingForReturn)
        {
            ShowMessage("Return to base first.");
            return;
        }

        DeliveryZone targetZone = deliveryManager.CurrentDeliveryZone;

        if (targetZone == null)
        {
            ShowMessage("No delivery zone assigned to the current mission!");
            return;
        }

        GameObject requiredLog =
            deliveryManager.CurrentRequiredLogPrefab;

        string logName = requiredLog != null
            ? requiredLog.name
            : "Unknown log";

        Debug.Log(
            "DELIVERY TARGET: " + targetZone.name +
            " | REQUIRED LOG: " + logName
        );

        CancelTimedCommand();
        drivingToDelivery = true;

        ShowMessage(
            "Deliver " + logName +
            " to " + targetZone.name
        );
    }



    private void ReturnToBase()
    {
        if (deliveryManager == null)
        {
            ShowMessage("Assign Delivery Manager.");
            return;
        }

        if (!deliveryManager.IsWaitingForReturn)
        {
            ShowMessage("Complete the delivery before returning to base.");
            return;
        }

        drivingToDelivery = false;

        if (truckController != null)
            truckController.StopAutonomousMovement();

        ShowMessage("Drive through the ReturnPoint trigger at base.");
    }




    private void FixedUpdate()


    {

        DeliveryZone zone = deliveryManager.CurrentDeliveryZone;

        if (zone != null)
        {
            if (Time.time >= nextTargetDebugTime)
            {
                Debug.Log(
                    "TRUCK TARGET: " + zone.name +
                    " | TARGET POSITION: " + zone.transform.position +
                    " | TRUCK POSITION: " + transform.position
                );

                nextTargetDebugTime = Time.time + 10f;
            }
        }
        else
        {
            Debug.LogError("NO DELIVERY ZONE ASSIGNED!");
        }

        if (truckController == null || deliveryManager == null)
            return;

        if (deliveryManager.IsWaitingForReturn ||
            deliveryManager.IsGameComplete)
        {
            if (drivingToDelivery)
            {
                drivingToDelivery = false;
                truckController.StopAutonomousMovement();
            }

            return;
        }

        if (!drivingToDelivery)
            return;



        if (zone == null)
        {
            drivingToDelivery = false;
            truckController.StopAutonomousMovement();
            ShowMessage("No active delivery zone!");
            return;
        }

        Vector3 target = zone.transform.position;
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance < arrivalDistance)
        {
            drivingToDelivery = false;
            truckController.StopAutonomousMovement();
            ShowMessage("Near delivery zone. Check that the log enters the sphere.");
            return;
        }

        float angle = Vector3.SignedAngle(
            transform.forward,
            direction.normalized,
            Vector3.up
        );

        // Steering decreases as the truck points toward the target.
        float steering = Mathf.Clamp(angle / 90f, -0.3f, 0.3f);

        // Do not drive full speed when facing away from the target.
        float throttle = Mathf.Abs(angle) > 90f ? 0.3f : driveThrottle;

        truckController.SetAutonomousInput(throttle, steering);
    }



    private void Drive(float throttle, float steering)
    {
        CancelTimedCommand();
        drivingToDelivery = false;

        truckController.SetAutonomousInput(throttle, steering);
    }

    private void StartTimedCommand(
        float throttle, float steering, float seconds)
    {
        CancelTimedCommand();
        drivingToDelivery = false;

        timedCommand = StartCoroutine(
            RunTimedCommand(throttle, steering, seconds)
        );
    }

    private IEnumerator RunTimedCommand(
        float throttle, float steering, float seconds)
    {
        truckController.SetAutonomousInput(throttle, steering);

        yield return new WaitForSeconds(seconds);

        truckController.StopAutonomousMovement();
        timedCommand = null;

        ShowMessage("Timed command finished. Truck stopped.");
    }

    private void CancelTimedCommand()
    {
        if (timedCommand != null)
        {
            StopCoroutine(timedCommand);
            timedCommand = null;
        }
    }

    private void StopTruck()
    {
        CancelTimedCommand();
        drivingToDelivery = false;

        if (truckController != null)
            truckController.StopAutonomousMovement();
    }

    private void ShowStatus()
    {
        string loadStatus = IsTruckLoaded() ? "LOADED" : "EMPTY";
        string missionStatus = "No delivery manager assigned.";

        if (deliveryManager != null)
        {
            if (deliveryManager.IsGameComplete)
                missionStatus = "All deliveries complete.";
            else if (deliveryManager.IsWaitingForReturn)
                missionStatus = "Delivery complete; return to base.";
            else
                missionStatus = "Delivery in progress.";
        }

        ShowMessage(
            "Truck: " + loadStatus +
            "\nMission: " + missionStatus
        );
    }

    private void ShowHelp()
    {
        ShowMessage(
            "COMMANDS\n" +
            "forward / drive\n" +
            "reverse / back up\n" +
            "turn left / turn right\n" +
            "forward left / forward right\n" +
            "straight\n" +
            "stop / halt / brake\n" +
            "collect log / pick up log\n" +
            "deliver log\n" +
            "return to base\n" +
            "status\n" +
            "help\n\n" +
            "TIMED COMMANDS\n" +
            "forward for 5 seconds\n" +
            "reverse for 3 seconds\n" +
            "turn left for 2 seconds"
        );
    }

    private bool ContainsAny(string input, params string[] phrases)
    {
        foreach (string phrase in phrases)
        {
            if (input == phrase)
                return true;
        }

        return false;
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);

        if (messageText != null)
            messageText.text = message;
    }
}
