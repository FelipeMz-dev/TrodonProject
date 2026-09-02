using UnityEngine;

public class RigidBody2dot5DScript : MonoBehaviour
{
    private void Awake()
    {
        Rigidbody body = GetComponent<Rigidbody>();

        if (body == null)
        {
            return;
        }

        body.constraints |= RigidbodyConstraints.FreezePositionZ |
                            RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationY;
    }
}
