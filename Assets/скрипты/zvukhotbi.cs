using UnityEngine;

public class zvukhotbi : MonoBehaviour
{
    [Header("Обязательные Компоненты")]
    [Tooltip("Перетащи сюда CharacterController с игрока")]
    [SerializeField] private CharacterController controller;

    [Tooltip("Перетащи сюда AudioSource с игрока")]
    [SerializeField] private AudioSource audioSource;

    [Header("Звуки Шагов")]
    [Tooltip("Массив аудиоклипов шагов")]
    [SerializeField] private AudioClip[] footstepSounds;

    [Header("Настройки Интервала")]
    [Tooltip("Интервал между шагами при ходьбе (в секундах)")]
    [SerializeField] private float stepInterval = 0.45f;

    [Tooltip("Разброс громкости шагов для естественности")]
    [Range(0f, 1f)][SerializeField] private float volumeBase = 0.9f;
    [Range(0f, 0.5f)][SerializeField] private float volumeVariation = 0.05f;

    [Tooltip("Разброс высоты тона (pitch)")]
    [Range(0.5f, 1.5f)][SerializeField] private float pitchBase = 1.0f;
    [Range(0f, 0.2f)][SerializeField] private float pitchVariation = 0.05f;

    private float stepTimer;

    private void Update()
    {
        if (controller == null || audioSource == null) return;

        // --- УЛУЧШЕННАЯ ПРОВЕРКА ДВИЖЕНИЯ ---
        // Проверяем, зажал ли игрок клавиши движения (Input) И находится ли он на земле.
        // Это убирает инерцию и задержку при остановке.
        bool hasPlayerInput = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool isMoving = controller.isGrounded && hasPlayerInput;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                stepTimer = stepInterval;
            }
        }
        else
        {
            // Сбрасываем таймер, чтобы при начале движения шаг воспроизвелся сразу
            stepTimer = 0f;
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        int randomIndex = Random.Range(0, footstepSounds.Length);
        AudioClip clip = footstepSounds[randomIndex];

        if (clip != null)
        {
            // Улучшенная вариация громкости и тональности
            audioSource.volume = volumeBase + Random.Range(-volumeVariation, volumeVariation);
            audioSource.pitch = pitchBase + Random.Range(-pitchVariation, pitchVariation);

            audioSource.PlayOneShot(clip);
        }
    }
}