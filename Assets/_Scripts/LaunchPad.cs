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
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // optional: reset vertical velocity
            rb.AddForce(launchDirection.normalized * launchForce, ForceMode.VelocityChange);
        }
    }
}
