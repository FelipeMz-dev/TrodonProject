using UnityEngine;

public class RigidBody2dot5DScript : MonoBehaviour
{

    private float fixedPlayerZ;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fixedPlayerZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = transform.position;
        transform.position = new Vector3(currentPosition.x, currentPosition.y, fixedPlayerZ);

        Vector3 currentAngles = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0f, 0f, currentAngles.z);
    }
}
