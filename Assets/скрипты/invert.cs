using System.Collections.Generic;
using UnityEngine;
using TMPro; // Потребуется для поля UI текста

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public string itemID;             // Уникальное имя (ID) предмета
        public GameObject handModel;      // 3D-модель предмета в руках
    }

    [Header("Слоты 1-9 (Максимум 9)")]
    public InventorySlot[] slots = new InventorySlot[9];

    [Header("Счётчик инструментов (UI)")]
    public TextMeshProUGUI toolsText;     // Перетащи сюда свой текстовый объект
    public int totalToolsNeeded = 4;      // Всего необходимо собрать
    public string toolPrefix = "Tool_";   // Префикс для ID (например: Tool_Key, Tool_Hammer)

    private int currentToolsCount = 0;
    private int currentSlotIndex = -1;

    void Start()
    {
        UpdateToolsUI();
    }

    void Update()
    {
        // Отслеживаем нажатия клавиш 1-9
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    public bool AddItem(string id, GameObject handModelPrefab)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (string.IsNullOrEmpty(slots[i].itemID))
            {
                slots[i].itemID = id;
                slots[i].handModel = handModelPrefab;

                // Увеличиваем цифру ТОЛЬКО если у предмета нужный префикс (например, Tool_Key)
                if (!string.IsNullOrEmpty(id) && id.StartsWith(toolPrefix))
                {
                    currentToolsCount++;
                    UpdateToolsUI();
                }

                Debug.Log($"[Инвентарь] Предмет {id} помещен в слот {i + 1}");

                if (currentSlotIndex == -1)
                {
                    SelectSlot(i);
                }
                return true;
            }
        }

        Debug.LogWarning("[Инвентарь] Нет свободных слотов!");
        return false;
    }

    private void UpdateToolsUI()
    {
        if (toolsText != null)
        {
            toolsText.text = $"Инструментов собрано: {currentToolsCount} / {totalToolsNeeded}";
        }
    }

    public void SelectSlot(int index)
    {
        if (currentSlotIndex == index)
        {
            if (slots[index].handModel != null)
                slots[index].handModel.SetActive(false);

            currentSlotIndex = -1;
            return;
        }

        if (currentSlotIndex != -1 && slots[currentSlotIndex].handModel != null)
        {
            slots[currentSlotIndex].handModel.SetActive(false);
        }

        currentSlotIndex = index;

        if (slots[currentSlotIndex].handModel != null)
        {
            slots[currentSlotIndex].handModel.SetActive(true);
            Debug.Log($"[Инвентарь] Выбран предмет из слота {index + 1}: {slots[index].itemID}");
        }
    }

    public bool IsHoldingItem(string id)
    {
        if (currentSlotIndex == -1) return false;
        return slots[currentSlotIndex].itemID == id;
    }
}