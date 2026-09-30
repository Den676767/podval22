using System.Collections;
using UnityEngine;
using TMPro;

public class TextTrigger : MonoBehaviour
{
    [Header("Настройки Текста")]
    public TextMeshProUGUI subText;         // Ссылка на компонент UI-текста
    [TextArea]
    public string[] messages = new string[] // Список сообщений для показа по очереди
    {
        "Кажется, тут что-то есть...",
        "Нужно быть осторожнее."
    };

    [Header("Тайминги")]
    public float fadeDuration = 0.5f;       // Время появления/исчезновения
    public float displayDuration = 3f;      // Сколько секунда висит каждое сообщение
    public float delayBetweenMessages = 0.5f; // Пауза между сообщениями

    [Header("Настройки Триггера")]
    public bool triggerOnce = true;         // Срабатывать только один раз?

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered && triggerOnce) return;

        // Проверяем, что в триггер зашел игрок
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            isTriggered = true;
            StartCoroutine(ShowMessagesRoutine());
        }
    }

    private IEnumerator ShowMessagesRoutine()
    {
        if (subText == null || messages == null || messages.Length == 0) yield break;

        foreach (string message in messages)
        {
            subText.text = message;
            Color color = subText.color;

            // 1. Появление (Fade In)
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

            // 2. Удержание на экране
            yield return new WaitForSeconds(displayDuration);

            // 3. Исчезновение (Fade Out)
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

            // Пауза перед следующим сообщением (если оно есть)
            yield return new WaitForSeconds(delayBetweenMessages);
        }
    }
}