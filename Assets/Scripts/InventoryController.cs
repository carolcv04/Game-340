using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;

    public GameObject slotPrefab;

    public int slotCount;

    public static InventoryController Instance { get; private set; }

    public GameObject[] itemPrefabs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    void Start()
    {
        for (int i = 0; i < slotCount; i++) //Initializing all slots.
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>(); //Grabbing the game slot objec
            if (i < itemPrefabs.Length) //Check if an item fits in the slot.
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform); //Places item on the slot.
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Ensures the item is centered to the slot.
                slot.currentItem = item;
            }
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false;
        
        //Check if we have this item
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
               Item slotItem = slot.currentItem.GetComponent<Item>();
               if (slotItem != null && slotItem.itemName == itemToAdd.itemName)
               {
                   //same item & stack 
                   slotItem.AddToSTack();
                   return true;
               }
            }
        }
        
        //Look for an empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform); //Create item in the inventory
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Item is centered in the middle of the slot
                slot.currentItem = newItem;
                return true;
            }
        }
        Debug.Log("Inventory is full!");
        return false;
    }
    
}
