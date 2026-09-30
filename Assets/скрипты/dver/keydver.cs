using System.Collections;
using UnityEngine;
using TMPro;

public class PictureTrigger : MonoBehaviour
{
    // Тот самый флаг, который читает дверь напрямую из этого скрипта
    public static bool hasKey = false;

    [Header("UI Текст")]
    public TextMeshProUGUI subText;
    public string message = "Железный ключ от железной двери... Логично.";
    public float displayDuration = 3.5f;

    private bool inTrigger = false;
    private int clickCount = 0;

    private void Start()
    {
        hasKey = false;
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
        if (inTrigger && Input.GetMouseButtonDown(0))
        {
            clickCount++;

            if (clickCount == 2)
            {
                hasKey = true; // Поднимаем флаг в PictureTrigger
                StartCoroutine(ShowTextRoutine());
            }
        }
    }

    private IEnumerator ShowTextRoutine()
    {
        if (subText != null)
        {
            subText.text = message;
            yield return new WaitForSeconds(displayDuration);
            subText.text = "";
        }
    }
}