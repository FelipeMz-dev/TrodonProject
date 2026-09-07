using System;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private ActionsController actionsController;
    private CharacterController characterController;
    private float verticalVelocity;
    private float fixedPlayerZ;
    private int lastDirection = 1;
    private float dashState = 0;
    private float _pushPower;
    private float originalColliderHeight;
    private bool isCrouching;
    private bool IsPlayState = false;
    PlayerStats stats;

    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float speed = 12f;
    public float pushPower = 1;
    public float marginHorizontal = 6f;
    public float cameraOffsetY = 4f;
    public float dashSpeed = 30;
    public float dashDuration = 0.5f;
    public float crouchScale = 0.5f;
    public float crouchSpeedMultiplier = 0.5f;
    public Transform cameraTransform;
    public Transform gun;
    public GameObject projectilePrefab;
    public float projectileSpeed = 30f;
    public float projectileOffset = 0.5f;
    public Material materialGreen;

    void Awake()
    {
        actionsController = new ActionsController();
        characterController = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();
    }

    void OnEnable()
    {
        actionsController.Player.Enable();
        actionsController.Gun.Enable();
    }

    void OnDisable()
    {
        actionsController.Player.Disable();
        actionsController.Gun.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fixedPlayerZ = transform.position.z;
        _pushPower = pushPower;
        originalColliderHeight = characterController.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsPlayState) return;

        HandleJumping();
        HandleDash();
        HandleCrouch();
        MoveGunAngle();
        ShootGunAngle();

        if (dashState == 0)
        {
            float currentSpeed = isCrouching? speed * crouchSpeedMultiplier : speed;
            Vector3 movement = transform.right * GetMovementInput().x * currentSpeed + transform.up * verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }

        Vector3 currentPosition = characterController.transform.position;
        characterController.transform.position = new Vector3(currentPosition.x, currentPosition.y, fixedPlayerZ);

        FixCameraPosition();
    }

    private void MoveGunAngle()
    {
        if (gun == null || Camera.main == null)
        {
            return;
        }

        Vector2 cursorPosition = actionsController.Gun.direction.ReadValue<Vector2>();
        float distanceToGun = Mathf.Abs(Camera.main.transform.position.z - gun.position.z);
        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(cursorPosition.x, cursorPosition.y, distanceToGun)
        );
        Vector2 direction = (Vector2)cursorWorldPosition - (Vector2)gun.position;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        gun.rotation = Quaternion.Euler(0f, 0f, directionAngle);
    }

    private void ShootGunAngle()
    {
        if (!actionsController.Gun.shoot.WasPressedThisFrame() || gun == null || projectilePrefab == null)
        {
            return;
        }

        Vector3 direction = gun.right;
        Vector3 spawnPosition = gun.position + direction * projectileOffset;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, gun.rotation);
        ProjectileScript projectile = projectileObject.GetComponent<ProjectileScript>();

        if (projectile != null)
        {
            stats.shot();
            projectile.Launch(direction, projectileSpeed, gameObject);
        }
    }

    private void FixCameraPosition()
    {
        if (cameraTransform != null)
        {
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 playerPosition = characterController.transform.position;
            float distanceHorizontal = Mathf.Abs(playerPosition.x - cameraPosition.x) - marginHorizontal;
            float newX = playerPosition.x < cameraPosition.x - marginHorizontal ? cameraPosition.x - distanceHorizontal : playerPosition.x > cameraPosition.x + marginHorizontal ? cameraPosition.x + distanceHorizontal : cameraPosition.x;
            newX = Mathf.Clamp(newX, 0, cameraPosition.x + marginHorizontal);
            Vector3 newPosition = new Vector3(newX, playerPosition.y + cameraOffsetY, cameraTransform.transform.position.z);
            cameraTransform.position = newPosition;
        }
    }

    private Vector3 GetMovementInput()
    {
        bool moveLeft = actionsController.Player.moveLeft.IsPressed();
        bool moveRight = actionsController.Player.moveRight.IsPressed();
        int direction = (moveLeft ? -1 : 0) + (moveRight ? 1 : 0);
        lastDirection = moveLeft ? -1 : moveRight ? 1 : lastDirection;
        return new Vector3(direction, 0f, 0f);
    }

    private void HandleJumping()
    {
        bool isGrounded = characterController.isGrounded;
        bool jump = actionsController.Player.jump.WasPressedThisFrame();

        if (isGrounded && jump)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        } else
        {
            verticalVelocity = verticalVelocity > 0 && isCrouching ? 0 : verticalVelocity;
        }

        if (!isGrounded || verticalVelocity > 0f)
        {
            verticalVelocity += gravity * Time.deltaTime * 3f;
        }
    }

    private void HandleDash()
    {
        bool dash = actionsController.Player.dash.WasPressedThisFrame();

        if (dash && dashState == 0)
        {
            dashState = dashDuration;
            stats.dash();
        } 
        
        if (IsDashState())
        {
            Vector3 movement = transform.right * lastDirection * dashSpeed;
            characterController.Move(movement * Time.deltaTime);
            pushPower = _pushPower * 10;
            dashState -= Time.deltaTime;
        } else if (dashState < 0)
        {
            dashState = 0;
            pushPower = _pushPower;
        }
    }

    private void HandleCrouch()
    {
        bool crouch = actionsController.Player.crouch.IsPressed();

        // No permitir levantarse mientras haya un objeto bloqueando el espacio superior.
        if (!crouch && isCrouching && !CanStand())
        {
            crouch = true;
        }

        isCrouching = crouch;
        characterController.height = crouch ? originalColliderHeight * crouchScale : originalColliderHeight;
    }

    private bool CanStand()
    {
        float crouchedHeight = characterController.height * crouchScale;
        Vector3 topPlayer = characterController.transform.position + Vector3.up * crouchedHeight;
        Vector3 boxHalfExtents = new Vector3(characterController.radius, (originalColliderHeight - crouchedHeight) / 2f, characterController.radius);

        Collider[] colliders = Physics.OverlapBox(
            topPlayer,
            boxHalfExtents,
            transform.rotation,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        foreach (Collider collider in colliders)
        {
            if (collider != characterController && !collider.transform.IsChildOf(transform))
            {
                return false;
            }
        }

        return true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Verificar que el objeto tenga un Rigidbody activo y no sea cinemático
        if (body == null || body.isKinematic)
        {
            return;
        }

        float verticalDirection = IsDashState() ? 0.4f: 0f;

        // Calcular la dirección de empuje horizontal (plano XZ)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, verticalDirection, hit.moveDirection.z);

        // Aplicar impulso al Rigidbody
        //body.linearVelocity = pushDir * pushPower;
        body.AddForce(pushDir * pushPower, ForceMode.Impulse);
    }

    public bool IsDashState()
    {
        return dashState > 0;
    }

    public void TogglePlayState(bool state)
    {
        IsPlayState = state;
    }

    public bool IsPlayerPlayState()
    {
        return IsPlayState;
    }
}
