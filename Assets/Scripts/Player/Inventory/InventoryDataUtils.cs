// using UnityEngine;
// using Unity.Netcode.Transports;
// using UnityEditor.Analytics;
//
// public static class InventoryDataUtils
// {
//     public static void ItemPresetWriter(BitPacker packer, ItemPreset preset)
//     {
//         if (preset == null)
//         {
//             Packer<bool>.Write(packer, false);
//         }
//
//         Packer<bool>.Write(packer, true);
//         Packer<string>.Write(packer, preset.itemID);
//     }
//
//     public static void ItemPresetreader(BitPacker packer, ref ItemPreset preset)
//     {
//         bool hasPreset = false;
//         Packer<bool>.Read(packer, ref haspreset);
//         if (!hasPreset)
//         {
//             preset = null;
//             return;
//         }
//         
//         //get database instance 
//             //if not, failed to get database 
//             
//         var itemID = default(string);
//         Packer<string>.Read(packer, ref itemID);
//         ItemDatabase.TryGetItem(itemID, out preset);
//         
//     }
// }
