using System.Collections;
using UnityEngine;

public class ChoiceDoor : MonoBehaviour
{
    [Header("Объект двери")]
    public Transform doorTransform;                      // Объект полотна двери, который поворачивается

    [Header("Настройки открывания")]
    public Vector3 openRotation = new Vector3(0, 90, 0); // На какой угол поворачивать дверь
    public float openSpeed = 2f;                         // Скорость открытия

    [Header("Связанный триггер (второй выбор)")]
    public ChoiceDoor otherDoorTrigger;                  // Ссылка на ChoiceDoor второй двери

    [HideInInspector]
    public bool isOpened = false;                        // Открыта ли эта дверь
    [HideInInspector]
    public bool isLocked = false;                        // Заблокирована ли дверь (если открыли другую)

    private bool inTrigger = false;
    private bool isAnimating = false;
    private Quaternion targetRotation;

    private void Start()
    {
        // Если забыли перетащить doorTransform, берем этот же объект
        if (doorTransform == null)
        {
            doorTransform = transform;
        }

        // Вычисляем целевой поворот двери
        targetRotation = doorTransform.localRotation * Quaternion.Euler(openRotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            inTrigger = false;
        }
    }

    private void Update()
    {
        // Проверяем: находимся ли мы в зоне триггера, нажата ли E или ЛКМ, и не заблокирована ли дверь
        if (inTrigger && !isAnimating && !isOpened && !isLocked && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)))
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        isOpened = true;

        // Блокируем второй триггер выборной двери
        if (otherDoorTrigger != null)
        {
            otherDoorTrigger.isLocked = true;
        }

        StartCoroutine(AnimateOpen());
    }

    private IEnumerator AnimateOpen()
    {
        isAnimating = true;

        while (Quaternion.Angle(doorTransform.localRotation, targetRotation) > 0.5f)
        {
            doorTransform.localRotation = Quaternion.Slerp(doorTransform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        doorTransform.localRotation = targetRotation;
        isAnimating = false;
    }
}