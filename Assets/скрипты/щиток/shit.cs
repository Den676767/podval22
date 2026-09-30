using System.Collections;
using UnityEngine;
using TMPro;

public class FuseBoxInteractive : MonoBehaviour
{
    [Header("Объекты для поворота")]
    public Transform doorTransform;        // Модель дверцы
    public Transform leverTransform;       // Модель рычага

    [Header("Настройки дверцы")]
    public Vector3 doorRotationAxis = new Vector3(0, -90, 0); // Поворот дверцы на 90 градусов
    public float doorRotateDuration = 0.6f;

    [Header("Настройки рычага")]
    public Vector3 leverRotationAxis = new Vector3(-90, 0, 0); // Поворот рычага вверх на 90 градусов
    public float leverRotateDuration = 0.4f;
    [Header("Звук включения света")]
    public AudioClip switchOnSound;        // Аудиоклип включения света
    [Header("Свет")]
    public LightGroup lightGroup;          // Объект LightManager со скриптом LightGroup

    [Header("UI Текст при повторном клике")]
    public TextMeshProUGUI subText;        // Ссылка на TextMeshPro из Canvas
    public float textFadeDuration = 0.8f;   // Скорость проявления/исчезновения
    public float textDisplayDuration = 3.0f;// Сколько секунд висит текст

    [TextArea(2, 3)]
    public string alreadyOnMessage = "Мне кажется, с включенным светом будет лучше...";

    private int step = 0;                  // 0 = Закрыто, 1 = Дверца открыта, 2 = Свет уже включен
    private bool isAnimating = false;
    private bool playerInTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        playerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        playerInTrigger = false;
    }

    private void Update()
    {
        // Нажатие ЛКМ
        if (playerInTrigger && Input.GetMouseButtonDown(0) && !isAnimating)
        {
            if (step == 0)
            {
                // Нажатие 1: Открываем дверцу
                StartCoroutine(RotateObject(doorTransform, doorRotationAxis, doorRotateDuration));
                step = 1;
            }
            else if (step == 1)
            {
                // Нажатие 2: Поднимаем рычаг и включаем свет
                StartCoroutine(RotateLeverAndTurnOnLight());
                step = 2;
            }
            else if (step == 2)
            {
                // Нажатие 3+: Игрок пытается щелкать щиток при включенном свете
                StartCoroutine(ShowAlreadyOnText());
            }
        }
    }

    private IEnumerator RotateLeverAndTurnOnLight()
    {
        yield return StartCoroutine(RotateObject(leverTransform, leverRotationAxis, leverRotateDuration));
        if (switchOnSound != null)
        {
            AudioSource.PlayClipAtPoint(switchOnSound, transform.position);
        }
        // Включаем свет обратно
        if (lightGroup != null)
        {
            lightGroup.TurnOnAllLights();
        }
    }

    private IEnumerator ShowAlreadyOnText()
    {
        if (subText == null) yield break;

        isAnimating = true;
        subText.text = alreadyOnMessage;

        // Fade In
        yield return StartCoroutine(FadeAlpha(0f, 1f, textFadeDuration));

        // Wait
        yield return new WaitForSeconds(textDisplayDuration);

        // Fade Out
        yield return StartCoroutine(FadeAlpha(1f, 0f, textFadeDuration));

        subText.text = "";
        isAnimating = false;
    }

    private IEnumerator FadeAlpha(float startAlpha, float targetAlpha, float duration)
    {
        Color c = subText.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            subText.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        subText.color = c;
    }

    private IEnumerator RotateObject(Transform target, Vector3 rotationAngles, float duration)
    {
        if (target == null) yield break;

        isAnimating = true;

        Quaternion startRot = target.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(rotationAngles);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            target.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        target.localRotation = endRot;
        isAnimating = false;
    }
}