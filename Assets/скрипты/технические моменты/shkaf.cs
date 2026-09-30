using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [Header("Настройки открытия")]
    public Vector3 openRotationOffset = new Vector3(0, 90, 0); // Поворот на 90 градусов
    public float openSpeed = 3f;                               // Скорость открытия
    public float interactDistance = 3f;                        // Дистанция клика

    [Header("Ссылки")]
    public Camera mainCamera;

    private bool isOpen = false;
    private bool inTrigger = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        initialRotation = transform.localRotation;
        targetRotation = initialRotation * Quaternion.Euler(openRotationOffset);
    }

    private void OnTriggerEnter(Collider other)
    {
        inTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        inTrigger = false;
    }

    private void Update()
    {
        // Нажатие E или ЛКМ (0)
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            TryOpenDoor();
        }

        // Плавный поворот двери
        if (isOpen)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    private void TryOpenDoor()
    {
        // Проверяем флаг из твоего скрипта ящика DrawerMove!
        if (!DrawerMove.hasBlueKey)
        {
            Debug.Log("Дверь заперта! Нужен синий ключ из ящика.");
            return;
        }

        // Если открываем по клику / взгляду:
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                isOpen = true;
            }
        }
        // Если открываем просто заходя в триггер и нажимая E:
        else if (inTrigger)
        {
            isOpen = true;
        }
    }
}