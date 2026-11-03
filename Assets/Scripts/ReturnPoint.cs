
using UnityEngine;

public class ReturnPoint : MonoBehaviour
{
    [Header("Truck")]
    public Transform truck;

    [Header("Delivery Manager")]
    public LogDeliveryManager deliveryManager;

    private bool truckInside;

    private void OnTriggerEnter(Collider other)
    {
        // Only Tyre-tagged colliders can trigger the return.
        if (!other.CompareTag("Tyre"))
            return;

        if (truck == null || deliveryManager == null)
            return;

        // Check that this tyre belongs to the assigned truck.
        if (other.transform != truck &&
            !other.transform.IsChildOf(truck))
        {
            return;
        }

        if (truckInside)
            return;

        truckInside = true;

        // Let the manager decide whether a mission is waiting.
        deliveryManager.TruckReturned();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Tyre"))
            return;

        if (truck == null)
            return;

        if (other.transform == truck ||
            other.transform.IsChildOf(truck))
        {
            truckInside = false;
        }
    }
}
