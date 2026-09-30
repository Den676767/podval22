using UnityEngine;

public class BrightnessHighlight : MonoBehaviour
{
    private Renderer objRenderer;
    private Color originalColor;
    public Color highlightColor = Color.yellow; // Цвет подсветки

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            // Запоминаем исходный цвет дефолтного материала
            originalColor = objRenderer.material.color;
        }
    }

    public void HighlightOn()
    {
        if (objRenderer != null)
        {
            // Меняем цвет напрямую у материала объекта
            objRenderer.material.color = highlightColor;
        }
    }

    public void HighlightOff()
    {
        if (objRenderer != null)
        {
            // Возвращаем оригинальный цвет
            objRenderer.material.color = originalColor;
        }
    }
}