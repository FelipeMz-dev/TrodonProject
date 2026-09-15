using UnityEngine;

public class PlatformActivatorScript : MonoBehaviour
{

    public float speed;
    public PlatformMovementScript platform;
    
    private int colliding = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
