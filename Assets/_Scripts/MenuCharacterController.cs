using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MenuCharacterController : MonoBehaviour
{
    public enum CharacterState { Idle, Wandering, Returning, Pausing, Jumping, Waving }
    public CharacterState currentState = CharacterState.Idle;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float minJumpInterval = 15f;
    [SerializeField] private float maxJumpInterval = 30f;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 2f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float wanderDelayMin = 6f;
    [SerializeField] private float wanderDelayMax = 12f;
    [SerializeField] private float pauseTimeMin = 3f;
    [SerializeField] private float pauseTimeMax = 8f;

    [Header("Wave Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float waveFacingThreshold = 0.9f;
    [SerializeField] private float waveDuration = 2f;
    [SerializeField] private float waveChancePerIdle = 0.1f;
    [SerializeField] private float waveCooldownMin = 10f;
    [SerializeField] private float waveCooldownMax = 20f;
    [SerializeField] private float returnFacingChance = 0.3f;
    private bool shouldFaceCameraOnReturn = false;
    private Quaternion returnTargetRotation;


    [Header("Wave IK Target")]
    [SerializeField] private Transform rightHandIKTarget;
    [SerializeField] private float waveAmplitude = 0.1f;
    [SerializeField] private float waveFrequency = 4f;
    [SerializeField] private float waveRaiseHeight = 0.4f;

    [Header("Facial Expressions")]
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private Material defaultFaceMaterial;
    [SerializeField] private Material[] idleExpressions;
    [SerializeField] private float expressionChance = 0.3f;
    [SerializeField] private float expressionDuration = 2f;
    [SerializeField] private float expressionCooldownMin = 12f;
    [SerializeField] private float expressionCooldownMax = 24f;
    private float nextExpressionTime;


    private Rigidbody rb;
    private float jumpCooldown;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 wanderTarget;
    private float nextWanderTime;
    private float pauseTimer;

    private float waveTimer;
    private float nextWaveTime;
    private Vector3 waveBasePosition;
    private Quaternion waveBaseRotation;

    private float expressionTimer;
    private bool isUsingExpression;

    private bool jumpSuppressed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        startPosition = transform.position;
        startRotation = transform.rotation;

        if (rightHandIKTarget != null)
        {
            waveBasePosition = rightHandIKTarget.localPosition;
            waveBaseRotation = rightHandIKTarget.localRotation;
        }

        ScheduleNextJump();
        ScheduleNextWander();
        ScheduleNextWave();
    }

    void Update()
    {
        if (!jumpSuppressed)
        {
            jumpCooldown -= Time.deltaTime;

            if (jumpCooldown <= 0f && currentState != CharacterState.Jumping && currentState != CharacterState.Waving)
            {
                StartCoroutine(JumpRoutine());
            }
        }

        if (currentState == CharacterState.Waving)
        {
            AnimateWave();

            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                currentState = CharacterState.Idle;
                jumpSuppressed = false;
                ScheduleNextWander();
                ScheduleNextWave();

                if (rightHandIKTarget != null)
                {
                    rightHandIKTarget.localPosition = waveBasePosition;
                    rightHandIKTarget.localRotation = waveBaseRotation;
                }
            }
            return;
        }

        HandleExpression();

        switch (currentState)
        {
            case CharacterState.Idle:
                if (Time.time >= nextWaveTime && Random.value < waveChancePerIdle && IsFacingCamera())
                {
                    TryWave();
                }
                else if (Time.time >= nextWanderTime)
                {
                    PickWanderTarget();
                    currentState = CharacterState.Wandering;
                }
                break;

            case CharacterState.Wandering:
                MoveToward(wanderTarget, () =>
                {
                    pauseTimer = Random.Range(pauseTimeMin, pauseTimeMax);
                    currentState = CharacterState.Pausing;
                });
                break;

            case CharacterState.Pausing:
                pauseTimer -= Time.deltaTime;
                if (pauseTimer <= 0f)
                {
                    currentState = CharacterState.Returning;

                    if (Random.value < returnFacingChance && cameraTransform)
                    {
                        Vector3 directionToCamera = (cameraTransform.position - transform.position);
                        directionToCamera.y = 0f;
                        if (directionToCamera != Vector3.zero)
                        {
                            returnTargetRotation = Quaternion.LookRotation(directionToCamera);
                            shouldFaceCameraOnReturn = true;
                        }
                    }
                    else
                    {
                        shouldFaceCameraOnReturn = false;
                    }
                }
                break;

            case CharacterState.Returning:
                MoveToward(startPosition, () =>
                {
                    ScheduleNextWander();
                    currentState = CharacterState.Idle;
                    shouldFaceCameraOnReturn = false; // Reset after arrival
                });
                break;
        }
    }

    void HandleExpression()
    {
        // Only attempt expression if we're idle-like and not already using one
        if ((currentState == CharacterState.Idle || currentState == CharacterState.Wandering || currentState == CharacterState.Returning) &&
            !isUsingExpression &&
            Time.time >= nextExpressionTime &&
            idleExpressions.Length > 0 &&
            Random.value < expressionChance)
        {
            int index = Random.Range(0, idleExpressions.Length);
            Material[] mats = faceRenderer.materials;

            if (mats.Length > 7)
            {
                mats[7] = idleExpressions[index];
                faceRenderer.materials = mats;
                isUsingExpression = true;
                expressionTimer = expressionDuration;
            }
        }

        // Timer to revert back to default face
        if (isUsingExpression)
        {
            expressionTimer -= Time.deltaTime;
            if (expressionTimer <= 0f)
            {
                Material[] mats = faceRenderer.materials;
                if (mats.Length > 7)
                {
                    mats[7] = defaultFaceMaterial;
                    faceRenderer.materials = mats;
                }
                isUsingExpression = false;
                ScheduleNextExpression(); // Set the next cooldown window
            }
        }
    }


    void AnimateWave()
    {
        if (rightHandIKTarget == null) return;

        Vector3 basePos = waveBasePosition + Vector3.up * waveRaiseHeight;
        float sideOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rightHandIKTarget.localPosition = basePos + Vector3.right * sideOffset;

        Vector3 handRotation = new Vector3(0f, 0f, 0f);
        rightHandIKTarget.localRotation = Quaternion.Euler(handRotation);
    }

    IEnumerator JumpRoutine()
    {
        currentState = CharacterState.Jumping;

        // Reset facial expression immediately
        Material[] mats = faceRenderer.materials;
        if (mats.Length > 7)
        {
            mats[7] = defaultFaceMaterial;
            faceRenderer.materials = mats;
        }
        isUsingExpression = false;

        ScheduleNextExpression(); // Schedule next cooldown

        if (rb != null)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        var arms = GetComponentsInChildren<RobotArmMenuIKController>();
        foreach (var arm in arms)
            arm.TriggerJumpPose();

        yield return new WaitForSeconds(0.8f);
        currentState = CharacterState.Idle;
        ScheduleNextJump();
    }



    void ScheduleNextJump()
    {
        jumpCooldown = Random.Range(minJumpInterval, maxJumpInterval);
    }

    void ScheduleNextWander()
    {
        float delay = Random.Range(wanderDelayMin, wanderDelayMax);
        nextWanderTime = Time.time + delay;
    }

    void ScheduleNextWave()
    {
        float delay = Random.Range(waveCooldownMin, waveCooldownMax);
        nextWaveTime = Time.time + delay;
    }

    void ScheduleNextExpression()
    {
        nextExpressionTime = Time.time + Random.Range(expressionCooldownMin, expressionCooldownMax);
    }


    void PickWanderTarget()
    {
        Vector2 circle = Random.insideUnitCircle * wanderRadius;
        wanderTarget = new Vector3(startPosition.x + circle.x, startPosition.y, startPosition.z + circle.y);
    }

    void MoveToward(Vector3 target, System.Action onArrive)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance > 0.05f)
        {
            Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + move);

            // If returning and supposed to face camera, slowly rotate toward that rotation
            if (currentState == CharacterState.Returning && shouldFaceCameraOnReturn)
            {
                Quaternion smoothed = Quaternion.Slerp(transform.rotation, returnTargetRotation, Time.deltaTime * 2f);
                rb.MoveRotation(smoothed);
            }

            if (move != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f));
            }
        }
        else
        {
            onArrive?.Invoke();
        }
    }

    public void TryWave()
    {
        if (currentState == CharacterState.Idle && IsFacingCamera())
        {
            currentState = CharacterState.Waving;
            waveTimer = waveDuration;
            jumpSuppressed = true;

            // Reset facial expression to default
            Material[] mats = faceRenderer.materials;
            if (mats.Length > 7)
            {
                mats[7] = defaultFaceMaterial;
                faceRenderer.materials = mats;
            }
            isUsingExpression = false;

            ScheduleNextExpression(); // Reset facial expression cooldown
        }
    }


    bool IsFacingCamera()
    {
        if (!cameraTransform) return false;
        Vector3 toCamera = (cameraTransform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, toCamera);
        return dot >= waveFacingThreshold;
    }
}
