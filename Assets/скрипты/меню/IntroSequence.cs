using System.Collections;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    [Header("Экраны и Элементы")]
    public CanvasGroup titleGroup;      // Экран 1: Название по центру
    public CanvasGroup backgroundGroup; // Задний фон (Картинка меню)
    public CanvasGroup storyGroup;      // Экран 2: Текст предыстории
    public CanvasGroup buttonsGroup;    // Экран 3: Главное меню (кнопки + название)

    [Header("Аудио")]
    public AudioSource bgMusic;         // Музыка меню (должна быть с плавной громкостью)

    [Header("Настройки Времени")]
    public float initialDarkDelay = 2.0f;  // Начальная темнота
    public float backgroundDelay = 1.5f;   // Пауза перед появлением фона
    public float fadeSpeed = 1f;           // Скорость затухания/проявления
    public float storyDisplayTime = 7f;    // Время показа предыстории

    private int currentState = 0; // 0 = Старт, 1 = Ожидание клика, 2 = История, 3 = Меню
    private bool isFading = false;
    private Coroutine autoAdvanceCoroutine;

    void Start()
    {
        // Скрываем все элементы в самом начале
        SetGroupAlpha(titleGroup, 0f, false);
        SetGroupAlpha(backgroundGroup, 0f, false);
        SetGroupAlpha(storyGroup, 0f, false);
        SetGroupAlpha(buttonsGroup, 0f, false);

        if (bgMusic != null)
        {
            bgMusic.volume = 0f;
            bgMusic.Stop();
        }

        StartCoroutine(StartIntro());
    }

    IEnumerator StartIntro()
    {
        isFading = true;

        // 1. Полная темнота 2 секунды
        yield return new WaitForSeconds(initialDarkDelay);

        // 2. Плавно проявляем только Название
        yield return StartCoroutine(FadeGroup(titleGroup, 0f, 1f));

        // 3. Задержка 1.5 секунды перед включением фона и музыки
        yield return new WaitForSeconds(backgroundDelay);

        // 4. Плавно проявляем Задний Фон и включаем Музыку
        if (bgMusic != null) bgMusic.Play();
        StartCoroutine(FadeAudio(bgMusic, 0f, 1f));
        yield return StartCoroutine(FadeGroup(backgroundGroup, 0f, 1f));

        isFading = false;
        currentState = 1; // Теперь ждем клика от игрока
    }

    void Update()
    {
        // Реакция на клик мыши или любую клавишу
        if ((Input.GetMouseButtonDown(0) || Input.anyKeyDown) && !isFading)
        {
            if (currentState == 1)
            {
                // Клик после появления фона -> Переходим к Предыстории
                GoToStory();
            }
            else if (currentState == 2)
            {
                // Клик во время Предыстории -> Пропускаем таймер и сразу в Меню
                if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
                GoToButtons();
            }
        }
    }

    void GoToStory()
    {
        StartCoroutine(TransitionToStory());
    }

    IEnumerator TransitionToStory()
    {
        isFading = true;
        currentState = 2;

        // Гасим Название
        yield return StartCoroutine(FadeGroup(titleGroup, 1f, 0f));

        // Проявляем Текст Предыстории
        yield return StartCoroutine(FadeGroup(storyGroup, 0f, 1f));

        isFading = false;

        // Запускаем таймер показа истории на 7 секунд
        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceStory());
    }

    IEnumerator AutoAdvanceStory()
    {
        yield return new WaitForSeconds(storyDisplayTime);
        if (!isFading && currentState == 2)
        {
            GoToButtons();
        }
    }

    void GoToButtons()
    {
        StartCoroutine(TransitionToButtons());
    }

    IEnumerator TransitionToButtons()
    {
        isFading = true;
        currentState = 3;

        // Гасим Предысторию
        yield return StartCoroutine(FadeGroup(storyGroup, 1f, 0f));

        // Проявляем Главное Меню (Кнопки)
        yield return StartCoroutine(FadeGroup(buttonsGroup, 0f, 1f));

        isFading = false;
    }

    // Плавное изменение прозрачности UI
    IEnumerator FadeGroup(CanvasGroup group, float startAlpha, float endAlpha)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeSpeed;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, timer);
            yield return null;
        }
        group.alpha = endAlpha;

        bool isFull = endAlpha > 0.5f;
        group.interactable = isFull;
        group.blocksRaycasts = isFull;
    }

    // Плавное нарастание/затухание громкости музыки
    IEnumerator FadeAudio(AudioSource audio, float startVol, float endVol)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeSpeed;
            audio.volume = Mathf.Lerp(startVol, endVol, timer);
            yield return null;
        }
        audio.volume = endVol;
    }

    void SetGroupAlpha(CanvasGroup group, float alpha, bool interactable)
    {
        group.alpha = alpha;
        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }
}