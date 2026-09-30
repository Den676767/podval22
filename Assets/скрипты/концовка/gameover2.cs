using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EscapeHorrorCutscene : MonoBehaviour
{
    [Header("1. Ссылки на Игрока и Камеру")]
    public Transform scenePlayerBody;         // Объект игрока
    public Camera scenePlayerCam;             // Камера игрока
    public MonoBehaviour sceneMovementScript; // Скрипт ходьбы/бега
    public MonoBehaviour sceneMouseLookScript;// Скрипт обзора мышью

    [Header("2. Слендер, Голова и Анимация")]
    public GameObject sceneMonsterObj;        // Объект Слендера
    public Transform monsterHeadTarget;       // Голова Слендера (для плавного наведения камеры)
    public Animator monsterAnimator;          // Компонент Animator у Слендера
    public string animationTriggerName = "Appear"; // Название триггера анимации
    public Transform cornerSpawnPoint;        // Точка спавна Слендера

    [Header("3. Звуки Хоррора и Зум")]
    public AudioSource scareAudioSource;      // Источник звука для скримера
    public AudioClip scareAudioClip;          // Звук испуга
    public AudioSource chaseMusicAudioSource; // AudioSource для фоновой музыки
    public AudioClip chaseMusicClip;          // Фоновая тревожная музыка
    public float zoomedFOV = 25f;             // Зум на голову Слендера

    [Header("4. Текст «БЕГИ!» и UI Subtitles")]
    public TextMeshProUGUI runWarningText;    // Текст "БЕГИ!"
    public float runTextBlinkSpeed = 3.0f;    // Скорость мигания надписи "БЕГИ!"
    public TextMeshProUGUI sceneSubText;      // Subtitles UI (мысли игрока)
    [TextArea]
    public string escapeSubTextPhrase = "Еле успел... Ещё бы секунда — и мне конец...";
    public Image sceneFadeScreen;             // Чёрный экран затухания
    public string winSceneName = "WinScene";   // Название сцены победы

    [Header("5. Спасительная Дверь и Триггер")]
    public Transform escapeDoorTransform;     // Объект спасительной двери
    public Vector3 doorOpenEulerAngles = new Vector3(0, 90f, 0); // Градусы открытия двери
    public float doorCloseSpeed = 2.0f;       // Медленное плавное закрытие двери
    public EscapeZoneTrigger escapeZoneTrigger; // Триггер за дверью
    public AudioSource doorAudioSource;       // Источник звука на двери
    public AudioClip doorCloseClip;           // Звук закрытия двери
    public AudioClip doorKnockClip;           // Звук ударов/стука в дверь

    [Header("6. Замедление и Тайминги")]
    public float turnDuration = 2.5f;         // Длительность разворота в секундах (чем больше, тем медленнее)
    public float lookAtMonsterDuration = 3.5f; // Длительное замирание при взгляде на Слендера
    public float textDisplayDuration = 4.0f;   // Длительность субтитров
    public float fadeToBlackDuration = 3.5f;   // Медленное затухание экрана

    [Header("7. Эффект одышки (Покачивание камеры от усталости)")]
    public float swayIntensity = 0.04f;       // Амплитуда покачивания камеры
    public float swaySpeed = 2.5f;            // Скорость "дыхания"

    private bool isCutsceneStarted = false;
    private bool isPlayerEscaped = false;
    private bool isRunTextActive = false;
    private Quaternion closedDoorRotation;
    private Quaternion openDoorRotation;
    private float defaultFOV;

    private void Start()
    {
        if (sceneMonsterObj != null)
            sceneMonsterObj.SetActive(false);

        if (runWarningText != null)
            runWarningText.gameObject.SetActive(false);

        if (sceneFadeScreen != null)
        {
            Color c = sceneFadeScreen.color;
            c.a = 0f;
            sceneFadeScreen.color = c;
        }

        if (escapeDoorTransform != null)
        {
            closedDoorRotation = escapeDoorTransform.localRotation;
            openDoorRotation = closedDoorRotation * Quaternion.Euler(doorOpenEulerAngles);
        }

        if (scenePlayerCam == null)
            scenePlayerCam = Camera.main;

        if (scenePlayerCam != null)
            defaultFOV = scenePlayerCam.fieldOfView;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCutsceneStarted) return;

        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            isCutsceneStarted = true;
            StartCoroutine(PlaySimpleLookRoutine());
        }
    }

    private void Update()
    {
        if (isCutsceneStarted && !isPlayerEscaped && escapeZoneTrigger != null)
        {
            if (escapeZoneTrigger.isPlayerInside)
            {
                isPlayerEscaped = true;
                StartCoroutine(PlayEscapeEnding());
            }
        }
    }

    private IEnumerator PlaySimpleLookRoutine()
    {
        // 1. Полностью отключаем скрипты управления движения и обзора
        if (sceneMovementScript != null) sceneMovementScript.enabled = false;
        if (sceneMouseLookScript != null) sceneMouseLookScript.enabled = false;

        if (scenePlayerCam == null) scenePlayerCam = Camera.main;
        if (scenePlayerBody == null && scenePlayerCam != null)
            scenePlayerBody = scenePlayerCam.transform.parent != null ? scenePlayerCam.transform.parent : scenePlayerCam.transform;

        // Появление Слендера
        if (sceneMonsterObj != null)
        {
            if (cornerSpawnPoint != null)
                sceneMonsterObj.transform.position = cornerSpawnPoint.position;

            sceneMonsterObj.SetActive(true);
        }

        // Звуки
        if (scareAudioSource != null && scareAudioClip != null)
            scareAudioSource.PlayOneShot(scareAudioClip);

        if (chaseMusicAudioSource != null && chaseMusicClip != null)
        {
            chaseMusicAudioSource.clip = chaseMusicClip;
            chaseMusicAudioSource.loop = true;
            chaseMusicAudioSource.Play();
        }

        // Фиксируем стартовые повороты
        Quaternion startBodyRot = scenePlayerBody.rotation;
        Quaternion startCamLocalRot = scenePlayerCam.transform.localRotation;

        Vector3 targetPos = (monsterHeadTarget != null) ? monsterHeadTarget.position : sceneMonsterObj.transform.position;

        // Поворот тела игрока
        Vector3 dirToMonsterBody = (targetPos - scenePlayerBody.position).normalized;
        dirToMonsterBody.y = 0;
        Quaternion targetBodyRot = Quaternion.LookRotation(dirToMonsterBody);

        // Поворот камеры прямо на голову
        Vector3 dirToHead = (targetPos - scenePlayerCam.transform.position).normalized;
        Quaternion targetCamWorldRot = Quaternion.LookRotation(dirToHead);

        // 2. ОЧЕНЬ МЕДЛЕННЫЙ И ПЛАВНЫЙ РАЗВОРОТ + ЗУМ
        float elapsed = 0f;
        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / turnDuration);

            // Плавный поворот тела и камеры
            scenePlayerBody.rotation = Quaternion.Slerp(startBodyRot, targetBodyRot, t);
            scenePlayerCam.transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(scenePlayerCam.transform.forward), targetCamWorldRot, t);

            // Плавный зум (изменение FOV)
            if (scenePlayerCam != null)
                scenePlayerCam.fieldOfView = Mathf.Lerp(defaultFOV, zoomedFOV, t);

            yield return null;
        }

        // Фиксируем точное направление и зум
        scenePlayerBody.rotation = targetBodyRot;
        scenePlayerCam.transform.rotation = targetCamWorldRot;
        scenePlayerCam.fieldOfView = zoomedFOV;

        // Анимация Слендера
        if (monsterAnimator != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            monsterAnimator.SetTrigger(animationTriggerName);
        }

        // Замираем в ужасе
        yield return new WaitForSeconds(lookAtMonsterDuration);

        // 3. МЕДЛЕННЫЙ РАЗВОРОТ ОБРАТНО К ДВЕРИ + СБРОС ЗУМА + ОТКРЫТИЕ ДВЕРИ
        elapsed = 0f;
        Quaternion currentBodyRot = scenePlayerBody.rotation;
        Quaternion currentCamRot = scenePlayerCam.transform.rotation;
        Quaternion targetCamBackWorldRot = startBodyRot * startCamLocalRot;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / turnDuration);

            scenePlayerBody.rotation = Quaternion.Slerp(currentBodyRot, startBodyRot, t);
            scenePlayerCam.transform.rotation = Quaternion.Slerp(currentCamRot, targetCamBackWorldRot, t);

            if (scenePlayerCam != null)
                scenePlayerCam.fieldOfView = Mathf.Lerp(zoomedFOV, defaultFOV, t);

            if (escapeDoorTransform != null)
                escapeDoorTransform.localRotation = Quaternion.Slerp(closedDoorRotation, openDoorRotation, t);

            yield return null;
        }

        // Возвращаем все в норму
        if (escapeDoorTransform != null) escapeDoorTransform.localRotation = openDoorRotation;
        if (scenePlayerCam != null)
        {
            scenePlayerCam.fieldOfView = defaultFOV;
            scenePlayerCam.transform.localRotation = startCamLocalRot;
        }

        // 4. Показываем надпись "БЕГИ!" и включаем только ходьбу
        StartCoroutine(BlinkRunText());

        if (sceneMovementScript != null)
            sceneMovementScript.enabled = true;
    }

    private IEnumerator BlinkRunText()
    {
        if (runWarningText == null) yield break;

        runWarningText.gameObject.SetActive(true);
        isRunTextActive = true;
        Color c = runWarningText.color;

        while (isRunTextActive && !isPlayerEscaped)
        {
            c.a = (Mathf.Sin(Time.time * runTextBlinkSpeed) + 1f) / 2f;
            runWarningText.color = c;
            yield return null;
        }

        runWarningText.gameObject.SetActive(false);
    }

    private IEnumerator PlayEscapeEnding()
    {
        isRunTextActive = false;
        if (runWarningText != null) runWarningText.gameObject.SetActive(false);

        // Блокируем ходьбу
        if (sceneMovementScript != null) sceneMovementScript.enabled = false;
        if (sceneMouseLookScript != null) sceneMouseLookScript.enabled = false;

        // Запускаем эффект одышки/покачивания
        Coroutine swayCoroutine = StartCoroutine(HeavyBreathingSway());

        // 1. Медленное закрытие двери
        if (escapeDoorTransform != null)
        {
            float doorTimer = 0f;
            Quaternion currentDoorRot = escapeDoorTransform.localRotation;
            while (doorTimer < 1f)
            {
                doorTimer += Time.deltaTime * doorCloseSpeed;
                escapeDoorTransform.localRotation = Quaternion.Slerp(currentDoorRot, closedDoorRotation, Mathf.SmoothStep(0f, 1f, doorTimer));
                yield return null;
            }
            escapeDoorTransform.localRotation = closedDoorRotation;
        }

        if (doorAudioSource != null && doorCloseClip != null)
        {
            doorAudioSource.PlayOneShot(doorCloseClip);
        }

        yield return new WaitForSeconds(0.5f);

        // Плавный затухающий уход музыки
        if (chaseMusicAudioSource != null && chaseMusicAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(chaseMusicAudioSource, 2.5f));
        }

        if (sceneMonsterObj != null)
            sceneMonsterObj.SetActive(false);

        // Стуки в дверь
        if (doorAudioSource != null && doorKnockClip != null)
        {
            doorAudioSource.PlayOneShot(doorKnockClip);
        }

        // 2. Субтитры
        if (sceneSubText != null)
        {
            sceneSubText.text = escapeSubTextPhrase;
            yield return FadeText(0f, 1f, 1.2f);
            yield return new WaitForSeconds(textDisplayDuration);
        }
        else
        {
            yield return new WaitForSeconds(2.5f);
        }

        // 3. Медленное кинематографичное затухание
        float fadeTimer = 0f;
        while (fadeTimer < fadeToBlackDuration)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / fadeToBlackDuration;

            if (sceneFadeScreen != null)
            {
                Color fc = sceneFadeScreen.color;
                fc.a = Mathf.Lerp(0f, 1f, progress);
                sceneFadeScreen.color = fc;
            }

            if (sceneSubText != null)
            {
                Color tc = sceneSubText.color;
                tc.a = Mathf.Lerp(1f, 0f, progress);
                sceneSubText.color = tc;
            }

            yield return null;
        }

        if (swayCoroutine != null) StopCoroutine(swayCoroutine);

        yield return new WaitForSeconds(0.8f);

        // Переход на сцену победы
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!string.IsNullOrEmpty(winSceneName))
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    private IEnumerator HeavyBreathingSway()
    {
        if (scenePlayerCam == null) yield break;

        Vector3 startCamPos = scenePlayerCam.transform.localPosition;
        float timer = 0f;

        while (isPlayerEscaped)
        {
            timer += Time.deltaTime * swaySpeed;
            float offsetY = Mathf.Sin(timer) * swayIntensity;
            float offsetX = Mathf.Cos(timer * 0.5f) * (swayIntensity * 0.5f);

            scenePlayerCam.transform.localPosition = startCamPos + new Vector3(offsetX, offsetY, 0);
            yield return null;
        }

        scenePlayerCam.transform.localPosition = startCamPos;
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        if (sceneSubText == null) yield break;
        Color c = sceneSubText.color;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            sceneSubText.color = c;
            yield return null;
        }
        c.a = endAlpha;
        sceneSubText.color = c;
    }

    private IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}