using UnityEngine;

public class playerScript : MonoBehaviour
{
    private ActionsController actionsController;
    private CharacterController characterController;
    private float verticalVelocity;

    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float speed = 12f;
    public float marginHorizontal = 6f;
    public float cameraOffsetY = 4f;
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
    
    }

    // Update is called once per frame
    void Update()
    {
        HandleJumping();
        Vector3 movement = transform.right * GetMovementInput().x * speed + transform.up * verticalVelocity;      
        characterController.Move(movement * Time.deltaTime);

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
        int moveDirection = (moveLeft ? -1 : 0) + (moveRight ? 1 : 0);
        return new Vector3(moveDirection, 0f, 0f);
    }

    private void HandleJumping()
    {
        bool isGrounded = characterController.isGrounded;
        bool isJumping = actionsController.Player.jump.WasPressedThisFrame();

        if (isGrounded && isJumping)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (!isGrounded || verticalVelocity > 0f)
        {
            verticalVelocity += gravity * Time.deltaTime * 3f;
        }
    }
}
