using UnityEngine;
using UnityEngine.UI;

public class MenuBackgroundAnimator : MonoBehaviour
{
    [Header("Настройки 'Дыхания' (Масштаб)")]
    public float scaleSpeed = 0.5f;     // Скорость пульсации
    public float scaleAmount = 0.03f;   // Насколько сильно увеличивается картинка

    [Header("Настройки Мерцания Света")]
    public float lightSpeed = 1.2f;     // Базовая скорость изменения яркости
    public float minBrightness = 0.4f; // Минимальная яркость при затухании
    public float maxBrightness = 1.0f;  // Максимальная яркость

    [Header("Рандомизация и Сбои")]
    [Range(0f, 1f)]
    public float glitchChance = 0.05f;  // Вероятность резкого скачка/мигания света (0.05 = 5%)

    private Image bgImage;
    private Vector3 initialScale;
    private Color initialColor;

    void Start()
    {
        bgImage = GetComponent<Image>();
        initialScale = transform.localScale;
        if (bgImage != null) initialColor = bgImage.color;
    }

    void Update()
    {
        // 1. Эффект плавного дыхания (Zoom In / Zoom Out)
        float scaleSine = Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = initialScale + new Vector3(scaleSine, scaleSine, 0);

        // 2. Рандомный хаотичный свет
        if (bgImage != null)
        {
            // Наложение трех разных частот дает нерегулярный волновой рисунок
            float wave1 = Mathf.Sin(Time.time * lightSpeed * 1.3f);
            float wave2 = Mathf.Cos(Time.time * lightSpeed * 2.7f);
            float noise = Mathf.PerlinNoise(Time.time * lightSpeed * 0.8f, 15.35f);

            // Объединяем и нормализуем значение в диапазон 0..1
            float combinedNoise = (wave1 + wave2 + (noise * 2f - 1f)) / 3f;
            combinedNoise = Mathf.Clamp01((combinedNoise + 1f) * 0.5f);

            float brightness = Mathf.Lerp(minBrightness, maxBrightness, combinedNoise);

            // Редкие случайные вспышки или резкие затухания лампы
            if (Random.value < glitchChance)
            {
                brightness *= Random.Range(0.3f, 1.2f);
            }

            brightness = Mathf.Clamp(brightness, minBrightness, maxBrightness);

            bgImage.color = new Color(
                initialColor.r * brightness,
                initialColor.g * brightness,
                initialColor.b * brightness,
                initialColor.a
            );
        }
    }
}