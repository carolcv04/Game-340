using System;
using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundary;
    CinemachineConfiner2D confiner;
    [SerializeField] private float additivePos = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f; // Shorter fade out
    [SerializeField] private float cameraUpdateDelay = 0.1f; // Minimal buffer
    [SerializeField] private float fadeInDelay = 0.15f; // Quick camera settle time
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;
    [SerializeField] Animator transitions;
    
    private GameObject currentPlayer;

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }
    
    enum Direction {Up, Down, Left, Right, Teleport}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            UpdatePlayerPosition(collision.gameObject);
        }
    }
    
    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction.Equals(Direction.Teleport))
        {
            transitions.SetTrigger("End"); // Start fade out
            currentPlayer = player;
            
            // Schedule the sequence:
            // 1. Teleport player after fade out
            Invoke(nameof(TeleportPlayer), fadeOutDuration);
            
            // 2. Update camera boundary
            Invoke(nameof(UpdateCameraBoundary), fadeOutDuration + cameraUpdateDelay);
            
            // 3. Start fade in after camera has updated
            Invoke(nameof(StartFadeIn), fadeOutDuration + cameraUpdateDelay + fadeInDelay);
            return;
        }
        
        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivePos;
                break;
            case Direction.Down:
                newPos.y -= additivePos;
                break;
            case Direction.Left:
                newPos.x += additivePos;
                break;
            case Direction.Right:
                newPos.x -= additivePos;
                break;
        }
        player.transform.position = newPos;
    }
    
    private void TeleportPlayer()
    {
        currentPlayer.transform.position = teleportTargetPosition.position;
    }
    
    private void UpdateCameraBoundary()
    {
        confiner.BoundingShape2D = mapBoundary;
    }
    
    private void StartFadeIn()
    {
        transitions.SetTrigger("Start");
    }
}