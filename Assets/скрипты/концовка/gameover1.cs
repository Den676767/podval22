using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Обязательно для загрузки сцен

public class FinalHorrorCutscene : MonoBehaviour
{
    [Header("1. Ссылки на Игрока и Камеру")]
    public Transform scenePlayerBody;         // Объект игрока
    public Camera scenePlayerCam;             // Камера игрока
    public MonoBehaviour sceneMovementScript; // Скрипт ходьбы
    public MonoBehaviour sceneMouseLookScript;// Скрипт обзора мышью

    [Header("2. Слендер и Звуки")]
    public GameObject sceneMonsterObj;        // Объект Слендера
    public Transform sceneMonsterFeet;        // Точка на НОГАХ Слендера
    public Transform sceneMonsterHead;        // Точка на ГОЛОВЕ Слендера
    public AudioSource sceneAudioSource;      // Источник звука
    public AudioClip sceneScreamerClip;       // Звук скримера

    [Header("3. Текст (Мысли) и Черный Экран")]
    public TextMeshProUGUI sceneSubText;      // UI текст мыслей
    [TextArea]
    public string sceneTextPhrase = "Тупик... Но я же только что оттуда пришёл...";
    public Image sceneFadeScreen;             // Чёрный экран для финала

    [Header("4. Переход в Финал")]
    [Tooltip("Название сцены, которая загрузится после черного экрана")]
    public string gameOverSceneName = "GameOverScene";
    [Tooltip("Пауза в полной темноте перед загрузкой сцены (в секундах)")]
    public float timeInDarkness = 1.5f;

    [Header("5. Кинематографичные Настройки")]
    public float initialPause = 0.8f;
    public float headShakeDuration = 1.2f;
    public float headShakeAngle = 12f;
    public float textDisplayDuration = 3.5f;

    [Header("6. Тайминги подъема взгляда и скримера")]
    [Tooltip("Длительность разворота тела лицом к ногам Слендера")]
    public float turnToFeetDuration = 2.0f;

    [Tooltip("Длительность медленного подъема взгляда от ног к голове")]
    public float tiltUpToHeadDuration = 3.0f;

    [Tooltip("Задержка перед скримером вверху")]
    public float delayBeforeScreamer = 0.05f;

    [Tooltip("Сколько длится резкий зум и скример перед темнотой")]
    public float zoomDuration = 1.5f;
    public float shakeIntensity = 0.35f;

    private bool isSceneActivated = false;
    private float defaultCamFov;

