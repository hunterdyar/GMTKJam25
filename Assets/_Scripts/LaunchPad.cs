using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    [SerializeField] private float launchForce = 25f;
    [SerializeField] private Vector3 launchDirection = Vector3.up;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && other.CompareTag("Player"))
        {
            var dir = transform.TransformDirection(launchDirection.normalized);
            var opposingFactor = Vector3.Dot(rb.linearVelocity, -dir);
            rb.linearVelocity = rb.linearVelocity+(dir*opposingFactor);
            rb.AddForce(dir * launchForce, ForceMode.VelocityChange);
        }
    }
}
