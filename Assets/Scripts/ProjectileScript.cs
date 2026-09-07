using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    private GameObject owner;

    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float speed, GameObject owner)
    {
        this.owner = owner;
        IgnoreOwnerCollisions(owner);

        if (body != null)
        {
            body.linearVelocity = direction.normalized * speed;
        }

        Destroy(gameObject, lifetime);
    }

    private void IgnoreOwnerCollisions(GameObject owner)
    {
        if (owner == null)
        {
            return;
        }

        Collider[] projectileColliders = GetComponentsInChildren<Collider>();
        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();

        foreach (Collider projectileCollider in projectileColliders)
        {
            foreach (Collider ownerCollider in ownerColliders)
            {
                Physics.IgnoreCollision(projectileCollider, ownerCollider);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerStats stats = owner.GetComponent<PlayerStats>();
        stats.projectilHit();
        Destroy(gameObject);
    }
}