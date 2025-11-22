using UnityEngine;

public class UnstuckPlayer : MonoBehaviour
{
    [Header("Optional: Assign Player Transform")]
    public Transform playerTransform;
    public void TeleportTo(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("[PlayerTeleporter] Target GameObject is null!");
            return;
        }
        playerTransform.position = target.transform.position;
        playerTransform.rotation = target.transform.rotation; // Optional: match rotation
        Debug.Log($"[PlayerTeleporter] Teleported player to {target.name} at {target.transform.position}");
    }
}