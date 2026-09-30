using UnityEngine;

[RequireComponent(typeof(Outline))]
public class OutlineController : MonoBehaviour
{
    [Header("Настройки мягкой обводки")]
    public Color highlightColor = new Color(1f, 1f, 1f, 0.7f);
    public float outlineWidth = 2f;

    // Флаг, разрешающий или запрещающий обводку
    public bool canBeHighlighted = true;

    private Outline outline;
    private Renderer objRenderer;

    void Awake()
    {
        outline = GetComponent<Outline>();
        objRenderer = GetComponent<Renderer>();

        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = highlightColor;
            outline.OutlineWidth = outlineWidth;
            outline.enabled = false;
        }
    }

    public void HighlightOn()
    {
        // Проверяем, существует ли объект, разрешена ли обводка и не выключен/уничтожен ли его MeshRenderer
        if (this == null || gameObject == null) return;
        if (objRenderer == null || !objRenderer.enabled) return;

        // Включаем обводку ТОЛЬКО если она разрешена и компонент существует
        if (outline != null && canBeHighlighted)
        {
            outline.enabled = true;
        }
    }

    public void HighlightOff()
    {
        if (this == null || gameObject == null) return;

        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    private void OnDisable()
    {
        // При выключении объекта сразу тушим обводку, чтобы не вызывать ошибок в QuickOutline
        HighlightOff();
    }
}