using System.Collections;
using UnityEngine;

public class CloseDoorTrigger : MonoBehaviour
{
    [Header("Настройки двери")]
    public Transform doorTransform;        // Объект двери, которую нужно закрыть
    public float closeSpeed = 3f;           // Скорость закрытия двери

    [Header("Звук закрытия")]
    public AudioSource audioSource;        // Компонент AudioSource
    public AudioClip closeSound;           // Звуковой файл закрытия/хлопка двери
    [Range(0f, 1f)]
    public float soundVolume = 1f;         // Громкость звука

    [Header("Настройки триггера")]
    public bool triggerOnce = true;        // Срабатывать только один раз?

    private Quaternion initialRotation;    // Исходное (закрытое) положение двери
    private bool isTriggered = false;
    private bool isClosing = false;

    private void Start()
    {
        // Запоминаем начальный поворот двери при старте сцены
        if (doorTransform != null)
        {
            initialRotation = doorTransform.localRotation;
        }

        // Если AudioSource не перетащен в инспекторе, пробуем взять его с этого же объекта
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered && triggerOnce) return;

        // Проверяем, что в триггер зашел игрок
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            if (doorTransform != null && !isClosing)
            {
                isTriggered = true;
                StartCoroutine(CloseDoorRoutine());
            }
        }
    }

    private IEnumerator CloseDoorRoutine()
    {
        isClosing = true;

        // Плавно поворачиваем дверь обратно к начальному вращению
        while (Quaternion.Angle(doorTransform.localRotation, initialRotation) > 0.5f)
        {
            doorTransform.localRotation = Quaternion.Slerp(
                doorTransform.localRotation,
                initialRotation,
                Time.deltaTime * closeSpeed
            );
            yield return null;
        }

        doorTransform.localRotation = initialRotation;

        // Воспроизводим звук закрытия/захлопывания в конце движения
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound, soundVolume);
        }

        isClosing = false;
    }
}