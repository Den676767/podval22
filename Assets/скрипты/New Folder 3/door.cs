using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorSequence : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerMovement playerMovement;
    public CameraController cameraController; // Перетащи сюда Main Camera
    public Transform entranceDoor;

    [Header("Настройки двери")]
    public float openAngle = 20.0f;
    public float doorCloseDuration = 1.5f;
    public float startDelay = 0.3f;

    [Header("Настройки разворота игрока")]
    public float turnDuration = 1.5f;

    public AudioClip doorSlamSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Блокируем движение и мышь на старте
        if (playerMovement != null) playerMovement.canMove = false;
        if (cameraController != null) cameraController.canLook = false;

        StartCoroutine(RunDoorSequence());
    }

    IEnumerator RunDoorSequence()
    {
        if (entranceDoor != null)
        {
            Vector3 baseEuler = entranceDoor.localEulerAngles;
            Quaternion closedRot = Quaternion.Euler(baseEuler);
            Quaternion openRot = Quaternion.Euler(baseEuler.x, baseEuler.y + openAngle, baseEuler.z);

            entranceDoor.localRotation = openRot;

            yield return new WaitForSeconds(startDelay);

            if (audioSource != null && doorSlamSound != null)
            {
                audioSource.PlayOneShot(doorSlamSound);
            }

            float elapsed = 0f;

            while (elapsed < doorCloseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / doorCloseDuration);
                entranceDoor.localRotation = Quaternion.Slerp(openRot, closedRot, t);
                yield return null;
            }
            entranceDoor.localRotation = closedRot;
        }

        yield return new WaitForSeconds(0.4f);

        float turnElapsed = 0f;
        Quaternion startPlayerRot = transform.rotation;
        Quaternion targetPlayerRot = startPlayerRot * Quaternion.Euler(0, 180f, 0);

        while (turnElapsed < turnDuration)
        {
            turnElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, turnElapsed / turnDuration);
            transform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, t);
            yield return null;
        }
        transform.rotation = targetPlayerRot;

        // Возвращаем управление мыши и движению
        if (playerMovement != null) playerMovement.canMove = true;
        if (cameraController != null) cameraController.canLook = true;
    }
}