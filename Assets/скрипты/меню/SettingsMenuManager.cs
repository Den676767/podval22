using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SettingsMenuManager : MonoBehaviour
{
    [Header("Главное меню (куда возвращаться)")]
    public CanvasGroup mainMenuGroup;   // Твой ButtonsGroup (Название + Кнопки)

    [Header("Элементы UI")]
    public Slider volumeSlider;

    [Header("Настройки анимации")]
    public float fadeSpeed = 3f;

    private CanvasGroup settingsGroup;
    private Coroutine activeTransition;

    void Awake()
    {
        settingsGroup = GetComponent<CanvasGroup>();

        // При старте полностью скрываем настройки
        settingsGroup.alpha = 0f;
        settingsGroup.interactable = false;
        settingsGroup.blocksRaycasts = false;
    }

    void Start()
    {
        // Инициализация звука
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Вызывается из Главного Меню по кнопке "НАСТРОЙКИ"
    public void OpenSettings()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionRoutine(mainMenuGroup, settingsGroup));
    }

    // Вызывается по кнопке "НАЗАД" в окне настроек
    public void CloseSettings()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionRoutine(settingsGroup, mainMenuGroup));
    }

    // Изменение громкости
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    // Плавный переключатель экранов
    private IEnumerator TransitionRoutine(CanvasGroup fromGroup, CanvasGroup toGroup)
    {
        // Блокируем клики на исчезающем экране
        if (fromGroup != null)
        {
            fromGroup.interactable = false;
            fromGroup.blocksRaycasts = false;
        }

        // 1. Гасим текущий экран
        if (fromGroup != null)
        {
            float timer = 0f;
            float startAlpha = fromGroup.alpha;

            while (timer < 1f)
            {
                timer += Time.deltaTime * fadeSpeed;
                fromGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer);
                yield return null;
            }
            fromGroup.alpha = 0f;
        }

        // 2. Проявляем новый экран
        if (toGroup != null)
        {
            float timer = 0f;
            float startAlpha = toGroup.alpha;

            while (timer < 1f)
            {
                timer += Time.deltaTime * fadeSpeed;
                toGroup.alpha = Mathf.Lerp(startAlpha, 1f, timer);
                yield return null;
            }
            toGroup.alpha = 1f;

            // Включаем клики
            toGroup.interactable = true;
            toGroup.blocksRaycasts = true;
        }
    }
}