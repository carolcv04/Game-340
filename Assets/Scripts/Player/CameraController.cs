using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineCamera cinemachineCamera; // Assign your Cinemachine camera here
    
    public void ChangePlayer(Transform player)
    {
        // Set the camera's Follow and LookAt to the new player
        cinemachineCamera.Follow = player;
        cinemachineCamera.LookAt = player;
    }
}