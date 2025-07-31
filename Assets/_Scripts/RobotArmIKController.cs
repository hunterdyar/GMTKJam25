using UnityEngine;

public class RobotArmIKController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform playerRoot;

    [Header("Idle Sway")]
    [SerializeField] private bool isLeftHand = false;
    [SerializeField] private float idleSwayAmplitude = 0.02f;
    [SerializeField] private float idleSwayFrequency = 1.5f;
    [SerializeField] private Vector3 idleSwayAxis = Vector3.up;

    [Header("Walk Dangle")]
    [SerializeField] private Vector3 walkOffset = new Vector3(0f, 0f, -0.2f);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float walkSwayAmount = 0.01f;
    [SerializeField] private float walkSwaySpeed = 6f;

    [Header("Jump Raise")]
    [SerializeField] private Vector3 jumpRaiseOffset = new Vector3(0f, 0.4f, 0f);
    [SerializeField] private float jumpRaiseSpeed = 8f;
    private Quaternion initialLocalRotation;
    private Quaternion jumpRotation;


    private Vector3 initialLocalPosition;
    private Vector3 swayOffset;
    private bool wasGrounded;
    private bool isJumpRaising;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        // 180° flip on X-axis from initial
        jumpRotation = initialLocalRotation * Quaternion.Euler(180f, 0f, 0f);

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (playerRoot == null)
            playerRoot = transform.root;

        wasGrounded = playerMovement != null && playerMovement.IsGrounded;
    }


    void Update()
    {
        if (playerMovement == null || playerRoot == null)
            return;

        bool isGrounded = playerMovement.IsGrounded;
        bool isMoving = playerMovement.IsMoving;

        // Detect jump start
        if (wasGrounded && !isGrounded)
        {
            isJumpRaising = true;
        }

        Vector3 targetPosition = initialLocalPosition;

        if (isJumpRaising)
        {
            targetPosition += jumpRaiseOffset;

            // End jump raise once player lands again
            if (isGrounded)
            {
                isJumpRaising = false;
            }
        }
        else if (isMoving && isGrounded)
        {
            // Walk dangle + slight sway
            float sway = Mathf.Sin(Time.time * walkSwaySpeed + (isLeftHand ? 0f : Mathf.PI)) * walkSwayAmount;
            Vector3 sideSway = playerRoot.right * sway;

            targetPosition += walkOffset + sideSway;
        }
        else if (isGrounded)
        {
            // Idle sway
            float idleSway = Mathf.Sin(Time.time * idleSwayFrequency + (isLeftHand ? 0f : Mathf.PI)) * idleSwayAmplitude;
            swayOffset = idleSwayAxis.normalized * idleSway;

            targetPosition += swayOffset;
        }

        // Smooth transition
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * (isJumpRaising ? jumpRaiseSpeed : followSpeed));

        // Choose target rotation
        Quaternion targetRotation = isJumpRaising ? jumpRotation : initialLocalRotation;

        // Smoothly rotate toward target
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * (isJumpRaising ? jumpRaiseSpeed : followSpeed));

        // Update grounded state for next frame
        wasGrounded = isGrounded;
    }
}
