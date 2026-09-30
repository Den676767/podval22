using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    [Header("Кнопка Выхода (Опционально)")]
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        // Если кнопка перетащена в инспектор, автоматически привязываем клик
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(ExitGame);
        }
    }

    // Метод для вызова
    public void ExitGame()
    {
        Debug.Log("Выход из игры...");

        // Закрывает скомпилированное приложение (.exe / билд)
        Application.Quit();

        // Если игра запущена прямо в редакторе Unity — останавливает воспроизведение
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}