using System.Collections;
using UnityEngine;

public class SmoothRotation : MonoBehaviour
{
    [Header("Настройки Поворота")]
    public Vector3 rotationAxis = new Vector3(0, -90, 0); // На сколько градусов и по какой оси повернуть
    public float duration = 0.5f;                         // Скорость поворота

    [Header("Звук (Опционально)")]
    public AudioSource audioSource;
    public AudioClip rotationSound;

    private bool isRotated = false;
    private bool isMoving = false;
    private Quaternion startRotation;
    private Quaternion targetRotation;

    void Start()
    {
        startRotation = transform.localRotation;
        targetRotation = startRotation * Quaternion.Euler(rotationAxis);
    }

    public void ToggleRotation()
    {
        if (isMoving || isRotated) return; // Поворачиваем только один раз
        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        isMoving = true;

        if (audioSource != null && rotationSound != null)
        {
            audioSource.PlayOneShot(rotationSound);
        }

        float elapsed = 0f;
        Quaternion from = transform.localRotation;
        Quaternion to = targetRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localRotation = Quaternion.Slerp(from, to, percent);
            yield return null;
        }

        transform.localRotation = to;
        isRotated = true;
        isMoving = false;
    }

    public bool IsRotated() => isRotated;
}