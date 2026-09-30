using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Движение")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 5.5f;
    public float gravity = -9.81f;
    public bool canMove = true;

    [Header("Настройки смещения (левый сдвиг)")]
    public float leftBiasStrength = 0.15f;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!canMove) return;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        moveDirection = (forward * moveY * currentSpeed) + (right * moveX * currentSpeed);

        // Легкий сдвиг влево при ходьбе вперед (W)
        if (moveY > 0.1f)
        {
            moveDirection -= right * (currentSpeed * leftBiasStrength);
        }

        if (!controller.isGrounded)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -0.5f;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }
}