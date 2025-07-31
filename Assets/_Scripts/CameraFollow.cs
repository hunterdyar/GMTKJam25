using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(-10f, 10f, -10f); // angled 45° down-right
    public float followSpeed = 5f;

    [Header("Look At Target")]
    public bool lookAtTarget = true;

    private Vector3 currentVelocity; // add this at the top

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = GetTargetPosition() + offset;

        // Smooth damping
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed);

        if (lookAtTarget)
        {
            transform.LookAt(GetTargetPosition());
        }
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
