using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public bool isInteracted{ get; private set; }

    public string interactableID;

    public GameObject itemPrefab;

    public Sprite interactedSprite;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactableID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    protected virtual void OpenInteractable()
    {
        SetInteracted(true);
        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }

    public void SetInteracted(bool interacted)
    {
       
        if (isInteracted = interacted)
        {
            GetComponent<SpriteRenderer>().sprite = interactedSprite;
        }
    }
    
    public void Interact()
    {
        if (!CanInteract()) return;
        OpenInteractable();
    }

    public bool CanInteract()
    {
        return !isInteracted;
    }
}
