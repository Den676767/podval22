using System.Collections;
using UnityEngine;
using TMPro;

public class SlenderCutsceneTrigger : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    public Transform playerTransform;      // Сам игрок (Capsule / Player)
    public Camera mainCamera;              // Камера игрока
    public Transform slenderHead;          // Голова Слендера
    public Transform targetLamp;           // Лампочка, на которую разворачиваемся
    public GameObject slenderObject;       // Объект Слендера (исчезает)
    public LightGroup lightGroup;          // Контроллер света

    [Header("Скрипты для жесткой блокировки")]
    public MonoBehaviour[] scriptsToDisable;
    public CharacterController characterController;
    public Rigidbody playerRigidbody;

    [Header("Звуки и UI")]
    public AudioSource audioSource;
    public AudioClip horrorMusic;          // 1) Ужасающая музыка
    public AudioClip lightSwitchSound;     // 2) Звук выключения света
    public AudioClip vaseDropSound;        // 3) Звук падения вазы
    public TextMeshProUGUI subText;

    [Header("Параметры Зума (Zoom)")]
    public float zoomFOV = 35f;
    public float zoomDuration = 0.6f;

    [Header("Скорости и Задержки")]
    public float lookAngleThreshold = 40f;
    public float walkDistance = 1.0f;
    public float moveDuration = 1.0f;
    public float turnDuration = 0.5f;
    public float alignDuration = 0.4f;
    public float resetCamDuration = 0.5f;   // Плавность возврата наклона камеры

    [Header("Плавность Субтитров")]
    public float textFadeDuration = 1.0f;   // Время появления/исчезновения текста
    public float textDisplayDuration = 3.5f;// Сколько текст висит на экране

    private bool isTriggered = false;
    private bool isLockingInput = false;
    private float defaultFOV;

    private bool isHoldingCameraX = false;
    private Quaternion lockedCamRot;
    private float targetCamX;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) defaultFOV = mainCamera.fieldOfView;

        // Делаем текст изначально прозрачным
        if (subText != null)
        {
            Color c = subText.color;
            c.a = 0f;
            subText.color = c;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player") || other.transform == playerTransform)
        {
            Vector3 dirToSlender = (slenderHead.position - mainCamera.transform.position).normalized;
            float angle = Vector3.Angle(mainCamera.transform.forward, dirToSlender);

            if (angle <= lookAngleThreshold)
            {
                isTriggered = true;
                StartCoroutine(RunSlenderCutscene());
            }
        }
    }

    void Update()
    {
        if (isLockingInput)
        {
            Input.ResetInputAxes();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void LateUpdate()
    {
        if (isHoldingCameraX && mainCamera != null)
        {
            mainCamera.transform.localRotation = lockedCamRot;
        }
    }

    private IEnumerator RunSlenderCutscene()
    {
        // 0. Отключаем управление
        SetPlayerControl(false);

        // 1) ВКЛЮЧАЕМ МУЗЫКУ
        if (audioSource != null && horrorMusic != null)
        {
            audioSource.clip = horrorMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 1. НАВОДИМ ВЗГЛЯД
        Vector3 dirToHead = (slenderHead.position - mainCamera.transform.position).normalized;
        Quaternion targetWorldRot = Quaternion.LookRotation(dirToHead);

        Quaternion targetPlayerRot = Quaternion.Euler(0, targetWorldRot.eulerAngles.y, 0);

        targetCamX = targetWorldRot.eulerAngles.x;
        if (targetCamX > 180f) targetCamX -= 360f;

        lockedCamRot = Quaternion.Euler(targetCamX, 0f, 0f);

        Quaternion startPlayerRot = playerTransform.rotation;
        Quaternion startCamLocalRot = mainCamera.transform.localRotation;

        float alignElapsed = 0f;
        while (alignElapsed < alignDuration)
        {
            alignElapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, alignElapsed / alignDuration);

            playerTransform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, percent);
            mainCamera.transform.localRotation = Quaternion.Slerp(startCamLocalRot, lockedCamRot, percent);
            yield return null;
        }

        playerTransform.rotation = targetPlayerRot;
        mainCamera.transform.localRotation = lockedCamRot;

        isHoldingCameraX = true;

        // 2. ИДЕМ ВПЕРЕД К СЛЕНДЕРУ + ЗУМ
        Vector3 startPos = playerTransform.position;
        Vector3 targetPos = startPos + playerTransform.forward * walkDistance;
        targetPos.y = startPos.y;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);

            playerTransform.position = Vector3.Lerp(startPos, targetPos, percent);
            mainCamera.fieldOfView = Mathf.Lerp(defaultFOV, zoomFOV, percent);

            yield return null;
        }

        playerTransform.position = targetPos;
        mainCamera.fieldOfView = zoomFOV;

        yield return new WaitForSeconds(0.2f);

        // 3. ВЫКЛЮЧЕНИЕ СВЕТА + ЗВУК
        if (lightGroup != null) lightGroup.TurnOffAllLights();
        if (audioSource != null && lightSwitchSound != null)
        {
            audioSource.PlayOneShot(lightSwitchSound);
        }

        float unzoomElapsed = 0f;
        while (unzoomElapsed < zoomDuration)
        {
            unzoomElapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, unzoomElapsed / zoomDuration);
            mainCamera.fieldOfView = Mathf.Lerp(zoomFOV, defaultFOV, percent);

            yield return null;
        }
        mainCamera.fieldOfView = defaultFOV;

        // 4. РАЗВОРОТ НА ЛАМПОЧКУ
        Quaternion rotHeadState = playerTransform.rotation;
        Quaternion rotToLamp = rotHeadState;

        if (targetLamp != null)
        {
            Vector3 dirToLamp = targetLamp.position - playerTransform.position;
            dirToLamp.y = 0;
            if (dirToLamp != Vector3.zero)
            {
                rotToLamp = Quaternion.LookRotation(dirToLamp);
            }
        }
        else
        {
            rotToLamp = rotHeadState * Quaternion.Euler(0, 180f, 0);
        }

        yield return StartCoroutine(SmoothRotatePlayerY(rotHeadState, rotToLamp, turnDuration));

        // 5. СЛЕНДЕР ПРОПАДАЕТ + ВАЗА
        if (slenderObject != null) slenderObject.SetActive(false);
        if (audioSource != null && vaseDropSound != null)
        {
            audioSource.PlayOneShot(vaseDropSound);
        }

        yield return new WaitForSeconds(0.4f);

        // 6. РАЗВОРОТ ОБРАТНО
        yield return StartCoroutine(SmoothRotatePlayerY(rotToLamp, rotHeadState, turnDuration));

        // ОСТАНАВЛИВАЕМ МУЗЫКУ
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // ПЛАВНЫЙ ВОЗВРАТ НАКЛОНА КАМЕРЫ В ГОРИЗОНТ
        float resetElapsed = 0f;
        Quaternion startResetRot = lockedCamRot;
        Quaternion targetResetRot = Quaternion.identity;

        while (resetElapsed < resetCamDuration)
        {
            resetElapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, resetElapsed / resetCamDuration);

            lockedCamRot = Quaternion.Slerp(startResetRot, targetResetRot, percent);
            yield return null;
        }

        isHoldingCameraX = false;
        mainCamera.transform.localRotation = targetResetRot;

        // 7. ВОЗВРАТ УПРАВЛЕНИЯ
        SetPlayerControl(true);

        yield return new WaitForSeconds(0.1f);

        // ПЛАВНОЕ ПОЯВЛЕНИЕ И ИСЧЕЗНОВЕНИЕ ТЕКСТА
        if (subText != null)
        {
            subText.text = "странно .. показалось что ли? ... надо проверить , что там упало";

            // Плавное проявление (Fade In)
            yield return StartCoroutine(FadeTextAlpha(0f, 1f, textFadeDuration));

            // Пауза чтения
            yield return new WaitForSeconds(textDisplayDuration);

            // Плавное затухание (Fade Out)
            yield return StartCoroutine(FadeTextAlpha(1f, 0f, textFadeDuration));

            subText.text = "";
        }
    }

    private IEnumerator FadeTextAlpha(float startAlpha, float targetAlpha, float duration)
    {
        if (subText == null) yield break;

        Color c = subText.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            subText.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        subText.color = c;
    }

    private IEnumerator SmoothRotatePlayerY(Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            playerTransform.rotation = Quaternion.Slerp(from, to, percent);

            yield return null;
        }
        playerTransform.rotation = to;
    }

    private void SetPlayerControl(bool enabledState)
    {
        isLockingInput = !enabledState;

        if (scriptsToDisable != null)
        {
            foreach (MonoBehaviour script in scriptsToDisable)
            {
                if (script != null)
                {
                    script.enabled = enabledState;

                    if (enabledState)
                    {
                        script.SendMessage("SetRotationX", 0f, SendMessageOptions.DontRequireReceiver);
                        script.SendMessage("SetXRotation", 0f, SendMessageOptions.DontRequireReceiver);
                        script.SendMessage("SetRotationY", playerTransform.eulerAngles.y, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        if (characterController != null) characterController.enabled = enabledState;
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = !enabledState;
            playerRigidbody.velocity = Vector3.zero;
        }
    }
}