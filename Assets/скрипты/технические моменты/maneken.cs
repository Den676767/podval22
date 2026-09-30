using UnityEngine;

public class MannequinHead : MonoBehaviour
{
    [Header("Цель наблюдения")]
    public Transform playerTransform;      // Ссылка на трансформ игрока (или его Камеру)

    [Header("Настройки поворота")]
    public float turnSpeed = 3f;           // Скорость поворота головы
    public bool lockVertical = true;       // Поворачивать только по горизонтали (Y-ось)
    public bool isFacingReverse = false;   // Включи, если 3D-модель смотрит назад своей 3D-осью Z

    void Start()
    {
        // Если игрок не перетащен вручную, ищем Камеру игрока по тегу MainCamera
        if (playerTransform == null)
        {
            if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("[MannequinHead] Камера / Игрок не найден на сцене!");
            }
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Вычисляем направление от головы до игрока
        Vector3 direction = playerTransform.position - transform.position;

        // Если включена блокировка вертикали — игнорируем разницу в высоте (ось Y)
        if (lockVertical)
        {
            direction.y = 0f;
        }

        // Если направление валидное (игрок не стоит ровно внутри головы)
        if (direction.sqrMagnitude > 0.001f)
        {
            // Корректируем ось, если модель задепортирована смотрит затылком
            Vector3 lookDirection = isFacingReverse ? -direction : direction;

            // Расчитываем целевой поворот
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Плавно поворачиваем голову в сторону цели
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }
}