using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/EnemyData")]

public class EnemyData : CharacterData
{
    [Header("Enemy-Specific Stats")]
    public float detectionRange;
}
