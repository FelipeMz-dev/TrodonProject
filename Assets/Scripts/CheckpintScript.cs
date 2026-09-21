using UnityEngine;

public class CheckpintScript : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerScript playerStats = other.GetComponent<PlayerScript>();
            playerStats.AddLastCheckpoint(transform.position);
        }
    }
}
