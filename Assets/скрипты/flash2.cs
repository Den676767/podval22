using UnityEngine;

public class DynamicFlashlight : MonoBehaviour
{
    private Light flashlight;
    public float maxDistance = 10f;
    public float minAngle = 30f; // Угол вдалеке (узкий луч)
    public float maxAngle = 60f; // Угол вблизи (широкое пятно)

    void Start()
    {
        flashlight = GetComponent<Light>();
    }

    void Update()
    {
        RaycastHit hit;
        // Пускаем луч вперед от фонарика
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            // Считаем коэффициент расстояния (от 0 до 1)
            float distanceRatio = hit.distance / maxDistance;

            // Плавной интерполяцией меняем угол в зависимости от расстояния до стены
            flashlight.spotAngle = Mathf.Lerp(minAngle, maxAngle, 1f - distanceRatio);
        }
        else
        {
            // Если перед нами пустота (далеко), держим узкий луч
            flashlight.spotAngle = minAngle;
        }
    }
}