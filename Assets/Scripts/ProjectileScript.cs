using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;
    private GameObject owner;
    private Rigidbody body;
    private float damage = 5;

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
        EnemyScript enemy = collision.gameObject.GetComponentInParent<EnemyScript>();
        PlayerStats targetStats = collision.gameObject.GetComponentInParent<PlayerStats>();
        PlayerStats shooterStats = owner == null ? null : owner.GetComponent<PlayerStats>();
        EnemyScript shooterEnemy = owner == null ? null : owner.GetComponent<EnemyScript>();

        if (enemy != null && shooterStats != null)
        {
            shooterStats.projectilHit();
            Destroy(gameObject);
            enemy.TakeDamage(damage);
        }
        else if (targetStats != null && shooterEnemy != null)
        {
            targetStats.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public void SetDamage(float value)
    {
        damage = value;
    }
}