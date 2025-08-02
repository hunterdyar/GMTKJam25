using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    [SerializeField] private float launchForce = 25f;
    [SerializeField] private Vector3 launchDirection = Vector3.up;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip launchSound;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string launchTriggerName = "Launch";

    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 0.5f;

    private bool justLaunched = false;

    private void OnTriggerEnter(Collider other)
    {
        if (justLaunched) return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && other.CompareTag("Player"))
        {
            var dir = transform.TransformDirection(launchDirection.normalized);
            var opposingFactor = Vector3.Dot(rb.linearVelocity, -dir);
            rb.linearVelocity += dir * opposingFactor;
            rb.AddForce(dir * launchForce, ForceMode.VelocityChange);

            if (audioSource != null && launchSound != null)
            {
                audioSource.PlayOneShot(launchSound);
            }

            if (animator != null)
            {
                animator.SetTrigger(launchTriggerName);
            }

            justLaunched = true;
            Invoke(nameof(ResetLaunch), cooldownDuration);
        }
    }

    private void ResetLaunch()
    {
        justLaunched = false;
    }
}
