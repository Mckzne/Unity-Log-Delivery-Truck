
using UnityEngine;

public class LogSpawner : MonoBehaviour
{
    [System.Serializable]
    public class LogType
    {
        public string logName;
        public GameObject logPrefab;

        [Min(0f)]
        public float spawnWeight = 1f;
    }

    [Header("Log Types")]
    public LogType[] logTypes;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Spawn Settings")]
    [Min(1)]
    public int maxLogs = 10;

    private int nextLogIndex = 0;
    private int activeLogs = 0;
    private bool waitingForReturn = false;

    private void Start()
    {
        // Spawn the first log immediately.
        if (!SpawnLog())
        {
            Debug.LogWarning(
                "First log could not spawn. Check Spawn Point and Log Types.",
                this
            );
        }
    }

    public void NotifyDeliveryCompleted()
    {
        waitingForReturn = true;
        Debug.Log("Delivery complete! Waiting for truck to return.");
    }

    public void SpawnAfterReturn()
    {
        Debug.Log("SpawnAfterReturn called.");

        if (!waitingForReturn)
        {
            Debug.LogWarning(
                "Spawn skipped: no delivery is waiting for return.",
                this
            );
            return;
        }

        if (activeLogs >= maxLogs)
        {
            Debug.LogWarning(
                "Maximum active logs reached: " + activeLogs,
                this
            );
            return;
        }

        if (SpawnLog())
        {
            waitingForReturn = false;
            Debug.Log("SUCCESS: Next log spawned!");
        }
        else
        {
            Debug.LogError("Next log failed to spawn.", this);
        }
    }

    private bool SpawnLog()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("Assign a Spawn Point.", this);
            return false;
        }

        if (activeLogs >= maxLogs)
        {
            Debug.LogWarning("Maximum active logs reached.", this);
            return false;
        }

        GameObject prefab = ChooseLogPrefab();

        if (prefab == null)
        {
            Debug.LogWarning("No valid log prefab is assigned.", this);
            return false;
        }

        // Create the log.
        GameObject log = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Add identity so delivery can recognize the prefab type.
        LogIdentity identity = log.GetComponent<LogIdentity>();

        if (identity == null)
            identity = log.AddComponent<LogIdentity>();

        identity.Initialize(prefab);

        // Track the spawned log.
        activeLogs++;

        LogLifetime lifetime = log.AddComponent<LogLifetime>();
        lifetime.Initialize(this);

        Debug.Log(
            "Spawned: " + log.name +
            " | Position: " + log.transform.position +
            " | Active: " + log.activeInHierarchy,
            this
        );

        return true;
    }

    private GameObject ChooseLogPrefab()
    {
        if (logTypes == null || logTypes.Length == 0)
            return null;

        for (int i = 0; i < logTypes.Length; i++)
        {
            int index = (nextLogIndex + i) % logTypes.Length;

            if (logTypes[index] != null &&
                logTypes[index].logPrefab != null)
            {
                nextLogIndex = (index + 1) % logTypes.Length;
                return logTypes[index].logPrefab;
            }
        }

        return null;
    }

    public void LogRemoved()
    {
        activeLogs = Mathf.Max(0, activeLogs - 1);
    }
}

public class LogLifetime : MonoBehaviour
{
    private LogSpawner spawner;

    public void Initialize(LogSpawner owner)
    {
        spawner = owner;
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.LogRemoved();
    }
}
