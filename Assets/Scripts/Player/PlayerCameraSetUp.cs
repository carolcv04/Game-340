using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    private GameObject _cmCamObject;
    private void Start()
    {
        // Dynamically find the CinemachineCamera in children
        CinemachineCamera cmCamera = GetComponentInChildren<CinemachineCamera>(true);

        if (cmCamera != null)
        {
            _cmCamObject = cmCamera.gameObject;

            if (IsOwner)
            {
                _cmCamObject.SetActive(true);
                Debug.Log($"[Camera] Enabled cmcam for local player {OwnerClientId}");
            }
            else
            {
                _cmCamObject.SetActive(false);
                Debug.Log($"[Camera] Disabled cmcam for remote player {OwnerClientId}");
            }
        }
        else
        {
            Debug.LogError("[Camera] No CinemachineCamera found under Player prefab!");
        }
    }
}
