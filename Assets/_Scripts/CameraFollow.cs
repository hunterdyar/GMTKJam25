using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Controls")] public InputActionReference _lookLeft;
    public InputActionReference _lookRight;
    public bool FlipDir;

    [Header("Camera Offset")] public bool Only4Dirs;
    public float offsetDistance = 10;
    public float followSpeed = 5f;

    [Header("Look At Target")]
    public bool lookAtTarget = true;

    public int LookDir;
    private Vector3Int[] LookDirs =
    {
        new Vector3Int(1, 0, 1), //up right
        new Vector3Int(1, 0, 0), //right
        new Vector3Int(1, 0, -1), //down right
        new Vector3Int(0, 0, -1), //down
        new Vector3Int(-1, 0, -1), //down left
        new Vector3Int(-1, 0, 0), //left
        new Vector3Int(-1, 0, 1), //up left
        new Vector3Int(0, 0, 1), //up
    };

    public float[] LookXToWorldX =
    {
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
    };

    public float[] LookYToWorldY =
    {
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
    };
    private Vector3 _currentVelocity;

    void Awake()
    {
        _lookLeft.action.Enable();
        _lookRight.action.Enable();
        if (Only4Dirs)
        {
            LookDirs = new[]
            {
                new Vector3Int(1, 0, 1), //up right
                new Vector3Int(1, 0, -1), //down right
                new Vector3Int(-1, 0, -1), //down left
                new Vector3Int(-1, 0, 1), //up left
            };
        }
    }
    void Start()
    {
        LookDir = 0;
        
    }

    [ContextMenu("Rotate Camera Left")]
    public void RotateCameraLeft()
    {
        //dear future me: sorry I wrote hacky C code in C#.
        LookDir = ++LookDir%LookDirs.Length;
    }

    [ContextMenu("Rotate Camera Right")]
    public void RotateCameraRight()
    {
        LookDir = (LookDirs.Length+ --LookDir)%LookDirs.Length;
    }

    public Vector3 InputDirToWorldDir(Vector2 input, float worldRotate = -45)
    {
        var facing = Quaternion.Euler(0, worldRotate, 0)*(Vector3)LookDirs[LookDir];
        var ind = new Vector3(input.x, 0, input.y).normalized;
        
        return Quaternion.LookRotation(facing, Vector3.up) * ind;
        
        //todo: if transform.position isn't desired position, then this is wrong.
        //return transform.TransformDirection(new Vector3(input.x, 0, input.y).normalized);
        // return transform.InverseTransformDirection(new Vector3(input.x, 0, input.y).normalized);
    }

    void Update()
    {
        if (_lookLeft.action.WasPerformedThisFrame())
        {
            if (FlipDir)
            {
                RotateCameraRight();
            }else
            {
                RotateCameraLeft();
            }
        }

        if (_lookRight.action.WasPerformedThisFrame())
        {
            if (FlipDir)
            {
                RotateCameraLeft();
            }
            else
            {
                RotateCameraRight();
            }
        }
    }
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = GetTargetPosition() + GetOffset();

        // Smooth damping
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, 1f / followSpeed);
        
        if (lookAtTarget)
        {
            transform.LookAt(GetTargetPosition());
        }
    }

    private Vector3 GetOffset()
    {
        var d = LookDirs[LookDir];
        return new Vector3(-d.x*offsetDistance, offsetDistance, -d.z*offsetDistance);
    }


    private Vector3 GetTargetPosition()
    {
        // If the target has a Rigidbody, use its current physics position
        if (target.TryGetComponent<Rigidbody>(out var rb))
        {
            return rb.position;
        }

        // Otherwise, fallback to transform
        return target.position;
    }

}
