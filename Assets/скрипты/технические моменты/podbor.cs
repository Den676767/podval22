using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    public string itemID = "Item_Name";    // Уникальное название (RustKey, Flashlight, Note и т.д.)
    public GameObject handModel;           // Ссылка на модель этого предмета В РУКАХ (под Камерой)
    public AudioClip pickupSound;         // Звук подбора предмета

    public void Interact()
    {
        // Ищем инвентарь на игроке / камере
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();

        if (inventory != null)
        {
            // Пытаемся положить предмет в свободный слот
            bool added = inventory.AddItem(itemID, handModel);

            if (added)
            {
                // Звук подбора
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.9f);
                }

                // Тушим обводку перед уничтожением
                OutlineController outlineCtrl = GetComponent<OutlineController>();
                if (outlineCtrl != null)
                {
                    outlineCtrl.HighlightOff();
                }

                // Удаляем предмет со сцены
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("[PickupItem] Не найден скрипт PlayerInventory на сцене!");
        }
    }
}