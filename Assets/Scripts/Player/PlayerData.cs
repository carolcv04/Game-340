using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : CharacterData
{
    [Header("Player-Specific Stats")]
    public float maxTotalHealth;
}
