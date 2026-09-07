using UnityEngine;

public class PlatformMovementScript : MonoBehaviour
{
    public float speed = 2f; // Speed of the platform movement
    public Vector3 initialPosition;
    public Vector3 targetPosition;
    private Vector3 currentPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPosition = initialPosition;
        transform.position = currentPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != currentPosition)
        {    
            transform.position = Vector3.MoveTowards(transform.position, currentPosition, speed * Time.deltaTime);
        }
    }

    public void moveToTarget()
    {
        currentPosition = targetPosition;
    }

    public void moveToInitial()
    {
        currentPosition = initialPosition;
    }
}
