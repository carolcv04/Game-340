using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/ItemPreset")]
public class ItemPreset : ScriptableObject
{
    [SerializeField] public string itemID;
    public string itemName;
    public Item prefab;
    public Sprite icon;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

        if (assetPath == null)
        {
            itemID = "";
            return;
        }
        
        var assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(assetGuid) || itemID != assetGuid)
        {
            itemID = assetGuid;
        }
    }
#endif
}

