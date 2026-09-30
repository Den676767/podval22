using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Панели")]
    [SerializeField] private GameObject pauseMenuUI;   // Панель паузы
    [SerializeField] private TextMeshProUGUI saveNotificationText; // Текст "Игра сохранена!"

    [Header("Кнопки")]
    [SerializeField] private Button resumeButton;     // Продолжить
    [SerializeField] private Button saveButton;       // Сохранить
    [SerializeField] private Button menuButton;       // В главное меню

    [Header("Скрипты Управления Игрока")]
    // Закинь сюда свои скрипты движения игрока и поворота камеры
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour cameraLookScript;

    [Header("Сцены")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private Coroutine saveNoticeCoroutine;

    private void Awake()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(LoadMainMenu);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (saveNotificationText != null)
            saveNotificationText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f; // Возобновляем время
        isPaused = false;

        // Включаем управление персонажем и камерой обратно
        SetPlayerControlsActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f; // Замораживаем физику и таймеры
        isPaused = true;

        // Отключаем управление, чтобы игрок не крутил камерой и не ходил
        SetPlayerControlsActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SaveGame()
    {
        if (SceneSaveManager.Instance != null)
        {
            SceneSaveManager.Instance.SaveFullState();
        }

        if (saveNotificationText != null)
        {
            if (saveNoticeCoroutine != null) StopCoroutine(saveNoticeCoroutine);
            saveNoticeCoroutine = StartCoroutine(ShowSaveNotification());
        }
    }

    // Корутина показа уведомления про сохранение (работает даже при Time.timeScale = 0)
    private IEnumerator ShowSaveNotification()
    {
        saveNotificationText.gameObject.SetActive(true);
        saveNotificationText.text = "ИГРА СОХРАНЕНА!";

        Color textColor = saveNotificationText.color;
        textColor.a = 1f;
        saveNotificationText.color = textColor;

        // Ждём 1.5 секунды реального времени (unscaled)
        yield return new WaitForSecondsRealtime(1.5f);

        // Плавное угасание текста
        float fadeTimer = 0f;
        while (fadeTimer < 1f)
        {
            fadeTimer += Time.unscaledDeltaTime; // Считаем реальное время
            textColor.a = Mathf.Lerp(1f, 0f, fadeTimer / 1f);
            saveNotificationText.color = textColor;
            yield return null;
        }

        saveNotificationText.gameObject.SetActive(false);
    }

    private void SetPlayerControlsActive(bool isActive)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = isActive;

        if (cameraLookScript != null)
            cameraLookScript.enabled = isActive;
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f; // Возвращаем время
        SceneManager.LoadScene(mainMenuSceneName);
    }
}