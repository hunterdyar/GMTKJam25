using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatAmplitude = 0.25f; // How far it floats up and down
    [SerializeField] private float floatFrequency = 1f;     // How fast it floats

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 45f; // Degrees per second

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Floating effect
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation effect
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
