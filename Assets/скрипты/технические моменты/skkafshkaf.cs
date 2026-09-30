using System.Collections;
using UnityEngine;
using TMPro;

public class FuseBoxSequential : MonoBehaviour
{
    [Header("1. Настройки Текста (До опускания рычага)")]
    public TextMeshProUGUI subText;
    [TextArea]
    public string click1Message = "Странно... Что щиток вообще делает в этом месте?";
    public float fadeDuration = 0.5f;
    public float displayDuration = 3f;

    [Header("Сообщения ПОСЛЕ опускания рычага")]
    [TextArea]
    public string postLeverMessage1 = "Странно. Это явно плохой знак...";
    [TextArea]
    public string postLeverMessage2 = "Надо проверить ту дверь с фонарём.";
    public float delayBetweenMessages = 0.5f;        // Пауза между первым и вторым сообщением

    [Header("2. Дверца Щитка")]
    public Transform doorTransform;                 // Объект дверцы щитка
    public Vector3 doorOpenRotation = new Vector3(0, 90, 0); // Поворот дверцы
    public float doorOpenSpeed = 3f;

    [Header("3. Рычаг и Выключение Света")]
    public Transform leverTransform;                // Объект рычага
    public Vector3 leverDownRotation = new Vector3(45, 0, 0); // Поворот рычага
    public float leverSpeed = 5f;
    [Header("Звук выключения света")]
    public AudioClip switchOffSound;                // Аудиоклип выключения света

    [Header("Ссылка на скрипт света")]
    public LightGroup lightGroup;                   // Перетащи сюда объект со скриптом LightGroup

    [Header("4. Дверь (открывается после рычага)")]
    public Transform mainDoorTransform;             // Объект самой двери в комнате/коридоре
    public Vector3 mainDoorOpenRotation = new Vector3(0, 90, 0); // На какой угол открыть дверь
    public float mainDoorOpenSpeed = 2f;            // Скорость открытия двери

    [Header("Состояние (для сторонних триггеров)")]
    public bool isLeverPulled = false;              // Флаг: поднят ли рычаг щитка

    private int clickStage = 0;                     // Шаг взаимодействия (0, 1, 2)
    private bool inTrigger = false;
    private bool isAnimating = false;

    private Quaternion doorTargetRot;
    private Quaternion leverTargetRot;
    private Quaternion mainDoorTargetRot;

    private void Start()
    {
        // Запоминаем целевые повороты
        if (doorTransform != null)
            doorTargetRot = doorTransform.localRotation * Quaternion.Euler(doorOpenRotation);

        if (leverTransform != null)
            leverTargetRot = leverTransform.localRotation * Quaternion.Euler(leverDownRotation);

        if (mainDoorTransform != null)
            mainDoorTargetRot = mainDoorTransform.localRotation * Quaternion.Euler(mainDoorOpenRotation);

        // Прозрачность текста по умолчанию = 0
        if (subText != null)
        {
            Color c = subText.color;
            c.a = 0f;
            subText.color = c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            inTrigger = false;
        }
    }

    private void Update()
    {
        // Реакция на E или ЛКМ
        if (inTrigger && !isAnimating && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        switch (clickStage)
        {
            case 0:
                // 1 КЛИК: Показываем первый текст
                clickStage = 1;
                StartCoroutine(ShowSingleTextRoutine(click1Message));
                break;

            case 1:
                // 2 КЛИК: Открываем дверцу щитка
                clickStage = 2;
                StartCoroutine(OpenDoorRoutine());
                break;

            case 2:
                // 3 КЛИК: Поднимаем рычаг, гасим свет, открываем дверь и показываем 2 сообщения
                clickStage = 3;
                StartCoroutine(PullLeverRoutine());
                break;

            case 3:
                // Больше ничего не происходит
                break;
        }
    }

    private IEnumerator ShowSingleTextRoutine(string message)
    {
        if (subText == null) yield break;

        subText.text = message;
        Color color = subText.color;

        // Появление
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            subText.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(displayDuration);

        // Исчезновение
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            subText.color = color;
            yield return null;
        }
        color.a = 0f;
        subText.color = color;
    }

    private IEnumerator OpenDoorRoutine()
    {
        if (doorTransform == null) yield break;
        isAnimating = true;

        while (Quaternion.Angle(doorTransform.localRotation, doorTargetRot) > 0.5f)
        {
            doorTransform.localRotation = Quaternion.Slerp(doorTransform.localRotation, doorTargetRot, Time.deltaTime * doorOpenSpeed);
            yield return null;
        }

        doorTransform.localRotation = doorTargetRot;
        isAnimating = false;
    }

    private IEnumerator PullLeverRoutine()
    {
        isAnimating = true;

        // 1. Поворачиваем рычаг
        if (leverTransform != null)
        {
            while (Quaternion.Angle(leverTransform.localRotation, leverTargetRot) > 0.5f)
            {
                leverTransform.localRotation = Quaternion.Slerp(leverTransform.localRotation, leverTargetRot, Time.deltaTime * leverSpeed);
                yield return null;
            }
            leverTransform.localRotation = leverTargetRot;
        }

        // Активируем флаг — рычаг поднят
        isLeverPulled = true;

        // 2. Выключаем свет
        // 2. Выключаем свет и играем звук
        if (switchOffSound != null)
        {
            AudioSource.PlayClipAtPoint(switchOffSound, transform.position);
        }

        if (lightGroup != null)
        {
            lightGroup.TurnOffAllLights();
        }

        // 3. Открываем внешнюю дверь
        if (mainDoorTransform != null)
        {
            while (Quaternion.Angle(mainDoorTransform.localRotation, mainDoorTargetRot) > 0.5f)
            {
                mainDoorTransform.localRotation = Quaternion.Slerp(mainDoorTransform.localRotation, mainDoorTargetRot, Time.deltaTime * mainDoorOpenSpeed);
                yield return null;
            }
            mainDoorTransform.localRotation = mainDoorTargetRot;
        }

        // 4. По очереди выводим два текста с паузой
        yield return StartCoroutine(ShowSingleTextRoutine(postLeverMessage1));
        yield return new WaitForSeconds(delayBetweenMessages);
        yield return StartCoroutine(ShowSingleTextRoutine(postLeverMessage2));

        isAnimating = false;
    }
}