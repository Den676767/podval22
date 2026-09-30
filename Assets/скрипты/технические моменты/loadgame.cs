using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveGameLoader : MonoBehaviour
{
    [Header("Кнопка")]
    [SerializeField] private Button loadGameButton;         // Кнопка "Загрузить / Продолжить"

    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 1.5f;      // Длительность ухода в темноту
    [SerializeField] private string defaultSceneIfNoSave = "GameScene"; // Сцена по умолчанию

    [Header("UI & Звук (Опционально)")]
    [SerializeField] private CanvasGroup fadeOverlay;       // Чёрный экран на весь Canvas
    [SerializeField] private AudioSource menuAudioSource;   // Музыка в меню

    private bool isLoading = false;

    private void Start()
    {
        // Проверку делаем в Start(), а не в Awake(), чтобы все UI-компоненты успели инициализироваться
        CheckSaveFile();

        if (loadGameButton != null)
        {
            loadGameButton.onClick.RemoveAllListeners();
            loadGameButton.onClick.AddListener(LoadSave);
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }
    }

    // Принудительная проверка наличия сохранения
    public void CheckSaveFile()
    {
        bool hasSave = PlayerPrefs.HasKey("SavedScene");

        if (loadGameButton != null)
        {
            loadGameButton.interactable = hasSave;
        }

        Debug.Log("[SaveGameLoader] Сохранение найдено: " + hasSave);
    }

    public void LoadSave()
    {
        if (isLoading) return;
        StartCoroutine(LoadSaveRoutine());
    }

    private IEnumerator LoadSaveRoutine()
    {
        isLoading = true;

        if (loadGameButton != null)
            loadGameButton.interactable = false;

        if (fadeOverlay != null)
            fadeOverlay.blocksRaycasts = true;

        float startVolume = menuAudioSource != null ? menuAudioSource.volume : 0f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // Используем unscaledDeltaTime
            float progress = timer / fadeDuration;

            if (fadeOverlay != null)
                fadeOverlay.alpha = progress;

            if (menuAudioSource != null)
                menuAudioSource.volume = Mathf.Lerp(startVolume, 0f, progress);

            yield return null;
        }

        // Загружаем сохранённую сцену
        string targetScene = PlayerPrefs.GetString("SavedScene", defaultSceneIfNoSave);
        SceneManager.LoadScene(targetScene);
    }
}