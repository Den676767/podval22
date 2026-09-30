using System.Collections;
using UnityEngine;

public class TriggerLaugh : MonoBehaviour
{
    [Header("Настройки звуков")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] laughSounds;

    [Header("Резкая пугающая музыка")]
    [SerializeField] private AudioClip jumpscareMusic; // Сюда перетащи трек резкой музыки

    [Header("Параметры")]
    [Tooltip("Сколько раз подряд проиграть смех")]
    [SerializeField] private int laughCount = 4;
    [Tooltip("Задержка между звуками (в секундах)")]
    [SerializeField] private float delayBetweenLaughs = 0.5f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayLaughSequence());
        }
    }

    private IEnumerator PlayLaughSequence()
    {
        if (audioSource == null || laughSounds == null || laughSounds.Length == 0) yield break;

        // Проигрываем цепочку смеха
        for (int i = 0; i < laughCount; i++)
        {
            AudioClip clip = laughSounds[Random.Range(0, laughSounds.Length)];
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }

            yield return new WaitForSeconds(delayBetweenLaughs);
        }

        // После смеха сразу включаем резкую пугающую музыку
        if (jumpscareMusic != null)
        {
            audioSource.PlayOneShot(jumpscareMusic);
        }

        // Выключаем сам триггер
        gameObject.SetActive(false);
    }
}