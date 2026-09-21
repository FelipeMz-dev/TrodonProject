using UnityEngine;

public class PlatformMovementScript : MonoBehaviour
{
    public float speed = 2f; // Speed of the platform movement
    public Transform initialPosition;
    public Transform targetPosition;
    private Vector3 currentPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPosition = initialPosition.position;
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
        currentPosition = targetPosition.position;
    }

    public void moveToInitial()
    {
        currentPosition = initialPosition.position;
    }
}
