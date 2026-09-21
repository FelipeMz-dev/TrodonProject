using UnityEngine;

public class PlatformActivatorScript : MonoBehaviour
{
    public PlatformMovementScript platform;
    
    private int colliding = 0;

    void OnTriggerEnter(Collider other)
    {
        if (canActivate(other))
        {
            platform.moveToTarget();
            colliding++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (canActivate(other))
        {
            colliding--;
            if (colliding == 0) platform.moveToInitial();
        }
    }

    bool canActivate(Collider other)
    {
        return other.CompareTag("Player") || other.CompareTag("Activator");
    }
}
