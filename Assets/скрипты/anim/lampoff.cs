using System.Collections;
using UnityEngine;

public class LightGroup : MonoBehaviour
{
    [Header("Все источники света в комнате")]
    public Light[] lights;

    [Header("Настройки мигания")]
    public int flickerCount = 4;           // Сколько раз мигнет перед выключением
    public float flickerSpeed = 0.08f;     // Скорость мигания (чем меньше, тем быстрее)

    // Метод для выключения всех источников света (теперь с миганием)
    public void TurnOffAllLights()
    {
        StartCoroutine(FlickerAndTurnOff());
    }

    private IEnumerator FlickerAndTurnOff()
    {
        // Быстро мигаем заданное количество раз
        for (int i = 0; i < flickerCount; i++)
        {
            SetLightsState(false);
            yield return new WaitForSeconds(flickerSpeed);

            SetLightsState(true);
            yield return new WaitForSeconds(flickerSpeed);
        }

        // В конце полностью гасим весь свет
        SetLightsState(false);
    }

    // Метод для включения обратно (если нужно по сюжету)
    public void TurnOnAllLights()
    {
        StopAllCoroutines(); // Останавливаем мигание, если оно еще шло
        SetLightsState(true);
    }

    // Вспомогательный метод для переключения всех ламп
    private void SetLightsState(bool state)
    {
        foreach (Light l in lights)
        {
            if (l != null)
            {
                l.enabled = state;
            }
        }
    }
}