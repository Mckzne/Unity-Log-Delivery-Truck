
using UnityEngine;

public class LoadAnchor : MonoBehaviour
{
    public LogIdentity detectedLog;

    public bool HasLog
    {
        get
        {
            detectedLog = null;

            LogIdentity[] logs =
                GetComponentsInChildren<LogIdentity>(true);

            foreach (LogIdentity log in logs)
            {
                if (log == null)
                    continue;

                if (log.transform != transform &&
                    log.transform.IsChildOf(transform) &&
                    log.SourcePrefab != null)
                {
                    detectedLog = log;
                    return true;
                }
            }

            return false;
        }
    }

    public void RefreshDetection()
    {
        LogIdentity[] logs =
            GetComponentsInChildren<LogIdentity>(true);

        Debug.Log(
            "LoadAnchor: " + name +
            " | LogIdentity components found: " + logs.Length +
            " | Loaded: " + HasLog,
            this
        );

        foreach (LogIdentity log in logs)
        {
            if (log != null)
            {
                Debug.Log(
                    "Found log: " + log.name +
                    " | SourcePrefab assigned: " +
                    (log.SourcePrefab != null) +
                    " | Parent: " + log.transform.parent,
                    log
                );
            }
        }
    }
}
