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

    //todo: this won't work when ticking through line-by-line.
    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 0.5f;

    private bool _justLaunched = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_justLaunched) return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && other.CompareTag("Player"))
        {
            //get world launch dir
            var dir = transform.TransformDirection(launchDirection.normalized);

            //remove speed in direction against launch
            var opposingFactor = Vector3.Dot(rb.linearVelocity, -dir);
            rb.linearVelocity -= dir * opposingFactor;
            
            //launch
            rb.AddForce(dir * launchForce, ForceMode.VelocityChange);

            if (audioSource != null && launchSound != null)
            {
                audioSource.PlayOneShot(launchSound);
            }

            if (animator != null)
            {
                animator.SetTrigger(launchTriggerName);
            }

            _justLaunched = true;
            Invoke(nameof(ResetLaunch), cooldownDuration);
        }
    }

    private void ResetLaunch()
    {
        _justLaunched = false;
    }
}
