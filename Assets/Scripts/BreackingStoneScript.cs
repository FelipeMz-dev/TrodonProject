using UnityEngine;

public class BreackingStoneScript : MonoBehaviour
{
    private Vector3 initialPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile") || other.CompareTag("DeathZone"))
        {
            transform.position = initialPosition;

        }
    }
}
