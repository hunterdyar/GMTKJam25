using UnityEngine;

public class ShadowFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float shadowHeight = 0.01f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastDistance = 5f;

    private void LateUpdate()
    {
        Vector3 rayOrigin = player.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            // Snap to ground hit point
            transform.position = new Vector3(hit.point.x, hit.point.y + shadowHeight, hit.point.z);
        }
        else
        {
            // No ground hit — keep shadow under player at fixed low Y
            Vector3 playerPos = player.position;
            transform.position = new Vector3(playerPos.x, shadowHeight, playerPos.z);
        }
    }
}