    private void Start()
    {
        if (sceneMonsterObj != null)
            sceneMonsterObj.SetActive(false);

        if (sceneFadeScreen != null)
        {
            Color c = sceneFadeScreen.color;
            c.a = 0f;
            sceneFadeScreen.color = c;
        }

        if (scenePlayerCam != null)
            defaultCamFov = scenePlayerCam.fieldOfView;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSceneActivated) return;

        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.GetComponent<Rigidbody>() != null)
        {
            isSceneActivated = true;
            StartCoroutine(PlayCinematicRoutine());
        }
    }

    private IEnumerator PlayCinematicRoutine()
    {
        // 0. БЛОКИРОВКА УПРАВЛЕНИЯ
        if (sceneMovementScript != null) sceneMovementScript.enabled = false;
        if (sceneMouseLookScript != null) sceneMouseLookScript.enabled = false;

        if (scenePlayerCam == null) scenePlayerCam = Camera.main;
        if (scenePlayerBody == null && scenePlayerCam != null)
            scenePlayerBody = scenePlayerCam.transform.parent != null ? scenePlayerCam.transform.parent : scenePlayerCam.transform;

        yield return new WaitForSeconds(initialPause);

        // 1. ПОКАЧИВАНИЕ ГОЛОВОЙ (Оглядываем тупик)
        Quaternion baseCamRot = scenePlayerCam.transform.localRotation;
        Quaternion leftRot = baseCamRot * Quaternion.Euler(0, -headShakeAngle, 0);
        Quaternion rightRot = baseCamRot * Quaternion.Euler(0, headShakeAngle, 0);

        yield return SmoothRotateCamera(baseCamRot, leftRot, headShakeDuration);
        yield return new WaitForSeconds(0.15f);
        yield return SmoothRotateCamera(leftRot, rightRot, headShakeDuration * 1.2f);
        yield return new WaitForSeconds(0.15f);
        yield return SmoothRotateCamera(rightRot, baseCamRot, headShakeDuration);

        yield return new WaitForSeconds(0.4f);

        // 2. МЫСЛИ ИГРОКА (Субтитры)
        if (sceneSubText != null)
        {
            sceneSubText.text = sceneTextPhrase;
            yield return FadeText(0f, 1f, 1.0f);
            yield return new WaitForSeconds(textDisplayDuration);
            yield return FadeText(1f, 0f, 0.8f);
        }

        yield return new WaitForSeconds(0.5f);

        // 3. СПАВН СЛЕНДЕРА И РАЗВОРОТ К ЕГО НОГАМ
        if (sceneMonsterObj != null)
            sceneMonsterObj.SetActive(true);

        Vector3 directionToMonster = (sceneMonsterObj.transform.position - scenePlayerBody.position);
        directionToMonster.y = 0;
        Quaternion targetBodyRot = Quaternion.LookRotation(directionToMonster);

        Quaternion startBodyRot = scenePlayerBody.rotation;

        float turnTimer = 0f;
        while (turnTimer < turnToFeetDuration)
        {
            turnTimer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, turnTimer / turnToFeetDuration);

            scenePlayerBody.rotation = Quaternion.Slerp(startBodyRot, targetBodyRot, t);

            Vector3 feetPos = sceneMonsterFeet != null ? sceneMonsterFeet.position : sceneMonsterObj.transform.position;
            Vector3 feetDirection = (feetPos - scenePlayerCam.transform.position).normalized;
            Quaternion targetCamFeetRot = Quaternion.LookRotation(feetDirection);

            scenePlayerCam.transform.rotation = Quaternion.Slerp(scenePlayerCam.transform.rotation, targetCamFeetRot, t);

            yield return null;
        }

        // 4. МЕДЛЕННЫЙ И НАГНЕТАЮЩИЙ ПОДЪЕМ ВЗГЛЯДА ОТ НОГ К ГОЛОВЕ
        if (sceneMonsterHead != null)
        {
            Vector3 feetPos = sceneMonsterFeet != null ? sceneMonsterFeet.position : sceneMonsterObj.transform.position;
            Quaternion startFeetCamRot = Quaternion.LookRotation((feetPos - scenePlayerCam.transform.position).normalized);

            float tiltTimer = 0f;
            while (tiltTimer < tiltUpToHeadDuration)
            {
                tiltTimer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, tiltTimer / tiltUpToHeadDuration);

                Vector3 headDirection = (sceneMonsterHead.position - scenePlayerCam.transform.position).normalized;
                Quaternion targetCamHeadRot = Quaternion.LookRotation(headDirection);

                scenePlayerCam.transform.rotation = Quaternion.Slerp(startFeetCamRot, targetCamHeadRot, t);

                yield return null;
            }
        }

        if (delayBeforeScreamer > 0f)
        {
            yield return new WaitForSeconds(delayBeforeScreamer);
        }

        // 5. РЕЗКИЙ СКРИМЕР
        if (sceneAudioSource != null && sceneScreamerClip != null)
        {
            sceneAudioSource.PlayOneShot(sceneScreamerClip);
        }

        float zoomTimer = 0f;
        Vector3 origCamPos = scenePlayerCam.transform.localPosition;

        while (zoomTimer < zoomDuration)
        {
            zoomTimer += Time.deltaTime;
            float progress = zoomTimer / zoomDuration;

            float fastZoomProgress = Mathf.Pow(progress, 0.5f);
            scenePlayerCam.fieldOfView = Mathf.Lerp(defaultCamFov, 10f, fastZoomProgress);

            if (sceneMonsterHead != null)
            {
                Vector3 headDir = (sceneMonsterHead.position - scenePlayerCam.transform.position).normalized;
                scenePlayerCam.transform.rotation = Quaternion.LookRotation(headDir);
            }

            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            scenePlayerCam.transform.localPosition = origCamPos + shakeOffset;

            yield return null;
        }

        // 6. УХОД В ТЕМНОТУ
        float fadeTimer = 0f;
        float fadeDuration = 0.3f;

        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            if (sceneFadeScreen != null)
            {
                Color c = sceneFadeScreen.color;
                c.a = Mathf.Lerp(0f, 1f, fadeTimer / fadeDuration);
                sceneFadeScreen.color = c;
            }
            yield return null;
        }

        if (sceneFadeScreen != null)
        {
            Color c = sceneFadeScreen.color;
            c.a = 1f;
            sceneFadeScreen.color = c;
        }

        // 7. ТИШИНА В ТЕМНОТЕ И ЗАГРУЗКА СЦЕНЫ
        if (timeInDarkness > 0f)
        {
            yield return new WaitForSeconds(timeInDarkness);
        }

        // Включаем видимость курсора мыши для меню Game Over
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Загрузка сцены проигрыша
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    private IEnumerator SmoothRotateCamera(Quaternion from, Quaternion to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            scenePlayerCam.transform.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        scenePlayerCam.transform.localRotation = to;
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
}