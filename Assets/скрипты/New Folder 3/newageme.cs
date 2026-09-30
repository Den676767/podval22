using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewGameLoader : MonoBehaviour
{
    [Header("Кнопка")]
    [SerializeField] private Button newGameButton;

    [Header("Настройки сцены")]
    [SerializeField] private string firstLevelSceneName = "GameScene";
    [SerializeField] private float fadeDuration = 1f;

    [Header("UI & Звук (Опционально)")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private AudioSource menuAudioSource;

    private bool isLoading = false;

    private void Awake()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(StartNewGame);
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }
    }

    public void StartNewGame()
    {
        if (isLoading) return;
        StartCoroutine(NewGameRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
        isLoading = true;

        if (newGameButton != null)
            newGameButton.interactable = false;

        if (fadeOverlay != null)
            fadeOverlay.blocksRaycasts = true;

        float startVolume = menuAudioSource != null ? menuAudioSource.volume : 0f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeDuration;

            if (fadeOverlay != null)
                fadeOverlay.alpha = progress;

            if (menuAudioSource != null)
                menuAudioSource.volume = Mathf.Lerp(startVolume, 0f, progress);

            yield return null;
        }

        // Загружаем чистую сцену
        SceneManager.LoadScene(firstLevelSceneName);
    }
}