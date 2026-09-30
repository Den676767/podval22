using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro; // Подключаем, если текст сделан через TextMeshPro

public class TimedControlsFade : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float delayBeforeShow = 4f;   // Задержка перед появлением (секунд)
    public float fadeInTime = 1f;        // Время плавного появления
    public float displayDuration = 7.5f; // Сколько висит на экране
    public float fadeOutTime = 1f;       // Время плавного исчезновения

    void Start()
    {
        // Получаем или вешаем CanvasGroup для плавной прозрачности
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Сразу делаем панель полностью прозрачной и выключаем её интерактивность
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Запускаем корутину таймера
        StartCoroutine(ShowAndFadeRoutine());
    }

    IEnumerator ShowAndFadeRoutine()
    {
        // 1. Ждем 4 секунды после старта игры
        yield return new WaitForSeconds(delayBeforeShow);

        // 2. Плавное появление (Fade In)
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 3. Висит на экране 7.5 секунд
        yield return new WaitForSeconds(displayDuration);

        // 4. Плавное исчезновение (Fade Out)
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Полностью гасим объект, чтобы не жрал ресурсы
        gameObject.SetActive(false);
    }
}