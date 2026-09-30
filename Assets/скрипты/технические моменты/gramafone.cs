using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gramophone : MonoBehaviour
{
    private AudioSource audioSource;
    private OutlineController outlineCtrl;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Находим контроллер обводки на этом же объекте
        outlineCtrl = GetComponent<OutlineController>();
    }

    public void Interact()
    {
        if (audioSource.clip == null)
        {
            Debug.LogWarning($"[Gramophone] На {gameObject.name} нет аудиоклипа!");
            return;
        }

        if (audioSource.isPlaying)
        {
            // Выключаем музыку
            audioSource.Stop();

            // Разрешаем обводку и сразу включаем (т.к. прицел наведен)
            if (outlineCtrl != null)
            {
                outlineCtrl.canBeHighlighted = true;
                outlineCtrl.HighlightOn();
            }
        }
        else
        {
            // Включаем музыку
            audioSource.Play();

            // Блокируем обводку и сразу тушим
            if (outlineCtrl != null)
            {
                outlineCtrl.canBeHighlighted = false;
                outlineCtrl.HighlightOff();
            }
        }
    }
}