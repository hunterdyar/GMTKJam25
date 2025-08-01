using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MenuCharacterController : MonoBehaviour
{
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
    private float wanderPauseTimer = 0f;
    private bool isPausedAtTarget = false;


    private Rigidbody rb;
    private float jumpCooldown;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 wanderTarget;
    private bool isWandering = false;
    private bool returningToStart = false;
    private float nextWanderTime;

    public static class MenuCharacterState
    {
        public static bool IsJumping = false;
        public static bool IsWandering = false;
    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        startPosition = transform.position;
        startRotation = transform.rotation;

        ScheduleNextJump();
        ScheduleNextWander();
    }


    void Update()
    {
        // Jumping logic
        jumpCooldown -= Time.deltaTime;
        if (jumpCooldown <= 0f)
        {
            Jump();
            ScheduleNextJump();
        }

        // Wandering logic
        if (!MenuCharacterState.IsJumping)
        {
            MenuCharacterState.IsWandering = isWandering; // <-- keep updated

            if (!isWandering && !returningToStart && Time.time >= nextWanderTime)
            {
                PickWanderTarget();
            }

            if (isWandering)
            {
                if (isPausedAtTarget)
                {
                    wanderPauseTimer -= Time.deltaTime;
                    if (wanderPauseTimer <= 0f)
                    {
                        isPausedAtTarget = false;
                        isWandering = false;
                        returningToStart = true;
                    }
                }
                else
                {
                    MoveToward(wanderTarget, () =>
                    {
                        // Reached wander target — now pause here
                        isPausedAtTarget = true;
                        wanderPauseTimer = Random.Range(pauseTimeMin, pauseTimeMax);
                    });
                }
            }

            else if (returningToStart)
            {
                MoveToward(startPosition, () =>
                {
                    returningToStart = false;
                    float pause = Random.Range(pauseTimeMin, pauseTimeMax);
                    nextWanderTime = Time.time + pause;
                });
            }
        }
        else
        {
            MenuCharacterState.IsWandering = false;
        }
    }

    void Jump()
    {
        if (rb != null)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        MenuCharacterState.IsJumping = true;
        Invoke(nameof(ClearJumpFlag), 0.8f);

        var arms = GetComponentsInChildren<RobotArmMenuIKController>();
        foreach (var arm in arms)
            arm.TriggerJumpPose();
    }

    void ClearJumpFlag()
    {
        MenuCharacterState.IsJumping = false;
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

    void PickWanderTarget()
    {
        Vector2 circle = Random.insideUnitCircle * wanderRadius;
        wanderTarget = new Vector3(startPosition.x + circle.x, startPosition.y, startPosition.z + circle.y);
        isWandering = true;

        // Reset pause state
        isPausedAtTarget = false;
    }

    void MoveToward(Vector3 target, System.Action onArrive)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0f; // Ignore vertical movement
        float distance = direction.magnitude;

        if (distance > 0.05f)
        {
            Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + move);

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
}
