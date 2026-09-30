using UnityEngine;

public class LoopSoundTrigger : MonoBehaviour
{
    [Header("Звуковые настройки")]
    public AudioSource audioSource;        // Компонент AudioSource
    public AudioClip soundClip;            // Звуковой файл (гул, шорох, эмбиент и т.д.)
    [Range(0f, 1f)]
    public float volume = 1f;              // Громкость звука

    [Header("Настройки триггера")]
    public bool triggerOnce = true;        // Срабатывать только один раз?

    private bool isTriggered = false;

    private void Start()
    {
        // Если AudioSource не перетащен в инспекторе, пробуем взять его с этого же объекта
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered && triggerOnce) return;

        // Проверяем, что зашел игрок
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            if (audioSource != null && soundClip != null)
            {
                isTriggered = true;

                audioSource.clip = soundClip;
                audioSource.volume = volume;
                audioSource.loop = true;      // Включаем бесконечный повтор
                audioSource.Play();
            }
        }
    }
}