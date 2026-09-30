using System.Collections;
using UnityEngine;
using TMPro;

public class DoorTriggerArea : MonoBehaviour
{
    [Header("Объект двери")]
    public Transform doorTransform;                       // Перетащи сюда объект ДВЕРИ из Hierarchy
    public Vector3 openRotation = new Vector3(0, 90, 0);  // Поворот двери
    public float openSpeed = 3f;                          // Скорость открытия

    [Header("UI Текст")]
    public TextMeshProUGUI subText;
    public string lockedText = "Дверь заперта.";

    private bool isOpen = false;
    private bool inTrigger = false;
    private bool isAnimating = false;

    private Quaternion closedQuaternion;
    private Quaternion openQuaternion;

    private void Start()
    {
        if (doorTransform != null)
        {
            // Запоминаем исходное положение двери
            closedQuaternion = doorTransform.localRotation;
            openQuaternion = closedQuaternion * Quaternion.Euler(openRotation);
        }
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
        if (inTrigger && Input.GetMouseButtonDown(0) && !isAnimating)
        {
            if (PictureTrigger.hasKey)
            {
                StartCoroutine(AnimateDoor());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(ShowLockedText());
            }
        }
    }

    private IEnumerator AnimateDoor()
    {
        if (doorTransform == null) yield break;

        isAnimating = true;
        isOpen = !isOpen;

        // Если на самой двери есть BoxCollider (физическая преграда), 
        // выключаем его при открытии, чтобы свободно проходить
        Collider doorCollider = doorTransform.GetComponent<Collider>();
        if (doorCollider != null && isOpen)
        {
            doorCollider.enabled = false;
        }

        Quaternion targetRotation = isOpen ? openQuaternion : closedQuaternion;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * openSpeed;
            doorTransform.localRotation = Quaternion.Slerp(doorTransform.localRotation, targetRotation, progress);
            yield return null;
        }

        doorTransform.localRotation = targetRotation;

        // Если дверь закрыли обратно — включаем физику двери снова
        if (doorCollider != null && !isOpen)
        {
            doorCollider.enabled = true;
        }

        isAnimating = false;
    }

    private IEnumerator ShowLockedText()
    {
        if (subText != null)
        {
            subText.text = lockedText;
            yield return new WaitForSeconds(2.5f);
            subText.text = "";
        }
    }
}