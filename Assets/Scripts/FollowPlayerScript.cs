using System;
using UnityEngine;

public class FollowPlayerScript : MonoBehaviour
{
    public Transform playerTransform;
    public float minDistance = 0.1f;
    public float minSpeed = 2f;
    public float maxSpeed = 15f;
    public float maxDistance = 10f;

    void Start()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= minDistance) return;

        float distanceFactor = Mathf.Clamp01(distance / maxDistance);
        float speed = Mathf.Lerp(minSpeed, maxSpeed, distanceFactor);

        float targetX = MathF.Max(0, playerTransform.position.x);
        float targetY = MathF.Max(0.6f, playerTransform.position.y);

        Vector3 target = new Vector3(targetX, targetY, playerTransform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
}
