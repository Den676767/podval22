using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class PlayMenuManager : MonoBehaviour
{
    [Header("Главное меню (которое скрываем)")]
    public CanvasGroup mainMenuGroup;   // Твой ButtonsGroup (Название + Кнопки)

    [Header("Настройки сцены")]
    public string gameSceneName = "GameScene";
    public float fadeSpeed = 3f;

    private CanvasGroup playGroup;
    private Coroutine activeTransition;

    void Awake()
    {
        playGroup = GetComponent<CanvasGroup>();

        // При старте скрываем экран выбора игры
        playGroup.alpha = 0f;
        playGroup.interactable = false;
        playGroup.blocksRaycasts = false;
    }

    // Вызывается из Главного Меню по кнопке "ИГРАТЬ"
    public void OpenPlayMenu()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionRoutine(mainMenuGroup, playGroup));
    }

    // Вызывается по кнопке "НАЗАД"
    public void ClosePlayMenu()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionRoutine(playGroup, mainMenuGroup));
    }

    // Кнопка "НОВАЯ ИГРА"
    public void StartNewGame()
    {
        PlayerPrefs.DeleteKey("SaveSlot"); // Сброс старого сохранения
        SceneManager.LoadScene(gameSceneName);
    }

    // Кнопка "ПРОДОЛЖИТЬ"
    public void ContinueGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Плавный переключатель экранов (аналогично настройкам)
    private IEnumerator TransitionRoutine(CanvasGroup fromGroup, CanvasGroup toGroup)
    {
        if (fromGroup != null)
        {
            fromGroup.interactable = false;
            fromGroup.blocksRaycasts = false;
        }

        // 1. Растворяем текущий экран
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

            toGroup.interactable = true;
            toGroup.blocksRaycasts = true;
        }
    }
}