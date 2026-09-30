using UnityEngine;

public class AmbientSoundLoop : MonoBehaviour
{
    [Header("Обязательный Компонент")]
    [Tooltip("Перетащи сюда AudioSource с этого же объекта")]
    [SerializeField] private AudioSource audioSource;

    [Header("Звуковой Клип Атмосферы")]
    [SerializeField] private AudioClip ambientClip;

    [Header("Настройки Воспроизведения")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    [Tooltip("Включи, если звук должен играться только в определенном радиусе на карте (3D)")]
    [SerializeField] private bool is3DSound = false;

    [Header("Настройки 3D Звука (если включен is3DSound)")]
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 20f;

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError($"[AmbientSoundLoop] На объекте {gameObject.name} не назначен AudioSource в Inspector!");
            return;
        }

        SetupAudioSource();

        if (ambientClip != null)
        {
            audioSource.clip = ambientClip;
            audioSource.Play();
        }
    }

    private void SetupAudioSource()
    {
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;

        if (is3DSound)
        {
            audioSource.spatialBlend = 1.0f; // 3D звук (слышен в радиусе)
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
        else
        {
            audioSource.spatialBlend = 0.0f; // 2D звук (слышен везде)
        }
    }
}