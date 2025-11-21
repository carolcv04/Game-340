using UnityEngine;

public class CharacterData : ScriptableObject
{
    [Header("Base Stats")]
    public string characterName;
    public int maxHealth;
    public float speed;
    public GameObject modelPrefab;
    public float damage;
}
