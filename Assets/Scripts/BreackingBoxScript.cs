using UnityEngine;

public class BreackingBoxScript : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Projectile") || other.CompareTag("DeathZone"))
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player == null) Destroy(gameObject);
            else if (player.IsDashState()) Destroy(gameObject);
        }
    }
}
