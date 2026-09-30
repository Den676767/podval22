using System.Collections;
using UnityEngine;
using TMPro;

public class SimpleKeyTextTrigger : MonoBehaviour
{
    public TextMeshProUGUI subText;
    public float fadeDuration = 1.0f;
    public float displayDuration = 4.0f;

    [TextArea(2, 4)]
    public string message = "Похоже, это ключ от электрощитка. Надо включить его... Вроде он был где-то в правой части подвала.";

    private bool isShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isShown) return;
        isShown = true;
        StartCoroutine(ShowTextRoutine());
    }

    private IEnumerator ShowTextRoutine()
    {
        if (subText == null) yield break;

        subText.text = message;

        // Появление
        float elapsed = 0f;
        Color c = subText.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = elapsed / fadeDuration;
            subText.color = c;
            yield return null;
        }

        // Показ
        yield return new WaitForSeconds(displayDuration);

        // Исчезновение
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = 1f - (elapsed / fadeDuration);
            subText.color = c;
            yield return null;
        }

        c.a = 0f;
        subText.color = c;
        subText.text = "";
    }
}