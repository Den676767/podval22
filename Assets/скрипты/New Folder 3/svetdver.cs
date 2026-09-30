using System.Collections;
using UnityEngine;

public class DoorFlickerLightTrigger : MonoBehaviour
{
    [Header("Ссылка на скрипт щитка")]
    public FuseBoxSequential fuseBox;      // Перетащи сюда объект со скриптом FuseBoxSequential

    [Header("Источник света для подсветки двери")]
    public Light doorLight;

    [Header("Настройки мигания")]
    public int flickerCount = 3;           // Сколько раз мигнет перед выключением
    public float flickerSpeed = 0.07f;     // Скорость мигания

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // Если рычаг щитка еще НЕ поднят — триггер игнорируется
        if (fuseBox != null && !fuseBox.isLeverPulled)
        {
            return;
        }

        // Проверяем, что зашел именно игрок
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            triggered = true;
            if (doorLight != null)
            {
                StartCoroutine(FlickerAndTurnOff());
            }
        }
    }

    private IEnumerator FlickerAndTurnOff()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            doorLight.enabled = false;
            yield return new WaitForSeconds(flickerSpeed);

            doorLight.enabled = true;
            yield return new WaitForSeconds(flickerSpeed);
        }

        doorLight.enabled = false;
    }
}