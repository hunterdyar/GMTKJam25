using UnityEngine;

public class SpineIKSpringController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Offsets")]
    [SerializeField] private Vector3 jumpOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private Vector3 landOffset = new Vector3(0f, -0.1f, 0f);
    [SerializeField] private Vector3 walkOffset = Vector3.zero;

    [Header("Idle Bob")]
    [SerializeField] private float idleBobAmplitude = 0.02f;
    [SerializeField] private float idleBobFrequency = 1.5f;

    [Header("Speed Settings")]
    [SerializeField] private float springSpeed = 10f;
    [SerializeField] private float landBounceDuration = 0.15f;

    [Header("Idle Turn")]
    [SerializeField] private float maxIdleTurnAngle = 10f;
    [SerializeField] private float idleTurnFrequency = 4f; // seconds between attempts
    [SerializeField] private float idleTurnSpeed = 2f;
    [SerializeField] private float idleHoldDuration = 2f;

    [Header("Face Expression")]
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private Material defaultFaceMaterial;
    [SerializeField] private Material[] idleExpressions; // e.g. Surprised, Bored, Happy

    [SerializeField] private float expressionChance = 0.3f; // 30% chance during idle turn
    [SerializeField] private float expressionDuration = 2f;

    private float expressionTimer = 0f;
    private bool isUsingExpression = false;
    private bool isReturningToCenter = false;
    private Vector3 initialLocalPosition;
    private bool wasGrounded;
    private float landTimer = 0f;
    private Quaternion initialLocalRotation;
    private Quaternion targetIdleRotation;
    private float idleTurnTimer = 0f;
    private float idleTurnCooldown = 0f;
    private float idleTurnDuration = 1f;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        targetIdleRotation = initialLocalRotation;


        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        wasGrounded = playerMovement != null && playerMovement.IsGrounded;
    }

    void Update()
    {
        if (playerMovement == null) return;

        bool isGrounded = playerMovement.IsGrounded;
        bool isMoving = playerMovement.IsMoving;

        Vector3 targetOffset = Vector3.zero;

        // Jumping (raise up slightly)
        if (!isGrounded && wasGrounded)
        {
            targetOffset = jumpOffset;
        }
        // Landing (trigger brief squash)
        else if (isGrounded && !wasGrounded)
        {
            landTimer = landBounceDuration;
        }

        // Active landing squash
        if (landTimer > 0f)
        {
            landTimer -= Time.deltaTime;
            targetOffset = landOffset;
        }
        else if (isGrounded && !isMoving)
        {
            // Idle bobbing + occasional turn
            float bob = Mathf.Sin(Time.time * idleBobFrequency) * idleBobAmplitude;
            targetOffset = new Vector3(0f, 0f, bob);

            // Try idle turn
            if (idleTurnCooldown <= 0f && idleTurnTimer <= 0f)
            {
                idleTurnCooldown = Random.Range(idleTurnFrequency * 0.8f, idleTurnFrequency * 1.2f);
                idleTurnTimer = idleTurnDuration;
                // Change face expression
                if (!isUsingExpression && idleExpressions.Length > 0 && Random.value < expressionChance)
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

                float angle = Random.Range(-maxIdleTurnAngle, maxIdleTurnAngle);
                targetIdleRotation = initialLocalRotation * Quaternion.Euler(0f, angle, 0f);
            }

            // If currently turning, count down and then return
            if (idleTurnTimer > 0f)
            {
                idleTurnTimer -= Time.deltaTime;

                // When turn ends, start holding
                if (idleTurnTimer <= 0f && !isReturningToCenter)
                {
                    idleTurnTimer = idleHoldDuration;
                    isReturningToCenter = true;
                }
                // After hold, return to center
                else if (idleTurnTimer <= 0f && isReturningToCenter)
                {
                    targetIdleRotation = initialLocalRotation;
                    isReturningToCenter = false;
                }
            }

            idleTurnCooldown -= Time.deltaTime;
        }
        else if (isGrounded && isMoving)
        {
            targetOffset = walkOffset;

            // Reset rotation
            targetIdleRotation = initialLocalRotation;
            idleTurnTimer = 0f;
            idleTurnCooldown = 0f;

            // Reset facial expression when walking
            if (isUsingExpression)
            {
                Material[] mats = faceRenderer.materials;
                if (mats.Length > 7)
                {
                    mats[7] = defaultFaceMaterial;
                    faceRenderer.materials = mats;
                }
                isUsingExpression = false;
                expressionTimer = 0f;
            }

        }
        else
        {
            // Reset rotation in air or during jump
            targetIdleRotation = initialLocalRotation;
            idleTurnTimer = 0f;
            idleTurnCooldown = 0f;
            isReturningToCenter = false;

            // Reset facial expression if we’re not idle
            if (isUsingExpression)
            {
                Material[] mats = faceRenderer.materials;
                if (mats.Length > 7)
                {
                    mats[7] = defaultFaceMaterial;
                    faceRenderer.materials = mats;
                }
                isUsingExpression = false;
                expressionTimer = 0f;
            }

        }


        Vector3 targetPosition = initialLocalPosition + targetOffset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * springSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetIdleRotation, Time.deltaTime * idleTurnSpeed);

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
            }
        }

        wasGrounded = isGrounded;
    }
}
