using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ScreenFade : MonoBehaviour
{
    public float fadeDuration = 1.5f; // Время появления из темноты
    private Image fadeImage;

    void Start()
    {
        fadeImage = GetComponent<Image>();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        // Плавно убираем черный цвет (от 1 до 0)
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;

        // Отключаем панель, чтобы она не перекрывала клики мышки по игре
        gameObject.SetActive(false);
    }
}