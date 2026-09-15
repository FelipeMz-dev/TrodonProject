using System;
using UnityEngine;

public class SensorCollider : MonoBehaviour
{
    public event Action<Collider> OnSensorEnter;
    public event Action<Collider> OnSensorExit;

    private void OnTriggerEnter(Collider other)
    {
        OnSensorEnter?.Invoke(other);
        if (!other.CompareTag("Player"))
        {
            Debug.Log("collision");
        }
    }

    void OnTriggerExit(Collider other)
    {
        OnSensorExit?.Invoke(other);
    }
}
