using GMTK;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlatformerGameInput gameInput;
    [SerializeField] private Camera playerCamera;

    [Header("Movement Settings")]
    public float moveSpeed = 22f;
    public float turnSpeed = 10f;
    [SerializeField] private float stepHeight = 0.4f;
    [SerializeField] private float stepCheckDistance = 0.5f;

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float fallMultiplier = 3f;
    public float lowJumpMultiplier = 5f;
    public float maxFallSpeed = -30f;
    public LayerMask groundMask;

    [Header("Jump Buffer & Coyote Time")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.15f;
    [HideInInspector]public float jumpBufferTimer = -1f;
    [HideInInspector]public float coyoteTimer = 0f;

    [Header("Wall Jump")]
    public float wallPushForce = 12f;
    private bool isTouchingWall = false;
    private Vector3 wallNormal;

    private Rigidbody rb;
    [HideInInspector] public Vector3 inputDirection;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool jump;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (gameInput == null)
            gameInput = FindAnyObjectByType<PlatformerGameInput>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void OnEnable()
    {
        Timeline.OnInput += OnInput;
    }

    private void OnDisable()
    {
        Timeline.OnInput -= OnInput;
    }
    
    private void OnInput(int frame, GameInput input, bool instant)
    {
        jump = input.JumpButton != null && input.JumpButton.IsPressed(frame);
        Vector2 dir = (input.ArrowButton != null && input.ArrowButton.IsPressed(frame)) ? input.ArrowButton.GetDir() : Vector2.zero;
        dir.Normalize();
        inputDirection = new Vector3(dir.x, 0, dir.y);
        isGrounded = IsOnGround();
        
        // Wall detection
        isTouchingWall = IsTouchingWall(out wallNormal);

        if (jump)
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.fixedUnscaledDeltaTime;
        }

        // Coyote time
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.fixedUnscaledDeltaTime;
        }
        
        // Jump logic
        if (jumpBufferTimer > 0f && (coyoteTimer > 0f || isTouchingWall))
        {
            PerformJump();

            if (isTouchingWall)
            {
                rb.AddForce((wallNormal + Vector3.up * 0.2f) * wallPushForce, ForceMode.VelocityChange);
            }

            jumpBufferTimer = -1f;
        }
        
        PhysicsTick();
    }

    private Vector3 InputToWorld(Vector2 dir)
    {
        return new Vector3(dir.x, 0, dir.y).normalized;
    }
    
    void PhysicsTick()
    {
        TryStepUp();

        // Movement
        Vector3 desiredVelocity = inputDirection * moveSpeed;
        Vector3 velocityChange = desiredVelocity - rb.linearVelocity;
        rb.AddForce(new Vector3(velocityChange.x, 0f, velocityChange.z), ForceMode.VelocityChange);

        // Rotate toward movement direction
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRotation);
        }

        // Apply jump gravity modifiers
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1f), ForceMode.Acceleration);
        }
         else if (rb.linearVelocity.y > 0f && !jump)
         {
             rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f), ForceMode.Acceleration);
         }

        // Clamp fall speed
        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxFallSpeed, rb.linearVelocity.z);
        }
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset vertical
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool IsOnGround()
    {
        float radius = 0.45f;
        float offset = 0.05f;
        Vector3 bottom = transform.position + Vector3.down * (0.5f + offset);
        Vector3 top = transform.position + Vector3.down * (0.5f - offset);
        return Physics.CheckCapsule(top, bottom, radius, groundMask);
    }

    private bool IsTouchingWall(out Vector3 normal)
    {
        normal = Vector3.zero;
        if (inputDirection.sqrMagnitude < 0.01f) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float checkDistance = 0.55f;
        Vector3 direction = inputDirection.normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, checkDistance, groundMask))
        {
            normal = hit.normal;
            return true;
        }

        return false;
    }

    private void TryStepUp()
    {
        if (inputDirection.sqrMagnitude < 0.01f) return;

        Vector3 origin = transform.position;
        Vector3 forward = inputDirection.normalized;

        Vector3[] rayOrigins = new Vector3[]
        {
            origin + Vector3.up * 0.1f,
            origin + Vector3.up * 0.1f + transform.right * 0.2f,
            origin + Vector3.up * 0.1f - transform.right * 0.2f
        };

        foreach (var rayOrigin in rayOrigins)
        {
            if (Physics.Raycast(rayOrigin, forward, out RaycastHit lowerHit, stepCheckDistance, groundMask))
            {
                Vector3 checkPos = rayOrigin + Vector3.up * (stepHeight + 0.1f);
                Vector3 halfExtents = new Vector3(0.2f, 0.05f, 0.2f);

                if (!Physics.CheckBox(checkPos, halfExtents, Quaternion.identity, groundMask))
                {
                    if (Vector3.Angle(lowerHit.normal, Vector3.up) > 45f)
                        continue;

                    rb.position += Vector3.up * stepHeight;
                    return;
                }
            }
        }
    }

    public bool IsGrounded => isGrounded;

    public bool IsMoving => inputDirection.sqrMagnitude > 0.01f;

}
