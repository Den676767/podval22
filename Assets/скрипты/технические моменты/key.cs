using System.Collections;
using UnityEngine;

public class PaintingTrigger : MonoBehaviour
{
    [Header("Настройки картины")]
    public float rotateSpeed = 2f;         // Скорость поворота картины
    public GameObject keyObject;          // Ссылка на ключ

    [Header("Звук ключа")]
    public AudioClip keyDropSound;         // Звук падения/появления ключа
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;      // Громкость звука

    private bool isRotated = false;
    private OutlineController outlineCtrl;

    void Awake()
    {
        outlineCtrl = GetComponent<OutlineController>();

        if (keyObject != null)
        {
            keyObject.SetActive(false);
        }
    }

    public void Interact()
    {
        if (!isRotated)
        {
            isRotated = true;

            if (outlineCtrl != null)
            {
                outlineCtrl.canBeHighlighted = false;
                outlineCtrl.HighlightOff();
            }

            StartCoroutine(RotateAndDropKey());
        }
    }

    private IEnumerator RotateAndDropKey()
    {
        Quaternion targetRotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);

        // Плавно поворачиваем картину
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotateSpeed);
            yield return null;
        }

        // Фиксируем ровно в 0 по Z
        transform.localRotation = targetRotation;

        // Включаем ключ и играем звук
        if (keyObject != null)
        {
            keyObject.SetActive(true);

            // Если на ключе есть Rigidbody — даем ему физику
            Rigidbody keyRb = keyObject.GetComponent<Rigidbody>();
            if (keyRb != null)
            {
                keyRb.useGravity = true;
                keyRb.isKinematic = false;
            }

            // Включаем 3D-звук падения в позиции ключа
            if (keyDropSound != null)
            {
                AudioSource.PlayClipAtPoint(keyDropSound, keyObject.transform.position, soundVolume);
            }
        }
    }
}