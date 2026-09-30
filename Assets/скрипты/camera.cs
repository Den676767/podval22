using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Обзор")]
    public Transform playerBody;
    public float mouseSensitivity = 2.0f;
    public float lookLimitX = 80.0f;
    public bool canLook = true; // Главный флаг блокировки мыши

    [Header("Покачивание Камеры (Head Bob)")]
    public bool enableHeadBob = true;
    public float bobSpeedWalk = 10f;
    public float bobSpeedRun = 14f;
    public float bobAmountWalk = 0.04f;
    public float bobAmountRun = 0.07f;

    private float rotationX = 0;
    private float defaultCameraY;
    private float bobTimer = 0;
    private CharacterController playerController;

    void Start()
    {
        defaultCameraY = transform.localPosition.y;

        if (playerBody != null)
        {
            playerController = playerBody.GetComponent<CharacterController>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Если обзор заблокирован — ничего не делаем с мышкой
        if (!canLook) return;

        // 1. Вращение мышью
        rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookLimitX, lookLimitX);
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity);
        }

        // 2. Покачивание
        if (!enableHeadBob || playerController == null) return;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f) && playerController.isGrounded;

        if (isMoving)
        {
            float currentBobSpeed = isRunning ? bobSpeedRun : bobSpeedWalk;
            float currentBobAmount = isRunning ? bobAmountRun : bobAmountWalk;

            bobTimer += Time.deltaTime * currentBobSpeed;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                defaultCameraY + Mathf.Sin(bobTimer) * currentBobAmount,
                transform.localPosition.z
            );
        }
        else
        {
            bobTimer = 0;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                Mathf.Lerp(transform.localPosition.y, defaultCameraY, Time.deltaTime * 8f),
                transform.localPosition.z
            );
        }
    }
}