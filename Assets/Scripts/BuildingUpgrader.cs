using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingUpgrader : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase[] upgradeTiles;
    public Vector2Int size = new Vector2Int(6, 6);
    
    public void UpgradeAtCell(Vector3Int anchorCell)
    {
        BoundsInt bounds = new BoundsInt(anchorCell.x, anchorCell.y, anchorCell.z,
            size.x, size.y, 1);
        // The tile array length must equal bounds.size.x * bounds.size.y * bounds.size.z
        if (upgradeTiles.Length != size.x * size.y)
        {
            Debug.LogError("upgradeTiles array size mismatch footprint size");
            return;
        }
        tilemap.SetTilesBlock(bounds, upgradeTiles);
    }

}
