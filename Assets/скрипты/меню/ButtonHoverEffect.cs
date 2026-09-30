using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Цвета при наведении")]
    public Color normalColor = new Color(0.75f, 0.75f, 0.75f, 0.85f);
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);               // Ярко-белый

    [Header("Эффект покачивания текста (Жизнь)")]
    public float floatSpeed = 0.6f;      // Скорость покачивания
    public float floatAmountY = 2f;      // Амплитуда (в пикселях)
    public float tiltAmount = 0.5f;      // Легкий наклон

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private TMP_Text buttonText;
    private RectTransform textRectTransform;
    private Vector2 initialTextPosition;
    private Quaternion initialTextRotation;
    private bool isHovered = false;
    private float seed;

    void Start()
    {
        buttonText = GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.color = normalColor;
            textRectTransform = buttonText.GetComponent<RectTransform>();

            if (textRectTransform != null)
            {
                initialTextPosition = textRectTransform.anchoredPosition;
                initialTextRotation = textRectTransform.localRotation;
            }
        }

        // Разнобой по времени для каждой кнопки
        seed = transform.GetSiblingIndex() * 1.5f;
    }

    void Update()
    {
        AnimateFloat();
        AnimateScaleAndColor();
    }

    // 1. Безопасное покачивание ТОЛЬКО ТЕКСТА внутри кнопки
    void AnimateFloat()
    {
        if (textRectTransform == null) return;

        float time = (Time.time + seed) * floatSpeed;
        float offsetY = Mathf.Sin(time) * floatAmountY;
        float tiltZ = Mathf.Sin(time * 0.8f) * tiltAmount;

        textRectTransform.anchoredPosition = initialTextPosition + new Vector2(0f, offsetY);
        textRectTransform.localRotation = initialTextRotation * Quaternion.Euler(0f, 0f, tiltZ);
    }

    // 2. Увеличение и белая подсветка
    void AnimateScaleAndColor()
    {
        Vector3 targetScale = isHovered ? Vector3.one * 1.08f : Vector3.one;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);

        if (buttonText != null)
        {
            Color targetColor = isHovered ? hoverColor : normalColor;
            buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.deltaTime * 12f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (audioSource != null && hoverSound != null) audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
    }
}