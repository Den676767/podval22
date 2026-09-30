using UnityEngine;

public class SlenderTriggerDeactivator : MonoBehaviour
{
    public enum Mode { Disable, Destroy }

    [Header("Настройки")]
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Mode action = Mode.Disable;

    private void OnTriggerEnter(Collider other)
    {
        if (targetObject != null)
        {
            if (action == Mode.Disable)
            {
                targetObject.SetActive(false);
            }
            else if (action == Mode.Destroy)
            {
                Destroy(targetObject);
            }
        }

        // Выключаем сам триггер, чтобы больше не срабатывал
        gameObject.SetActive(false);
    }
}