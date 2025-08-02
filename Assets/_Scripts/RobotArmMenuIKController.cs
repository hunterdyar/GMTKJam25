using UnityEngine;

public class RobotArmMenuIKController : MonoBehaviour
{
    [Header("Idle Sway")]
    [SerializeField] private bool isLeftHand = false;
    [SerializeField] private float idleSwayAmplitude = 0.02f;
    [SerializeField] private float idleSwayFrequency = 1.5f;
    [SerializeField] private Vector3 idleSwayAxis = Vector3.up;

    [Header("Jump Pose")]
    [SerializeField] private Vector3 jumpRaiseOffset = new Vector3(0f, 0.4f, 0f);
    [SerializeField] private float jumpRaiseSpeed = 8f;
    [SerializeField] private float jumpPoseDuration = 1f;

    [Header("Wander Lean")]
    [SerializeField] private Vector3 wanderArmOffset = new Vector3(0f, 0f, -0.05f);
    [SerializeField] private Vector3 wanderArmRotation = new Vector3(-20f, 0f, 0f);

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Quaternion jumpRotation;

    private bool isJumpRaising = false;
    private float jumpTimer = 0f;

    private MenuCharacterController characterController;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        jumpRotation = initialLocalRotation * Quaternion.Euler(180f, 0f, 0f);
        characterController = GetComponentInParent<MenuCharacterController>();
    }

    void Update()
    {
        Vector3 targetPosition = initialLocalPosition;
        Quaternion finalRotation = initialLocalRotation;

        if (isJumpRaising)
        {
            jumpTimer -= Time.deltaTime;

            if (jumpTimer <= 0f)
            {
                isJumpRaising = false;
            }
            else
            {
                targetPosition += jumpRaiseOffset;
                finalRotation = jumpRotation;
            }
        }

        if (!isJumpRaising)
        {
            float idleSway = Mathf.Sin(Time.time * idleSwayFrequency + (isLeftHand ? 0f : Mathf.PI)) * idleSwayAmplitude;
            targetPosition += idleSwayAxis.normalized * idleSway;

            if (characterController != null && characterController.currentState == MenuCharacterController.CharacterState.Wandering)
            {
                targetPosition += wanderArmOffset;
                finalRotation *= Quaternion.Euler(wanderArmRotation);
            }
        }

        float speed = isJumpRaising ? jumpRaiseSpeed : 5f;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * speed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRotation, Time.deltaTime * speed);
    }

    public void TriggerJumpPose()
    {
        isJumpRaising = true;
        jumpTimer = jumpPoseDuration;
    }
}
