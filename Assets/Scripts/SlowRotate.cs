using UnityEngine;

public class SlowRotate : MonoBehaviour
{
    public float rotationSpeed = 25f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}