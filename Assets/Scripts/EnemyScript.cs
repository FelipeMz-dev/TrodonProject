using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private int movementDirection = 1;
    private float stateTimer;
    private float shotTimer;
    private Transform currentWaypoint;
    private Quaternion targetFacingRotation;
    enum EnemyState
    {
        patrol,
        detection,
        attack,
    }

    private EnemyState state = EnemyState.patrol;
    private Animator animator;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioShot;
    [SerializeField] private AudioClip audioAlert;

    public Transform wayPoint1;
    public Transform wayPoint2;
    public Transform PlayerPosition;
    public float speed = 5f;
    public float rotationSpeed = 360f;
    public float health = 10f;
    public float distanceToDetect = 8f;
    public MeshRenderer meshAlertSymbol;
    public GameObject projectilePrefab;
    public float projectileSpeed = 30f;
    public float projectileOffset = 0.5f;
    public float projectileDamage = 10;
    public float timeIntervalShot = 1f;
    public float waypointWaitTime = 1f;
    public float detectionWaitTime = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentWaypoint = wayPoint2;
        movementDirection = GetDirectionTo(currentWaypoint);
        animator = GetComponentInChildren<Animator>();
        stateTimer = waypointWaitTime;
        UpdateFacingDirection();
        SetAlertSymbol(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (wayPoint1 == null || wayPoint2 == null || PlayerPosition == null) return;
        bool playerInRange = IsPlayerInDetectionArea();
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetFacingRotation,
            rotationSpeed * Time.deltaTime
        );
        if (animator == null) return;
        switch (state)
        {
            case EnemyState.patrol:
                animator.StopPlayback();
                if (playerInRange)
                {
                    ChangeState(EnemyState.detection);
                }
                else
                {
                    Patrol();
                }
                break;

            case EnemyState.detection:
                animator.StartPlayback();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    ChangeState(playerInRange ? EnemyState.attack : EnemyState.patrol);
                }
                break;

            case EnemyState.attack:
                //animator.SetBool("isWalking", false);
                if (!playerInRange)
                {
                    ChangeState(EnemyState.detection);
                }
                else
                {
                    shotTimer -= Time.deltaTime;
                    if (shotTimer <= 0f)
                    {
                        Shoot();
                        shotTimer = Mathf.Max(0.01f, timeIntervalShot);
                    }
                }
                break;
        }
    }

    private void Patrol()
    {
        Vector3 targetPosition = new Vector3(
            currentWaypoint.position.x,
            transform.position.y,
            transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - currentWaypoint.position.x) <= 0.01f)
        {
            transform.position = new Vector3(
                currentWaypoint.position.x,
                transform.position.y,
                transform.position.z);
            stateTimer -= Time.deltaTime;
            animator.SetBool("isWalking", false);
            if (stateTimer <= 0f)
            {
                currentWaypoint = currentWaypoint == wayPoint1 ? wayPoint2 : wayPoint1;
                movementDirection = GetDirectionTo(currentWaypoint);
                stateTimer = waypointWaitTime;
                UpdateFacingDirection();
                animator.SetBool("isWalking", true);
            }
        }
    }

    private bool IsPlayerInDetectionArea()
    {
        Vector3 playerPosition = PlayerPosition.position;
        float distanceToPlayer = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(playerPosition.x, playerPosition.y));

        if (distanceToPlayer > distanceToDetect ||
            Mathf.Sign(playerPosition.x - transform.position.x) != movementDirection)
        {
            return false;
        }

        Collider enemyCollider = GetComponent<Collider>();
        float enemyHeight = enemyCollider == null ? 1f : enemyCollider.bounds.extents.y;
        return Mathf.Abs(playerPosition.y - transform.position.y) <= enemyHeight;
    }

    private void ChangeState(EnemyState newState)
    {
        state = newState;

        if (newState == EnemyState.detection)
        {
            stateTimer = detectionWaitTime;
            SetAlertSymbol(true);
            if (audioSource != null && audioAlert != null)
            {
                audioSource.PlayOneShot(audioAlert);
            }
        }
        else if (newState == EnemyState.attack)
        {
            shotTimer = 0f;
        }
        else if (newState == EnemyState.patrol)
        {
            stateTimer = waypointWaitTime;
            SetAlertSymbol(false);
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector3 direction = Vector3.right * movementDirection;
        Vector3 spawnPosition = transform.position + direction * projectileOffset;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        ProjectileScript projectile = projectileObject.GetComponent<ProjectileScript>();

        if (projectile != null)
        {
            projectile.Launch(direction, projectileSpeed, gameObject);
            projectile.SetDamage(10f);
            if (audioSource != null && audioShot != null)
            {
                audioSource.PlayOneShot(audioShot);
            }
        }
    }

    private int GetDirectionTo(Transform waypoint)
    {
        float horizontalDifference = waypoint.position.x - transform.position.x;
        return horizontalDifference == 0f ? movementDirection : (horizontalDifference > 0f ? 1 : -1);
    }

    private void UpdateFacingDirection()
    {
        float targetY = movementDirection == 1 ? 0f : 180f;
        targetFacingRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            targetY,
            transform.eulerAngles.z);
    }

    private void SetAlertSymbol(bool isVisible)
    {
        if (meshAlertSymbol != null)
        {
            meshAlertSymbol.enabled = isVisible;
        }
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
