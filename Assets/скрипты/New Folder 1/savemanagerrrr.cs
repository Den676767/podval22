using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSaveManager : MonoBehaviour
{
    public static SceneSaveManager Instance;

    [Header("Игрок")]
    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // При загрузке игровой сцены проверяем: если есть сохранение — загружаем его
        LoadFullState();
    }

    // Вызывается при нажатии "Сохранить" в меню паузы
    public void SaveFullState()
    {
        // 1. Сохраняем имя текущей сцены
        PlayerPrefs.SetString("SavedScene", SceneManager.GetActiveScene().name);

        // 2. Сохраняем координаты и поворот игрока
        if (playerTransform != null)
        {
            PlayerPrefs.SetFloat("PlayerPosX", playerTransform.position.x);
            PlayerPrefs.SetFloat("PlayerPosY", playerTransform.position.y);
            PlayerPrefs.SetFloat("PlayerPosZ", playerTransform.position.z);
            PlayerPrefs.SetFloat("PlayerRotY", playerTransform.eulerAngles.y);
        }

        // 3. Сохраняем состояние всех интерактивных объектов
        foreach (var obj in SaveableObject.AllSaveables)
        {
            if (obj != null)
                obj.SaveState();
        }

        // Выставляем флаг, что сохранение существует
        PlayerPrefs.SetInt("HasSavedGame", 1);
        PlayerPrefs.Save();

        Debug.Log("<color=green>[SaveManager] Полное состояние игры успешно сохранено!</color>");
    }

    // Автоматически восстанавливает состояние игры
    public void LoadFullState()
    {
        // Проверяем: если флага сохранения нет, это "Новая Игра" — ничего не загружаем
        if (PlayerPrefs.GetInt("HasSavedGame", 0) == 0)
        {
            Debug.Log("[SaveManager] Файл сохранения отсутствует. Запуск Новой Игры.");
            return;
        }

        // 1. Восстанавливаем позицию игрока
        if (playerTransform != null && PlayerPrefs.HasKey("PlayerPosX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");
            float rotY = PlayerPrefs.GetFloat("PlayerRotY");

            // Временно отключаем CharacterController для корректного перемещения физики
            CharacterController controller = playerTransform.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            playerTransform.position = new Vector3(x, y, z);
            playerTransform.eulerAngles = new Vector3(0f, rotY, 0f);

            if (controller != null) controller.enabled = true;
        }

        // 2. Восстанавливаем состояние интерактивных объектов (двери, ключи, предметы)
        var objectsToLoad = new List<SaveableObject>(SaveableObject.AllSaveables);
        foreach (var obj in objectsToLoad)
        {
            if (obj != null)
                obj.LoadState();
        }

        Debug.Log("<color=cyan>[SaveManager] Состояние сцены и игрока успешно восстановлено!</color>");
    }

    // Вызывается перед стартом "Новой Игры"
    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>[SaveManager] Все сохранения полностью очищены!</color>");
    }
}