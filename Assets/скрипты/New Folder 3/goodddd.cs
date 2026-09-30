using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GoodEndingUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Image photoImage;             // Сама фотка
    [SerializeField] private CanvasGroup storyTextGroup;   // Группа с текстом истории
    [SerializeField] private CanvasGroup menuGroup;        // Группа "Хорошая концовка" + Кнопка
    [SerializeField] private CanvasGroup fadeOverlay;      // Чёрный экран
    [SerializeField] private Button mainMenuButton;        // Кнопка выхода в меню
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Музыка и Звук")]
    [SerializeField] private AudioSource bgmAudioSource;   // Источник музыки (AudioSource)
    [SerializeField] private AudioClip bgmClip;            // Сама музыкальная дорожка

    [Header("Настройки времени")]
    [SerializeField] private float textDisplayTime = 10.0f; // Сколько висит текст

    private Color originalPhotoColor;
    private Vector3 originalPhotoScale;
    private bool isRunning = true;

    private void Awake()
    {
        if (storyTextGroup != null) storyTextGroup.alpha = 0f;
        if (menuGroup != null)
        {
            menuGroup.alpha = 0f;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f;
            fadeOverlay.blocksRaycasts = false;
        }

        if (photoImage != null)
        {
            originalPhotoColor = photoImage.color;
            originalPhotoScale = photoImage.rectTransform.localScale;
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMenuClick);
        }

        // Подготовка звука
        if (bgmAudioSource != null && bgmClip != null)
        {
            bgmAudioSource.clip = bgmClip;
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Запуск музыки
        if (bgmAudioSource != null && bgmClip != null)
        {
            bgmAudioSource.Play();
        }

        StartCoroutine(EndingSequence());
        StartCoroutine(PhotoEffects());
    }

    private IEnumerator EndingSequence()
    {
        // 1. Из темноты плавно проявляются фотка и текст
        float timer = 0f;
        while (timer < 2.5f)
        {
            timer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, timer / 2.5f);

            if (fadeOverlay != null) fadeOverlay.alpha = 1f - progress;
            if (storyTextGroup != null) storyTextGroup.alpha = progress;

            yield return null;
        }

        // 2. Чтение (10 секунд)
        yield return new WaitForSeconds(textDisplayTime);

        // 3. Плавно убираем текст
        timer = 0f;
        while (timer < 2.0f)
        {
            timer += Time.deltaTime;
            if (storyTextGroup != null)
                storyTextGroup.alpha = Mathf.SmoothStep(1f, 0f, timer / 2.0f);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 4. Проявляем заголовок и кнопку
        timer = 0f;
        while (timer < 2.0f)
        {
            timer += Time.deltaTime;
            if (menuGroup != null)
                menuGroup.alpha = Mathf.SmoothStep(0f, 1f, timer / 2.0f);

            yield return null;
        }

        if (menuGroup != null)
        {
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
        }
    }

    private IEnumerator PhotoEffects()
    {
        while (isRunning)
        {
            if (photoImage != null)
            {
                // 1. Зум: от 1.0 (исходный) до 1.05 (приближение) и обратно в 1.0
                float scale = 1f + Mathf.Abs(Mathf.Sin(Time.time * 0.5f)) * 0.05f;
                photoImage.rectTransform.localScale = originalPhotoScale * scale;

                // 2. Случайное мигание света
                if (Random.value < 0.05f)
                {
                    float flicker = Random.Range(0.7f, 1.2f);
                    photoImage.color = originalPhotoColor * flicker;
                }
                else
                {
                    photoImage.color = Color.Lerp(photoImage.color, originalPhotoColor, Time.deltaTime * 5f);
                }
            }
            yield return null;
        }
    }

    private void OnMenuClick()
    {
        StartCoroutine(LoadMenuRoutine());
    }

    private IEnumerator LoadMenuRoutine()
    {
        isRunning = false;
        if (menuGroup != null) menuGroup.interactable = false;

        float startVolume = bgmAudioSource != null ? bgmAudioSource.volume : 0f;

        // Плавно убираем изображение в чёрное И плавно глушим музыку
        float timer = 0f;
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            float progress = timer / 1.5f;

            if (fadeOverlay != null)
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, progress);

            if (bgmAudioSource != null)
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, progress);

            yield return null;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}