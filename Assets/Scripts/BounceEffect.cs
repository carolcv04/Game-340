using System.Collections;
using UnityEngine;

public class BounceEffect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float bounceCount;
    public float bounceHeight = 0.3f;
    public float bounceDuration = 0.2f;

    public void StartBounce()
    {
        StartCoroutine(BounceHandler());
    }

    private IEnumerator BounceHandler()
    {
        Vector3 startPosition = transform.position;
        float localHeight = bounceHeight;
        float localDuration = bounceDuration;

        for (int i = 0; i < bounceCount; i++)
        {
            yield return Bounce(startPosition, localHeight, localDuration / 2);
            localHeight *= 0.5f;
            localDuration *= 0.8f;
        }

        transform.position = startPosition;
    }

    private IEnumerator Bounce(Vector3 startPosition, float height, float duration)
    {
        Vector3 peak = startPosition + Vector3.up * height;
        float elapsed = 0f;
        
        //Move up
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, peak, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        
        //Move down
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(peak, startPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
}
