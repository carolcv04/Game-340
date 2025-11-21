// using System.Collections;
// using System.Collections.Generic;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.EventSystems;
// public class ItemDragHandler : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
// {
//     Transform originalParent;
//     CanvasGroup canvasGroup;
//
//     public float minDropDistance = 2f;
//     public float maxDropDistance = 3f;
//     private IPointerClickHandler _pointerClickHandlerImplementation;
//     [SerializeField] private List<GameObject> itemPrefabs = new List<GameObject>();
//
//     private InventoryController inventoryController;
//
//     // Start is called before the first frame update
//     void Start()
//     {
//         canvasGroup = GetComponent<CanvasGroup>();
//         Debug.Log($"ItemDragHandler initialized on {gameObject.name}");
//         inventoryController = InventoryController.Instance;
//     }
//
//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         Debug.Log($"[OnBeginDrag] Started dragging {gameObject.name}");
//         originalParent = transform.parent; //Save OG parent
//         Debug.Log($"[OnBeginDrag] Original parent: {originalParent.name}");
//         
//         transform.SetParent(transform.root); //Above other canvas'
//         Debug.Log($"[OnBeginDrag] Moved to root: {transform.root.name}");
//         
//         canvasGroup.blocksRaycasts = false;
//         canvasGroup.alpha = 0.6f; //Semi-transparent during drag
//     }
//
//     public void OnDrag(PointerEventData eventData)
//     {
//         transform.position = eventData.position; //Follow the mouse
//     }
//
//     public void OnEndDrag(PointerEventData eventData)
//     {
//         Debug.Log($"[OnEndDrag] Ended dragging {gameObject.name} at position {eventData.position}");
//         
//         canvasGroup.blocksRaycasts = true; //Enables raycasts
//         canvasGroup.alpha = 1f; //No longer transparent
//
//         Debug.Log($"[OnEndDrag] pointerEnter: {(eventData.pointerEnter != null ? eventData.pointerEnter.name : "NULL")}");
//         
//         Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>(); //Slot where item dropped
//         if(dropSlot == null)
//         {
//             Debug.Log("[OnEndDrag] No slot component on pointerEnter, checking parent...");
//             GameObject dropItem = eventData.pointerEnter;
//             if (dropItem != null)
//             {
//                 dropSlot = dropItem.GetComponentInParent<Slot>();
//                 Debug.Log($"[OnEndDrag] Found slot in parent: {(dropSlot != null ? dropSlot.gameObject.name : "NULL")}");
//             }
//         }
//         else
//         {
//             Debug.Log($"[OnEndDrag] Found dropSlot directly: {dropSlot.gameObject.name}");
//         }
//         
//         Slot originalSlot = originalParent.GetComponent<Slot>();
//         Debug.Log($"[OnEndDrag] Original slot: {(originalSlot != null ? originalSlot.gameObject.name : "NULL")}");
//
//         if(dropSlot != null) //Found a valid slot
//         {
//             Debug.Log($"[OnEndDrag] Dropped on slot: {dropSlot.gameObject.name}");
//             if (dropSlot == originalSlot) //Check if the drop slot is the same oas original slot
//             {
//                 transform.SetParent(dropSlot.transform);
//                 dropSlot.currentItem = gameObject;
//                 Debug.Log($"[OnEndDrag] Item moved to slot: {dropSlot.gameObject.name}");
//                 
//                 GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Center
//                 return;
//             }
//             
//             if (dropSlot.currentItem != null) //There's an item in the current slot
//             {
//                 Item draggedItem = GetComponent<Item>();
//                 Item targetItem  = dropSlot.currentItem.GetComponent<Item>();
//
//                 if (draggedItem.itemName == targetItem.itemName) //Stack the items if they're the same
//                 {
//                     targetItem.AddToSTack(draggedItem.quantity);
//                     originalSlot.currentItem = null;
//                     Destroy(gameObject);
//                 }
//                 else //Swap the items if they're different
//                 {
//                     Debug.Log($"[OnEndDrag] Slot has item: {dropSlot.currentItem.name} - SWAPPING");
//                     //Slot has an item - swap items
//                     dropSlot.currentItem.transform.SetParent(originalSlot.transform);
//                     originalSlot.currentItem = dropSlot.currentItem;
//                     dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//                     
//                     //Move item into drop slot
//                     transform.SetParent(dropSlot.transform);
//                     dropSlot.currentItem = gameObject;
//                     Debug.Log($"[OnEndDrag] Item moved to slot: {dropSlot.gameObject.name}");
//                     
//                     GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Center
//                 }
//             }
//             else //Slot is empty. Moves the item to the new slot & clears the original slot
//             {
//                 Debug.Log("[OnEndDrag] Slot is empty - clearing original slot");
//                 originalSlot.currentItem = null;
//                 
//                 //Move item into drop slot
//                 transform.SetParent(dropSlot.transform);
//                 dropSlot.currentItem = gameObject;
//                 Debug.Log($"[OnEndDrag] Item moved to slot: {dropSlot.gameObject.name}");
//                 
//                 GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Center
//             }
//         }
//         else
//         {
//             Debug.Log("[OnEndDrag] No slot found under drop point");
//             //If where we are dropping is not within inventory
//             bool withinInventory = IsWithinInventory(eventData.position);
//             Debug.Log($"[OnEndDrag] Is within inventory: {withinInventory}");
//             
//             if (!withinInventory)
//             {
//                 Debug.Log("[OnEndDrag] Dropping item into world");
//                 DropItem(originalSlot);
//             }
//             else
//             {
//                 Debug.Log("[OnEndDrag] Within inventory but no slot - returning to original parent");
//                 //No slot under drop point
//                 transform.SetParent(originalParent);
//                 GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Center
//             }
//         }
//     }
//
//     bool IsWithinInventory(Vector2 position)
//     {
//         RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>(); //gets the shape of our inventory panel
//         bool isWithin = RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, position); //if what we passed in contains the mouse
//         Debug.Log($"[IsWithinInventory] Checking position {position} against inventory rect {inventoryRect.gameObject.name}: {isWithin}");
//         return isWithin;
//     }
//
//     void DropItem(Slot originalSlot)
//     {
//         if (!IsOwner) return;
//         
//         Item item = GetComponent<Item>();
//         int quantity = item.quantity; //Get the current item quantity
//
//         if (quantity > 1)
//         {
//             item.RemoveFromSTack();
//             transform.SetParent(originalParent);
//             GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//             quantity = 1;
//         }
//         else
//         {
//             originalSlot.currentItem = null;
//         }
//         
//         //Find player
//         Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
//         if (playerTransform == null)
//         {
//             Debug.LogWarning("[DropItem] No player found with 'Player' tag!");
//             return;
//         }
//         
//         //Find a random drop pos
//         // Get the pickup detector radius from the player's child CircleCollider2D
//         CircleCollider2D detector = playerTransform.GetComponentInChildren<CircleCollider2D>();
//         float pickupRadius = detector != null ? detector.radius * playerTransform.localScale.x : 0f;
//
//         // Pick a random direction, then choose a distance between safeMinDistance and maxDropDistance
//         Vector2 direction = Random.insideUnitCircle.normalized;
//         float distance = Random.Range(pickupRadius + minDropDistance, maxDropDistance);
//
//         // Final drop offset
//         Vector2 dropOffset = direction * distance;
//         Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;
//
//         // Instantiate new drop item
//         GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
//         Item droppedItem = dropItem.GetComponent<Item>();
//
//         itemPrefabs.Add(dropItem);
//         // Spawn it on the network
//         dropItem.GetComponent<NetworkObject>().Spawn();
//
//         // Set quantity
//         droppedItem.quantity = 1;
//         
//         SoundEffectManager.Play("Drop");
//         BounceEffect bounceEffect = droppedItem.GetComponent<BounceEffect>();
//         if (bounceEffect != null)
//         {
//             bounceEffect.StartBounce();
//             Debug.Log("[DropItem] Started bounce effect");
//         }
//         
//         //Destroy the UI
//         Debug.Log($"[DropItem] Destroying UI item: {gameObject.name}");
//         if (originalSlot.currentItem == null)
//         {
//             droppedItem.RequestDestroy();
//         }
//     }
//
//     public void OnPointerClick(PointerEventData eventData)
//     {
//         if (eventData.button == PointerEventData.InputButton.Right)
//         {
//             SplitStack();
//         }
//     }
//
//     private void SplitStack()
//     {
//         Item item = GetComponent<Item>();
//         if (item == null || item.quantity <= 1) return;
//         
//         int splitAmount = item.quantity / 2;
//
//         item.RemoveFromSTack(splitAmount);
//         GameObject newItem = item.CloneItem(splitAmount);
//
//         if (inventoryController == null || newItem == null) return;
//
//         foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
//         {
//             Slot slot = slotTransform.GetComponent<Slot>();
//             if (slot != null && slot.currentItem == null)
//             {
//                 slot.currentItem = newItem;
//                 newItem.transform.SetParent(slot.transform);
//                 newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//                 return;
//             }
//         }
//         item.AddToSTack(splitAmount);
//         Destroy(gameObject);
//     }
// }

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    public float minDropDistance = 2f;
    public float maxDropDistance = 3f;
    
    private InventoryController inventoryController;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Debug.Log($"ItemDragHandler initialized on {gameObject.name}");
        
        // Find the inventory controller on the player
        inventoryController = GetComponentInParent<InventoryController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[OnBeginDrag] Started dragging {gameObject.name}");
        originalParent = transform.parent;
        
        transform.SetParent(transform.root);
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[OnEndDrag] Ended dragging {gameObject.name}");
        
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        
        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if(dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }
        
        Slot originalSlot = originalParent.GetComponent<Slot>();

        if(dropSlot != null)
        {
            if (dropSlot == originalSlot)
            {
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
            
            if (dropSlot.currentItem != null)
            {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                if (draggedItem.itemName == targetItem.itemName)
                {
                    // Stack items - need to sync with server
                    int slotIndex = GetSlotIndex(dropSlot);
                    // inventoryController.StackItemsServerRpc(slotIndex, draggedItem.quantity);
                    
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                }
                else
                {
                    // Swap items
                    dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                    originalSlot.currentItem = dropSlot.currentItem;
                    dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    
                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                originalSlot.currentItem = null;
                
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            bool withinInventory = IsWithinInventory(eventData.position);
            
            if (!withinInventory)
            {
                Debug.Log("[OnEndDrag] Dropping item into world");
                DropItem(originalSlot);
            }
            else
            {
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
    }

    bool IsWithinInventory(Vector2 position)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, position);
    }

    void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        int quantity = item.quantity;
        int slotIndex = GetSlotIndex(originalSlot);

        if (quantity > 1)
        {
            // Drop 1, keep the rest
            // inventoryController.DropItemServerRpc(slotIndex, 1);
            
            item.RemoveFromSTack();
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            // Drop all
            // inventoryController.DropItemServerRpc(slotIndex, quantity);
            
            originalSlot.currentItem = null;
            Destroy(gameObject);
        }
        
        SoundEffectManager.Play("Drop");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;
        
        int slotIndex = GetSlotIndex(originalParent.GetComponent<Slot>());
        // inventoryController.SplitStackServerRpc(slotIndex);
    }
    
    private int GetSlotIndex(Slot slot)
    {
        if (slot == null || inventoryController == null) return -1;
        
        Transform inventoryPanel = inventoryController.inventoryPanel.transform;
        for (int i = 0; i < inventoryPanel.childCount; i++)
        {
            if (inventoryPanel.GetChild(i) == slot.transform)
            {
                return i;
            }
        }
        return -1;
    }
}