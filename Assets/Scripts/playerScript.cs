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

    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float speed = 12f;
    public float pushPower = 1;
    public float marginHorizontal = 6f;
    public float cameraOffsetY = 4f;
    public float dashSpeed = 30;
    public float dashDuration = 0.5f;
    public Transform cameraTransform;

    void Awake()
    {
        actionsController = new ActionsController();
        characterController = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        actionsController.Player.Enable();
    }

    void OnDisable()
    {
        actionsController.Player.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fixedPlayerZ = transform.position.z;
        _pushPower = pushPower;
    }

    // Update is called once per frame
    void Update()
    {
        handleJumping();

        handleDash();

        if (dashState == 0)
        {
            Vector3 movement = transform.right * GetMovementInput().x * speed + transform.up * verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }

        Vector3 currentPosition = characterController.transform.position;
        characterController.transform.position = new Vector3(currentPosition.x, currentPosition.y, fixedPlayerZ);

        fixCameraPosition();
    }

    private void fixCameraPosition()
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

    private void handleJumping()
    {
        bool isGrounded = characterController.isGrounded;
        bool jump = actionsController.Player.jump.WasPressedThisFrame();

        if (isGrounded && jump)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (!isGrounded || verticalVelocity > 0f)
        {
            verticalVelocity += gravity * Time.deltaTime * 3f;
        }
    }

    private void handleDash()
    {
        bool dash = actionsController.Player.dash.WasPressedThisFrame();

        if (dash && dashState == 0) dashState = dashDuration;   
        
        if (dashState > 0)
        {
            Vector3 movement = transform.right * lastDirection * dashSpeed;
            characterController.Move(movement * Time.deltaTime);
            pushPower = _pushPower * 6;
            dashState -= Time.deltaTime;
        } else if (dashState < 0)
        {
            dashState = 0;
            pushPower = _pushPower;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Verificar que el objeto tenga un Rigidbody activo y no sea cinemático
        if (body == null || body.isKinematic)
        {
            return;
        }

        // Evitar empujar objetos si estamos cayendo sobre ellos
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        // Calcular la dirección de empuje horizontal (plano XZ)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z - 15f);

        // Aplicar impulso al Rigidbody
        body.linearVelocity = pushDir * pushPower;
    }
}
