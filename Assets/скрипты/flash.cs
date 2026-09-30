using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(AudioSource))]
public class Flashlight : MonoBehaviour
{
    [Header("Клавиша")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Звук")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private Light flashlightLight;
    private bool isOn = false;

    void Start()
    {
        flashlightLight = GetComponent<Light>();
        flashlightLight.enabled = false;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }

    void ToggleFlashlight()
    {
        isOn = !isOn;
        flashlightLight.enabled = isOn;

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, 0.5f);
        }
    }
}