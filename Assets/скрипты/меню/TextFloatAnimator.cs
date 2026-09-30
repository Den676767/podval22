using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextFloatAnimator : MonoBehaviour
{
    [Header("Настройки плавания всего текста")]
    public float floatSpeed = 0.8f;      // Общая скорость покачивания
    public float floatAmountY = 5f;      // Амплитуда вверх-вниз
    public float floatAmountX = 2.5f;    // Легкий сдвиг влево-вправо
    public float tiltAmount = 1.0f;      // Легкий наклон всего текста

    [Header("Настройки разобщения букв")]
    public float letterSpeed = 1.8f;     // Скорость независимого волнового движения
    public float letterOffsetY = 3.5f;   // Насколько буквы смещаются по вертикали
    public float letterOffsetX = 1.5f;   // Насколько буквы смещаются по горизонтали
    public float letterTilt = 3.0f;      // Наклон отдельной буквы в градусах

    [Header("Эффект хоррор-глитча (Сбои)")]
    [Range(0f, 1f)]
    public float glitchChance = 0.04f;   // Шанс мгновенного скачка отдельной буквы
    public float glitchIntensity = 8f;   // Сила мгновенного скачка при сбое

    [Header("Настройки прозрачности и мерцания")]
    public float alphaSpeed = 1.2f;      // Скорость изменения света
    public float minAlpha = 0.5f;        // Минимальная прозрачность
    public float maxAlpha = 1.0f;        // Максимальная прозрачность
    [Range(0f, 1f)]
    public float flickerChance = 0.03f;  // Шанс микро-проседания света

    private TMP_Text tmpText;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Color initialColor;

    void Start()
    {
        tmpText = GetComponent<TMP_Text>();
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        if (tmpText != null)
        {
            initialColor = tmpText.color;
        }
    }

    void Update()
    {
        AnimateFloatAndTilt();
        AnimateDisconnectedLetters();
        AnimateAlpha();
    }

    // 1. Общее плавное покачивание и наклон всего объекта
    void AnimateFloatAndTilt()
    {
        float time = Time.time * floatSpeed;

        float offsetY = (Mathf.Sin(time) + Mathf.Sin(time * 1.7f) * 0.5f) * floatAmountY;
        float offsetX = (Mathf.Cos(time * 0.8f) + Mathf.Sin(time * 1.3f) * 0.5f) * floatAmountX;

        transform.localPosition = initialPosition + new Vector3(offsetX, offsetY, 0f);

        float tiltZ = Mathf.Sin(time * 0.9f) * tiltAmount;
        transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, tiltZ);
    }

    // 2. Разобщенность, вращение и хоррор-глитч каждой буквы
    void AnimateDisconnectedLetters()
    {
        if (tmpText == null) return;

        // Обновляем информацию о тексте
        tmpText.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmpText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Находим центр конкретной буквы
            Vector3 charCenter = (vertices[vertexIndex] + vertices[vertexIndex + 2]) * 0.5f;

            // Индивидуальные волновые смещения
            float time = Time.time * letterSpeed;
            float waveY = Mathf.Sin(time + i * 0.7f) * letterOffsetY;
            float waveX = Mathf.Cos(time * 0.6f + i * 0.4f) * letterOffsetX;

            // Случайный глитч (мгновенный сдвиг отдельной буквы)
            Vector3 glitchOffset = Vector3.zero;
            if (Random.value < glitchChance)
            {
                glitchOffset = new Vector3(
                    Random.Range(-glitchIntensity, glitchIntensity),
                    Random.Range(-glitchIntensity, glitchIntensity),
                    0f
                );
            }

            Vector3 totalOffset = new Vector3(waveX, waveY, 0f) + glitchOffset;

            // Индивидуальный наклон буквы (вращение вокруг ее центра)
            float angle = Mathf.Sin(time * 1.2f + i) * letterTilt;
            Quaternion charRotation = Quaternion.Euler(0f, 0f, angle);

            // Применяем смещение и поворот ко всем 4 вершинам символа
            for (int j = 0; j < 4; j++)
            {
                Vector3 orig = vertices[vertexIndex + j] - charCenter;
                vertices[vertexIndex + j] = charCenter + charRotation * orig + totalOffset;
            }
        }

        // Обновляем геометрию
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }

    // 3. Атмосферная пульсация прозрачности
    void AnimateAlpha()
    {
        if (tmpText == null) return;

        float wave1 = Mathf.Sin(Time.time * alphaSpeed);
        float wave2 = Mathf.Cos(Time.time * alphaSpeed * 2.1f);
        float combinedWave = (wave1 + wave2 * 0.5f) / 1.5f;

        float alphaFactor = (combinedWave + 1f) * 0.5f;
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, alphaFactor);

        // Редкое случайное проседание света
        if (Random.value < flickerChance)
        {
            currentAlpha *= Random.Range(0.5f, 0.85f);
        }

        tmpText.color = new Color(
            initialColor.r,
            initialColor.g,
            initialColor.b,
            initialColor.a * currentAlpha
        );
    }
}