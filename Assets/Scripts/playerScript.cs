using System;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private ActionsController actionsController;
    private CharacterController characterController;
    private Animator animator;
    private Vector3 lastPosition;
    private float verticalVelocity;
    private float fixedPlayerZ;
    private int lastDirection = 1;
    private float dashState = 0;
    private float dashCooldownTimer = 0f;
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
    public float dashCooldown = 1f;
    public float crouchScale = 0.5f;
    public float crouchSpeedMultiplier = 0.5f;
    
    // --- NUEVA VARIABLE GLOBAL ---
    [Tooltip("El centro físico en Y que tendrá el Character Controller permanentemente")]
    public float crouchCenterY = 0.2f;

    public float rotationSpeed = 12f;
    public Transform gun;
    public GameObject projectilePrefab;
    public float projectileSpeed = 30f;
    public float projectileOffset = 0.5f;
    public Material materialGreen;
    public Transform mesh;

    [Header("Idle AFK Settings")]
    [Tooltip("Tiempo en segundos antes de que el personaje haga la animación de espera")]
    public float timeToWaitAFK = 5f;
    private float afkTimer = 0f;
    
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
        lastPosition = transform.position;
        _pushPower = pushPower;
        originalColliderHeight = characterController.height;
        animator = GetComponentInChildren<Animator>();
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

        // Obtener la dirección actual del movimiento
        Vector3 inputMovement = GetMovementInput();

        if (dashState == 0)
        {
            float currentSpeed = isCrouching? speed * crouchSpeedMultiplier : speed;
            Vector3 movement = Vector3.right * GetMovementInput().x * currentSpeed + Vector3.up * verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }

        // --- NUEVA LÓGICA: CONTADOR DE ESPERA (AFK) ---
        // Verificamos si el personaje está completamente quieto en el suelo y sin agacharse
        bool isQuiet = inputMovement.x == 0 && characterController.isGrounded && !isCrouching && dashState == 0;

        if (isQuiet)
        {
            afkTimer += Time.deltaTime; // Sumamos tiempo si está quieto

            if (afkTimer >= timeToWaitAFK)
            {
                if (animator != null)
                {
                    animator.SetTrigger("playAFK"); // Activamos la animación especial
                }
                afkTimer = 0f; // Reiniciamos el contador para que pueda repetirse después
            }
        }
        else
        {
            afkTimer = 0f; // Si se mueve, salta, se agacha o hace dash, el contador vuelve a cero de inmediato
        }

        Vector3 currentPosition = characterController.transform.position;
        characterController.transform.position = new Vector3(currentPosition.x, currentPosition.y, fixedPlayerZ);
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

    private Vector3 GetMovementInput()
    {
        bool moveLeft = actionsController.Player.moveLeft.IsPressed();
        bool moveRight = actionsController.Player.moveRight.IsPressed();
        int direction = (moveLeft ? -1 : 0) + (moveRight ? 1 : 0);
        lastDirection = moveLeft ? -1 : moveRight ? 1 : lastDirection;

        float targetY = direction == 0 ? (lastDirection == 1 ? 360f : 180f) : (direction == 1 ? 360f : 180f);

        if (mesh != null)
        {
            Quaternion targetRotation = Quaternion.Euler(mesh.eulerAngles.x, targetY, mesh.eulerAngles.z);
            mesh.rotation = Quaternion.RotateTowards(mesh.rotation, targetRotation, rotationSpeed * 100f * Time.deltaTime);
        }

        // --- ACTUALIZACIÓN DE BLEND TREE ---
        if (animator != null)
        {
            // Si el jugador pulsa teclas, el valor objetivo es 1, si no, es 0
            float targetSpeedValue = (moveLeft || moveRight) ? 1f : 0f;
            float currentAnimSpeed = animator.GetFloat("speed");

            // Suaviza la transición matemática entre Idle (0) y Caminar (1)
            float smoothedSpeed = Mathf.MoveTowards(currentAnimSpeed, targetSpeedValue, Time.deltaTime * 5f);
            animator.SetFloat("speed", smoothedSpeed);
        }

        return new Vector3(direction, 0f, 0f);
    }

    private void HandleJumping()
    {
        bool isGrounded = characterController.isGrounded;
        bool jump = actionsController.Player.jump.WasPressedThisFrame();

        if (isGrounded && jump)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            //1. Animacion de salto (cuando empieza a saltar)
            if (animator != null)
            {
                animator.SetTrigger("isJumping");
            }
        } else
        {
            verticalVelocity = verticalVelocity > 0 && isCrouching ? 0 : verticalVelocity;
        }

        if (!isGrounded || verticalVelocity > 0f)
        {
            verticalVelocity += gravity * Time.deltaTime * 3f;
        }
        // CONTROL DE ANIMACIÓN PARA EL AIRE Y EL ATERRIZAJE
        if (animator != null)
        {
            // 2. MOMENTO: En el aire (si no está en el suelo y se está moviendo verticalmente)
            animator.SetBool("isInAir", !isGrounded);

            // 3. MOMENTO: Aterrizaje (enviar el estado del suelo directo al Animator)
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    private void HandleDash()
    {
        bool dash = actionsController.Player.dash.WasPressedThisFrame();
        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);

        if (dash && dashState == 0 && dashCooldownTimer <= 0f)
        {
            dashState = dashDuration;
            stats.dash();
            
            // --- NUEVA LÓGICA: Encendemos la animación del Dash ---
            if (animator != null)
            {
                animator.SetBool("isDashing", true);
            }
        } 
        
        if (IsDashState())
        {
            Vector3 movement = Vector3.right * lastDirection * dashSpeed;
            characterController.Move(movement * Time.deltaTime);
            pushPower = _pushPower * 2;
            dashState -= Time.deltaTime;
        } else if (dashState < 0)
        {
            dashState = 0;
            dashCooldownTimer = dashCooldown;
            pushPower = _pushPower;

            // --- NUEVA LÓGICA: Apagamos la animación al terminar el Dash ---
            if (animator != null)
            {
                animator.SetBool("isDashing", false);
            }
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

        if (crouch != isCrouching)
        {
        isCrouching = crouch;

        characterController.height = crouch ? originalColliderHeight * crouchScale : originalColliderHeight;

            if (crouch)
            {
                // Al agacharse baja a -0.2f
                characterController.center = new Vector3(characterController.center.x, -0.2f, characterController.center.z);
            }
            else
            {
                // Al levantarse regresa EXACTAMENTE a 0.2f
                characterController.center = new Vector3(characterController.center.x, 0.2f, characterController.center.z);
            }
        }

        // NUEVA LÓGICA DE ANIMACIÓN PARA AGACHARSE
        if (animator != null)
        {
            animator.SetBool("isDown", isCrouching);
        }
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

    public void AddLastCheckpoint(Vector3 position)
    {
        lastPosition = position;
    }

    public void MoveToLastCheckpoint()
    {
        bool wasControllerEnabled = characterController != null && characterController.enabled;
        if (wasControllerEnabled)
        {
            characterController.enabled = false;
        }

        transform.position = lastPosition;
        verticalVelocity = 0f;

        if (wasControllerEnabled)
        {
            characterController.enabled = true;
        }
    }
}
