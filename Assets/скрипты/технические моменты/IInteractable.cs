using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Дистанции")]
    public float highlightDistance = 7f;   // Подсветка включается издалека
    public float interactDistance = 2.5f;  // Клик Е / ЛКМ работает только вблизи

    private OutlineController currentOutline;

    void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        OutlineController foundOutline = null;

        // Стреляем дальше для подсветки
        if (Physics.Raycast(ray, out hit, highlightDistance))
        {
            foundOutline = hit.collider.GetComponentInParent<OutlineController>();
        }

        // Переключение обводки
        if (currentOutline != foundOutline)
        {
            if (currentOutline != null)
                currentOutline.HighlightOff();

            currentOutline = foundOutline;

            if (currentOutline != null)
                currentOutline.HighlightOn();
        }

        // Взаимодействие (только если подсвечен И подошёл вплотную)
        if (currentOutline != null && hit.distance <= interactDistance)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                // 1. Граммофон
                Gramophone gramophone = currentOutline.GetComponent<Gramophone>();
                if (gramophone != null)
                {
                    gramophone.Interact();
                }

                // 2. Картина
                PaintingTrigger painting = currentOutline.GetComponent<PaintingTrigger>();
                if (painting != null)
                {
                    painting.Interact();
                }

                // 3. УНИВЕРСАЛЬНЫЙ ПОДБОР ПРЕДМЕТА
                PickupItem pickup = currentOutline.GetComponent<PickupItem>();
                if (pickup != null)
                {
                    pickup.Interact();
                }
            }
        }
    }
}