using System.Collections.Generic;
using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    public List<Collider2D> detectedObject = new List<Collider2D>();
    public Collider2D col;
    public string targetTag = "Player";
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Trigger entered by: {collision.gameObject.name}, Tag: {collision.tag}");
        if (collision.gameObject.CompareTag(targetTag))
        {
            detectedObject.Add(collision);
            Debug.Log($"Player detected! Total detected: {detectedObject.Count}");
        }
    }
    
    void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            detectedObject.Remove(collision);
        }
    }
}
