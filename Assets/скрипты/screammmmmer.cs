using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class screammmmmer : MonoBehaviour
{
    [Header("Компоненты Игрока")]
    [Tooltip("Скрипт движения игрока")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Tooltip("Компонент CharacterController игрока")]
    [SerializeField] private CharacterController playerController;

    [Header("Объекты")]
    [Tooltip("Основная камера игрока")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Голова Слендера (объект head)")]
    [SerializeField] private Transform monsterHead;

    [Tooltip("Аниматор Слендера (перетащи сюда Слендера из иерархии)")]
    [SerializeField] private Animator monsterAnimator;

    [Header("Настройки Зума")]
    [SerializeField] private float zoomDuration = 0.4f;
    [SerializeField] private float holdDuration = 0.8f;
    [SerializeField] private float targetFOV = 18f;

    [Header("Настройки Тряски")]
    [SerializeField] private float shakeIntensity = 0.12f;

    [Header("Эффект Затемнения")]
    [SerializeField] private float fadeToBlackDuration = 0.2f;
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private Image blackoutImage;

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamerSound;

    [Header("Дополнительно")]
    [SerializeField] private bool destroyMonsterAfter = true;

    private bool hasTriggered = false;
    private float originalFOV;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayScreamerRoutine());
        }
    }

    private IEnumerator PlayScreamerRoutine()
    {
        // 0. Автопоиск камеры
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) yield break;

        // Отключаем Animator, чтобы он не сбрасывал поворот костей
        if (monsterAnimator != null)
        {
            monsterAnimator.enabled = false;
        }

        // 1. Блокируем управление
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (playerController != null) playerController.enabled = false;

        originalFOV = mainCamera.fieldOfView;

        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            SetImageAlpha(0f);
        }

        if (screamerSound != null)
        {
            if (audioSource != null) audioSource.PlayOneShot(screamerSound);
            else AudioSource.PlayClipAtPoint(screamerSound, mainCamera.transform.position);
        }

        Vector3 originalCamPos = mainCamera.transform.localPosition;
        Quaternion startCamRot = mainCamera.transform.rotation;

        // 2. БЫСТРЫЙ ЗУМ
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;

            if (monsterHead != null)
            {
                monsterHead.LookAt(mainCamera.transform.position);
                Vector3 direction = (monsterHead.position - mainCamera.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    mainCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetRotation, t);
                }
            }

            mainCamera.fieldOfView = Mathf.Lerp(originalFOV, targetFOV, t);
            mainCamera.transform.localPosition = originalCamPos + (Random.insideUnitSphere * shakeIntensity);
            yield return null;
        }

        // 3. УДЕРЖАНИЕ ВЗГЛЯДА В УПОР
        float holdElapsed = 0f;
        while (holdElapsed < holdDuration)
        {
            holdElapsed += Time.deltaTime;

            if (monsterHead != null)
            {
                monsterHead.LookAt(mainCamera.transform.position);
            }

            mainCamera.transform.localPosition = originalCamPos + (Random.insideUnitSphere * shakeIntensity);
            yield return null;
        }

        // 4. УХОД В ТЕМНОТУ (СЛЕДЕНИЕ И ТРЯСКА ДО 100% ТЕМНОТЫ В ПОСЛЕДНЕМ КАДРЕ)
        if (blackoutImage != null)
        {
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeToBlackDuration)
            {
                fadeElapsed += Time.deltaTime;
                SetImageAlpha(Mathf.Lerp(0f, 1f, fadeElapsed / fadeToBlackDuration));

                // Голова смотрит прямо на нас ВСЁ время, пока экран темнеет
                if (monsterHead != null)
                {
                    monsterHead.LookAt(mainCamera.transform.position);
                }

                mainCamera.transform.localPosition = originalCamPos + (Random.insideUnitSphere * shakeIntensity);
                yield return null;
            }

            SetImageAlpha(1f); // Экран стал полностью чёрным
        }

        // =========================================================================
        // 5. ОТКЛЮЧЕНИЕ СЛЕНДЕРА И СБРОС КАМЕРЫ — ПРОИСХОДИТ В ПОЛНОЙ ТЕМНОТЕ
        // =========================================================================

        mainCamera.transform.localPosition = originalCamPos;
        mainCamera.fieldOfView = originalFOV;

        if (destroyMonsterAfter && monsterHead != null)
        {
            GameObject monsterRoot = monsterHead.root != null ? monsterHead.root.gameObject : monsterHead.gameObject;
            monsterRoot.SetActive(false);
        }

        yield return new WaitForSeconds(0.2f);

        // 6. ПЛАВНЫЙ ВЫХОД ИЗ ТЕМНОТЫ
        if (blackoutImage != null)
        {
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeInDuration)
            {
                fadeElapsed += Time.deltaTime;
                SetImageAlpha(Mathf.Lerp(1f, 0f, fadeElapsed / fadeInDuration));
                yield return null;
            }

            blackoutImage.gameObject.SetActive(false);
        }

        // 7. Возвращаем управление
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (playerController != null) playerController.enabled = true;

        gameObject.SetActive(false);
    }

    private void SetImageAlpha(float alpha)
    {
        if (blackoutImage != null)
        {
            Color c = blackoutImage.color;
            c.a = alpha;
            blackoutImage.color = c;
        }
    }
}