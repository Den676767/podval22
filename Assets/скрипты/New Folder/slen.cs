using UnityEngine;

public class SlenderSequenceManager : MonoBehaviour
{
    [Header("Ссылки")]
    public Camera mainCamera;

    [Header("Массив Слендеров (в порядке появления)")]
    public GameObject[] slenderChain;

    [Header("Настройки")]
    public bool triggerOnStart = true;
    [Range(10f, 80f)]
    public float viewAngle = 50f; // Максимальный угол обзора (в градусах от центра экрана)

    private int currentIndex = -1;
    private bool isWaitingForTurnAway = false;
    private bool sequenceFinished = false;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Выключаем всех Слендеров на старте
        for (int i = 0; i < slenderChain.Length; i++)
        {
            if (slenderChain[i] != null)
                slenderChain[i].SetActive(false);
        }

        if (triggerOnStart)
        {
            StartSequence();
        }
    }

    public void StartSequence()
    {
        if (slenderChain == null || slenderChain.Length == 0) return;

        currentIndex = 0;
        if (slenderChain[currentIndex] != null)
        {
            slenderChain[currentIndex].SetActive(true);
        }
        isWaitingForTurnAway = false;
    }

    private void Update()
    {
        if (sequenceFinished || currentIndex < 0 || currentIndex >= slenderChain.Length) return;

        GameObject currentSlender = slenderChain[currentIndex];
        if (currentSlender == null) return;

        bool isLookingAtCurrent = IsLookingAtTarget(currentSlender);

        // Фаза 1: Игрок посмотрел на текущего Слендера
        if (isLookingAtCurrent && !isWaitingForTurnAway)
        {
            isWaitingForTurnAway = true;
            Debug.Log($"[SlenderSequence] Посмотрели на Слендера #{currentIndex}");
        }

        // Фаза 2: Игрок отвернулся от текущего Слендера
        if (!isLookingAtCurrent && isWaitingForTurnAway)
        {
            // Проверяем, есть ли следующий Слендер в списке
            if (currentIndex < slenderChain.Length - 1)
            {
                Debug.Log($"[SlenderSequence] Отвернулись от Слендера #{currentIndex}. Переключаем на #{currentIndex + 1}...");

                // Отключаем ТЕКУЩЕГО только если есть следующий
                currentSlender.SetActive(false);
                currentIndex++;

                if (slenderChain[currentIndex] != null)
                {
                    slenderChain[currentIndex].SetActive(true);
                }
                isWaitingForTurnAway = false;
            }
            else
            {
                // Это был последний Слендер — не выключаем его, а завершаем логику
                sequenceFinished = true;
                Debug.Log("[SlenderSequence] Цепочка завершена! Последний Слендер остается на сцене.");
            }
        }
    }

    // Упрощенная и точная проверка: смотрит ли камера на объект
    private bool IsLookingAtTarget(GameObject target)
    {
        Vector3 targetPos = target.transform.position + Vector3.up * 1.5f; // Точка на уровне головы/груди
        Vector3 dirToTarget = (targetPos - mainCamera.transform.position).normalized;

        // Проверяем угол между взглядом камеры и направлением на Слендера
        float angle = Vector3.Angle(mainCamera.transform.forward, dirToTarget);

        if (angle < viewAngle)
        {
            // Рейкаст: проверяем, нет ли стены между игроком и Слендером
            if (Physics.Raycast(mainCamera.transform.position, dirToTarget, out RaycastHit hit, 50f))
            {
                // Если луч попал в Слендера или его дочерние объекты — мы его видим
                if (hit.transform.IsChildOf(target.transform) || hit.transform == target.transform)
                {
                    return true;
                }
            }
            else
            {
                // Если Raycast не попал ни во что (например, нет коллайдера), но угол совпал — всё равно считаем видимым
                return true;
            }
        }

        return false;
    }
}