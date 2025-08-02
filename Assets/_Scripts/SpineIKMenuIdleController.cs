using UnityEngine;
using static MenuCharacterController;

public class SpineIKMenuIdleController : MonoBehaviour
{

    private MenuCharacterController controller;

    [Header("Idle Bob")]
    [SerializeField] private float idleBobAmplitude = 0.02f;
    [SerializeField] private float idleBobFrequency = 1.5f;

    [Header("Speed Settings")]
    [SerializeField] private float springSpeed = 10f;

    [Header("Idle Turn")]
    [SerializeField] private float maxIdleTurnAngle = 10f;
    [SerializeField] private float idleTurnFrequency = 4f;
    [SerializeField] private float idleTurnSpeed = 2f;
    [SerializeField] private float idleHoldDuration = 2f;

    [Header("Wander Lean")]
    [SerializeField] private Vector3 wanderOffset = new Vector3(0f, 0f, -0.05f);
    [SerializeField] private Vector3 wanderRotation = new Vector3(-10f, 0f, 0f);

    private bool isReturningToCenter = false;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Quaternion targetIdleRotation;

    private float idleTurnTimer = 0f;
    private float idleTurnCooldown = 0f;
    private float idleTurnDuration = 1f;

    void Start()
    {
        controller = GetComponentInParent<MenuCharacterController>();

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        targetIdleRotation = initialLocalRotation;
    }

    void Update()
    {
        // --- Idle Bobbing ---
        float bob = Mathf.Sin(Time.time * idleBobFrequency) * idleBobAmplitude;
        Vector3 targetOffset = new Vector3(0f, 0f, bob);

        // --- Idle Turn ---
        if (idleTurnCooldown <= 0f && idleTurnTimer <= 0f)
        {
            idleTurnCooldown = Random.Range(idleTurnFrequency * 0.8f, idleTurnFrequency * 1.2f);
            idleTurnTimer = idleTurnDuration;

            float angle = Random.Range(-maxIdleTurnAngle, maxIdleTurnAngle);
            targetIdleRotation = initialLocalRotation * Quaternion.Euler(0f, angle, 0f);
        }

        if (idleTurnTimer > 0f)
        {
            idleTurnTimer -= Time.deltaTime;

            if (idleTurnTimer <= 0f && !isReturningToCenter)
            {
                idleTurnTimer = idleHoldDuration;
                isReturningToCenter = true;
            }
            else if (idleTurnTimer <= 0f && isReturningToCenter)
            {
                targetIdleRotation = initialLocalRotation;
                isReturningToCenter = false;
            }
        }

        idleTurnCooldown -= Time.deltaTime;

        // --- Animate Position & Rotation ---
        Vector3 targetPosition = initialLocalPosition + targetOffset;
        Quaternion targetRot = targetIdleRotation;

        if (controller != null && controller.currentState == MenuCharacterController.CharacterState.Wandering)
        {
            targetPosition += wanderOffset;
            targetRot *= Quaternion.Euler(wanderRotation);
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * springSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * idleTurnSpeed);
    }
}
