using System.Collections;
using UnityEngine;
using TMPro;

public class DrawerMove : MonoBehaviour
{
    // Глобальный флаг для системы шкафчиков
    public static bool hasBlueKey = false;

    [Header("Настройки движения")]
    [Tooltip("На сколько единиц выдвигать полочку (например, X=0, Y=0, Z=0.5)")]
    public Vector3 moveDistance = new Vector3(0f, 0f, 0.5f);

    [Tooltip("Скорость выдвижения")]
    public float moveSpeed = 3f;

    [Header("UI Текст")]
    public TextMeshProUGUI subText;
    public string message = "синий ключ . наверное для шкафчиков";
    public float fadeDuration = 0.8f;      // Время появления/исчезновения текста
    public float displayDuration = 3.5f;   // Время показа текста

    private bool isOpen = false;
    private bool inTrigger = false;
    private bool isAnimating = false;
    private bool messageShown = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private void Start()
    {
        hasBlueKey = false; // Сбрасываем флаг при старте сцены
        closedPosition = transform.localPosition;
        openPosition = closedPosition + moveDistance;

        // Если текст назначен, ставим прозрачность в 0
        if (subText != null)
        {
            Color c = subText.color;
            c.a = 0f;
            subText.color = c;
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
        if (inTrigger && !isAnimating && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)))
        {
            StartCoroutine(AnimateDrawer());
        }
    }

    private IEnumerator AnimateDrawer()
    {
        isAnimating = true;
        isOpen = !isOpen;

        Vector3 targetPosition = isOpen ? openPosition : closedPosition;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * moveSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, progress);
            yield return null;
        }

        transform.localPosition = targetPosition;
        isAnimating = false;

        // При первом открытии ящика: активируем флаг ключа и плавно выводим текст
        if (isOpen && !messageShown)
        {
            messageShown = true;
            hasBlueKey = true; // Запоминаем синий ключ для шкафчиков!
            StartCoroutine(FadeTextRoutine());
        }
    }

    private IEnumerator FadeTextRoutine()
    {
        if (subText == null) yield break;

        subText.text = message;
        Color color = subText.color;

        // 1. Плавный Fade In (проявление)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            subText.color = color;
            yield return null;
        }
        color.a = 1f;
        subText.color = color;

        // 2. Удержание текста на экране
        yield return new WaitForSeconds(displayDuration);

        // 3. Плавный Fade Out (затухание)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            subText.color = color;
            yield return null;
        }
        color.a = 0f;
        subText.color = color;
        subText.text = "";
    }
}