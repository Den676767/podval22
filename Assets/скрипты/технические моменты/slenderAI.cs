using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SlenderAI : MonoBehaviour
{
    public enum AIState
    {
        ShakingPhase,           // 1. Преследование спереди + ТРЯСКА (при сближении — скример)
        CooldownAfterShaking,   // Отдых
        JumpscarePhase,         // 2. Засада (спереди или сзади) + ожидание взгляда
        CooldownAfterJumpscare  // Долгий отдых
    }

    [Header("Текущее Состояние (для отладки)")]
    [SerializeField] private AIState currentState = AIState.ShakingPhase;

    [Header("Обязательные Компоненты")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator monsterAnimator;

    [Header("Кости Монстра для Тряски")]
    [SerializeField] private Transform headTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("Маски Слоев (Стены)")]
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Настройки 1 Фазы (Тряска Спереди)")]
    [SerializeField] private float minDistantRange = 10f;
    [SerializeField] private float maxDistantRange = 18f;
    [SerializeField] private float approachDistance = 4.5f;
    [SerializeField] private float noInteractionTimeout = 10f;
    [SerializeField] private float shakeIntensity = 0.8f;

    [Header("Настройки 2 Фазы (Засада со спины/спереди)")]
    [SerializeField] private float startDelay = 3f; // Время отсрочки в секундах
    [SerializeField] private float lookAngleThreshold = 40f;
    [SerializeField] private float ambushSpawnDistance = 6.0f;
    [SerializeField] private float jumpscareDistance = 2.8f;
    [SerializeField] private float backTrackingTimeout = 8f;
    [Range(0f, 100f)]
    [SerializeField] private float frontAmbushChance = 50f;

    [Header("Настройки Перезарядок")]
    [SerializeField] private float cdAfterShakingMin = 2f;
    [SerializeField] private float cdAfterShakingMax = 4f;
    [SerializeField] private float cdAfterJumpscare = 15f;

    [Header("Звуки")]
    [SerializeField] private AudioClip jumpscareSound;
    [SerializeField] private AudioClip[] laughSounds = new AudioClip[5];
    [SerializeField] private AudioClip teleportAppearSound;

    [Header("Эффекты Экранного Скримера")]
    [SerializeField] private Image blackoutImage;
    [SerializeField] private Image staticNoiseImage;
    [SerializeField] private Image vignetteOverlay;
    [SerializeField] private Image redFlashImage;
    [SerializeField] private float jumpscareShakeIntensity = 1.5f;

    private float stateTimer = 0f;
    private float noInteractionTimer = 0f;
    private float backTrackingTimer = 0f;

    private bool isBusy = false;
    private bool isActivated = false;

    private Vector3 originalCamLocalPos;
    private float originalFov;

    private Vector3 basePosition;
    private Quaternion baseRotation;

    private AIState previousState;

    private Quaternion originalHeadRot;
    private Quaternion originalLeftHandRot;
    private Quaternion originalRightHandRot;

    private void Start()
    {
        // Изначально логика заблокирована флагом, но сам скрипт включен, чтобы корутина работала
        isActivated = false;

        if (agent != null)
        {
            agent.speed = 0f;
            agent.acceleration = 0f;
            agent.angularSpeed = 0f;
            agent.isStopped = true;
            agent.enabled = false;
        }

        transform.position = new Vector3(0, -999f, 0);

        if (mainCamera != null)
        {
            originalCamLocalPos = mainCamera.transform.localPosition;
            originalFov = mainCamera.fieldOfView;
        }

        if (headTarget != null) originalHeadRot = headTarget.localRotation;
        if (leftHandTarget != null) originalLeftHandRot = leftHandTarget.localRotation;
        if (rightHandTarget != null) originalRightHandRot = rightHandTarget.localRotation;

        previousState = currentState;

        // Запускаем таймер отсрочки
        StartCoroutine(ActivationRoutine());
    }

    private IEnumerator ActivationRoutine()
    {
        // Ждем указанное время отсрочки
        yield return new WaitForSeconds(startDelay);

        // Разрешаем выполнение логики в Update
        isActivated = true;

        if (agent != null)
        {
            agent.enabled = true;
        }

        ApplyStateChange(currentState);
    }

    private void Update()
    {
        if (!isActivated || playerTransform == null || mainCamera == null || isBusy) return;

        if (currentState != previousState)
        {
            ApplyStateChange(currentState);
            previousState = currentState;
        }

        if (currentState == AIState.JumpscarePhase || currentState == AIState.ShakingPhase)
        {
            AlwaysFacePlayer();
        }

        switch (currentState)
        {
            case AIState.ShakingPhase:
                UpdateShakingPhase();
                break;

            case AIState.CooldownAfterShaking:
                UpdateCooldownPhase(AIState.JumpscarePhase);
                break;

            case AIState.JumpscarePhase:
                UpdateJumpscarePhase();
                break;

            case AIState.CooldownAfterJumpscare:
                UpdateCooldownPhase(AIState.ShakingPhase);
                break;
        }
    }

    private void LateUpdate()
    {
        if (!isActivated || mainCamera == null) return;

        if (currentState == AIState.ShakingPhase && !isBusy)
        {
            ApplyLimbShake();
        }
    }

    private void ApplyStateChange(AIState newState)
    {
        currentState = newState;
        previousState = newState;

        ResetLimbRotations();

        switch (newState)
        {
            case AIState.ShakingPhase:
                StartShakingPhase();
                break;
            case AIState.CooldownAfterShaking:
                StartCooldownAfterShaking();
                break;
            case AIState.JumpscarePhase:
                StartJumpscarePhase();
                break;
            case AIState.CooldownAfterJumpscare:
                StartCooldownAfterJumpscare();
                break;
        }
    }

    private void SetMonsterActive(bool active)
    {
        if (agent != null)
        {
            agent.enabled = active;
        }

        if (!active)
        {
            transform.position = new Vector3(0, -999f, 0);
        }
    }

    private void StartShakingPhase()
    {
        noInteractionTimer = noInteractionTimeout;
        SetMonsterActive(true);
        TeleportInFrontOfPlayer();
    }

    private void UpdateShakingPhase()
    {
        float distanceToPlayer = Vector3.Distance(basePosition, playerTransform.position);

        if (distanceToPlayer <= approachDistance)
        {
            StartCoroutine(OriginalJumpscareRoutine());
            return;
        }

        noInteractionTimer -= Time.deltaTime;
        if (noInteractionTimer <= 0f)
        {
            TeleportInFrontOfPlayer();
            noInteractionTimer = noInteractionTimeout;
        }
    }

    private void TeleportInFrontOfPlayer()
    {
        float frontAngle = Random.Range(-30f, 30f);

        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 spawnDir = Quaternion.Euler(0, frontAngle, 0) * camForward;

        float desiredDist = Random.Range(minDistantRange, maxDistantRange);
        Vector3 targetPos = GetValidSpawnPosition(spawnDir, desiredDist);

        SetMonsterPosition(targetPos);
        AlwaysFacePlayer();

        PlayRandomLaugh();
        if (teleportAppearSound != null) AudioSource.PlayClipAtPoint(teleportAppearSound, transform.position, 0.6f);
    }

    private Vector3 GetValidSpawnPosition(Vector3 direction, float desiredDistance)
    {
        Vector3 origin = playerTransform.position + Vector3.up * 1.0f;
        RaycastHit hit;

        float finalDistance = desiredDistance;

        if (Physics.Raycast(origin, direction, out hit, desiredDistance, obstacleLayers))
        {
            finalDistance = Mathf.Max(hit.distance - 1.2f, approachDistance + 0.5f);
        }

        Vector3 calculatedPos = playerTransform.position + direction * finalDistance;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(calculatedPos, out navHit, 5.0f, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        calculatedPos.y = playerTransform.position.y;
        return calculatedPos;
    }

    private void ApplyLimbShake()
    {
        transform.position = basePosition;
        transform.rotation = baseRotation;

        float headMult = shakeIntensity * 25f;
        float handMult = shakeIntensity * 8f;

        if (headTarget != null)
        {
            Quaternion hOffset = Quaternion.Euler(
                Random.Range(-headMult, headMult),
                Random.Range(-headMult, headMult),
                Random.Range(-headMult * 0.5f, headMult * 0.5f)
            );
            headTarget.localRotation = originalHeadRot * hOffset;
        }

        if (leftHandTarget != null)
        {
            Quaternion lOffset = Quaternion.Euler(
                Random.Range(-handMult, handMult),
                Random.Range(-handMult * 0.3f, handMult * 0.3f),
                Random.Range(-handMult, handMult)
            );
            leftHandTarget.localRotation = originalLeftHandRot * lOffset;
        }

        if (rightHandTarget != null)
        {
            Quaternion rOffset = Quaternion.Euler(
                Random.Range(-handMult, handMult),
                Random.Range(-handMult * 0.3f, handMult * 0.3f),
                Random.Range(-handMult, handMult)
            );
            rightHandTarget.localRotation = originalRightHandRot * rOffset;
        }
    }

    private void ResetLimbRotations()
    {
        if (headTarget != null) headTarget.localRotation = originalHeadRot;
        if (leftHandTarget != null) leftHandTarget.localRotation = originalLeftHandRot;
        if (rightHandTarget != null) rightHandTarget.localRotation = originalRightHandRot;
    }

    private void StartJumpscarePhase()
    {
        backTrackingTimer = backTrackingTimeout;
        SetMonsterActive(true);
        TeleportForAmbushPhase();
    }

    private void UpdateJumpscarePhase()
    {
        if (IsPlayerLookingAtMonster())
        {
            StartCoroutine(OriginalJumpscareRoutine());
            return;
        }

        backTrackingTimer -= Time.deltaTime;
        if (backTrackingTimer <= 0f)
        {
            TeleportForAmbushPhase();
            backTrackingTimer = backTrackingTimeout;
        }
    }

    private void TeleportForAmbushPhase()
    {
        float roll = Random.Range(0f, 100f);

        if (roll <= frontAmbushChance)
        {
            float frontAngle = Random.Range(-45f, 45f);
            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            Vector3 spawnDir = Quaternion.Euler(0, frontAngle, 0) * camForward;

            float desiredDist = Random.Range(minDistantRange, maxDistantRange);
            Vector3 targetPos = GetValidSpawnPosition(spawnDir, desiredDist);

            SetMonsterPosition(targetPos);
        }
        else
        {
            float backAngle = Random.Range(150f, 210f);
            Vector3 spawnDir = Quaternion.Euler(0, backAngle, 0) * mainCamera.transform.forward;
            spawnDir.y = 0;
            spawnDir.Normalize();

            Vector3 spawnPos = playerTransform.position + spawnDir * ambushSpawnDistance;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(spawnPos, out navHit, 3.0f, NavMesh.AllAreas))
            {
                spawnPos = navHit.position;
            }
            else
            {
                spawnPos.y = playerTransform.position.y;
            }

            SetMonsterPosition(spawnPos);
        }

        AlwaysFacePlayer();

        PlayRandomLaugh();
        if (teleportAppearSound != null) AudioSource.PlayClipAtPoint(teleportAppearSound, transform.position, 0.8f);
    }

    private void StartCooldownAfterShaking()
    {
        stateTimer = Random.Range(cdAfterShakingMin, cdAfterShakingMax);
        SetMonsterActive(false);
    }

    private void StartCooldownAfterJumpscare()
    {
        stateTimer = cdAfterJumpscare;
        SetMonsterActive(false);
    }

    private void UpdateCooldownPhase(AIState nextState)
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ApplyStateChange(nextState);
        }
    }

    private void SetMonsterPosition(Vector3 pos)
    {
        basePosition = pos;
        if (agent != null && agent.enabled)
        {
            agent.Warp(pos);
        }
        else
        {
            transform.position = pos;
        }
    }

    private void AlwaysFacePlayer()
    {
        Vector3 direction = (playerTransform.position - basePosition).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            baseRotation = Quaternion.LookRotation(direction);
            transform.rotation = baseRotation;
        }
    }

    private bool IsPlayerLookingAtMonster()
    {
        if (mainCamera == null) return false;

        Vector3 checkPos = transform.position + Vector3.up * 1.8f;
        Vector3 dirToMonster = (checkPos - mainCamera.transform.position).normalized;

        float angle = Vector3.Angle(mainCamera.transform.forward, dirToMonster);

        if (angle <= lookAngleThreshold)
        {
            if (!Physics.Linecast(mainCamera.transform.position, checkPos, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                return true;
            }
        }

        return false;
    }

    private void PlayRandomLaugh()
    {
        if (laughSounds != null && laughSounds.Length > 0)
        {
            int index = Random.Range(0, laughSounds.Length);
            if (laughSounds[index] != null)
            {
                AudioSource.PlayClipAtPoint(laughSounds[index], mainCamera.transform.position, 0.9f);
            }
        }
    }

    private IEnumerator OriginalJumpscareRoutine()
    {
        isBusy = true;

        if (jumpscareSound != null) AudioSource.PlayClipAtPoint(jumpscareSound, mainCamera.transform.position);
        PlayRandomLaugh();

        Vector3 jumpscarePos = mainCamera.transform.position + mainCamera.transform.forward * jumpscareDistance;
        jumpscarePos.y = playerTransform.position.y;
        SetMonsterPosition(jumpscarePos);

        Vector3 lookDirToCam = (mainCamera.transform.position - transform.position);
        lookDirToCam.y = 0;
        if (lookDirToCam != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirToCam);
        }

        Vector3 headPos = transform.position + Vector3.up * 1.8f;
        mainCamera.transform.LookAt(headPos);

        float duration = 0.9f;
        float timer = duration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            Vector3 shakeOffset = Random.insideUnitSphere * jumpscareShakeIntensity;
            mainCamera.transform.localPosition = originalCamLocalPos + shakeOffset;
            mainCamera.transform.LookAt(headPos);

            mainCamera.fieldOfView = originalFov - Random.Range(25f, 40f);

            if (staticNoiseImage != null)
            {
                staticNoiseImage.gameObject.SetActive(true);
                SetImageAlpha(staticNoiseImage, Random.Range(0.4f, 1f));
            }

            if (vignetteOverlay != null)
            {
                vignetteOverlay.gameObject.SetActive(true);
                SetImageAlpha(vignetteOverlay, Random.Range(0.5f, 1f));
            }

            if (redFlashImage != null)
            {
                redFlashImage.gameObject.SetActive(true);
                SetImageAlpha(redFlashImage, Random.Range(0.2f, 0.6f));
            }

            yield return null;
        }

        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            SetImageAlpha(blackoutImage, 1f);
        }

        if (staticNoiseImage != null) staticNoiseImage.gameObject.SetActive(false);
        if (vignetteOverlay != null) vignetteOverlay.gameObject.SetActive(false);
        if (redFlashImage != null) redFlashImage.gameObject.SetActive(false);

        mainCamera.transform.localPosition = originalCamLocalPos;
        mainCamera.fieldOfView = originalFov;

        ApplyStateChange(AIState.CooldownAfterJumpscare);

        yield return new WaitForSeconds(0.2f);

        if (blackoutImage != null)
        {
            float fadeTime = 0.5f;
            float fElapsed = 0f;
            while (fElapsed < fadeTime)
            {
                fElapsed += Time.deltaTime;
                SetImageAlpha(blackoutImage, 1f - (fElapsed / fadeTime));
                yield return null;
            }
            blackoutImage.gameObject.SetActive(false);
        }

        isBusy = false;
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}