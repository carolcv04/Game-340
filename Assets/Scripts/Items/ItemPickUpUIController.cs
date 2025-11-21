using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemPickUpUIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static ItemPickUpUIController Instance { get; private set; }
    public GameObject popupPrefab;
    public int maxPopups = 5;
    public float popUpDuration = 3f;
    
    private readonly Queue<GameObject> activePopUps = new Queue<GameObject>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple itemPickUpUIControllers detected.");
            Destroy(gameObject);
        }
    }
    private IEnumerator FadeOutAndDestroy(GameObject popup)
    {
        yield return new WaitForSeconds(popUpDuration);
        if (popup == null) yield break;
        
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        for(float timePassed = 0f; timePassed < 1f; timePassed += Time.deltaTime)
        {
            if (popup == null) yield break;
            canvasGroup.alpha = 1f - timePassed;
            yield return null;
        }
        Destroy(popup);
    }

    // Update is called once per frame
    public void ShowItemPickUpUI(string itemName, Sprite itemIcon)
    {
        GameObject newPopup = Instantiate(popupPrefab, transform);
        newPopup.GetComponentInChildren<TMP_Text>().text = itemName;

        Image itemImage = newPopup.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemImage)
        {
            itemImage.sprite = itemIcon;
        }
        
        activePopUps.Enqueue(newPopup);
        if (activePopUps.Count > maxPopups)
        {
            Destroy(activePopUps.Dequeue());
        }
        
        //Fade out & destory object
        StartCoroutine(FadeOutAndDestroy(newPopup));
    }
}
