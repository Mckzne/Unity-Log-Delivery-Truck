
using UnityEngine;

public class LogIdentity : MonoBehaviour
{
    public GameObject SourcePrefab { get; private set; }

    public void Initialize(GameObject sourcePrefab)
    {
        SourcePrefab = sourcePrefab;
    }
}
