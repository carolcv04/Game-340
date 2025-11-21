using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Slot : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    public GameObject currentItem;
    private void Awake()
    {
        ResetTile();
    }
    private int _index;
    private InventoryView _inventoryView;

    public void Init(InventoryView inventoryView, int index)
    {
        _inventoryView = inventoryView;
        _index = index;
    }

    public void SetItem(InventoryItemData inventoryItemData)
    {
        if (!inventoryItemData.GetPreset())
        {
            ResetTile();
            return;
        }
        quantityText.text = inventoryItemData.quantity.ToString();
        itemImage.sprite = inventoryItemData.GetPreset().icon;
        itemImage.color = Color.white;
    }

    public void ResetTile()
    {
        itemImage.color = Color.clear;
        itemImage.sprite = null;
        quantityText.text = "";
    }
}
