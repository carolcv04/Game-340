using Unity.Collections;
 using Unity.Netcode;
 using UnityEngine;
 
 [System.Serializable]
 public struct InventoryItemData : INetworkSerializable, System.IEquatable<InventoryItemData>
 {
     public FixedString64Bytes itemId;
     public int quantity;
     
     public ItemPreset GetPreset()
     {
         // Look up preset by itemId
         ItemDatabase.itemDatabaseInstance.TryGetItem(itemId.ToString(), out var preset);
         return preset;
     }
     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
     {
         serializer.SerializeValue(ref itemId);
         serializer.SerializeValue(ref quantity);
     }
 
     public bool Equals(InventoryItemData other)
     {
         return itemId == other.itemId && quantity == other.quantity;
     }
 }