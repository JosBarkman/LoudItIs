using UnityEngine;

// TODO: COnsider making this a scriptable object if characters are going to share goals
[System.Serializable]
public class CharacterGoal
{
    [TextArea(1, 3)]
    public string goal;
}

[CreateAssetMenu(fileName = "CharacterSheet", menuName = "CharacterSheet", order = 1)]
public class CharacterSheet : ScriptableObject
{
    [Header("Character settings")]
    public new string name;
    public string nickname;

    [TextArea(3, 9)]
    public string background;
    
    public Sprite portrait;
    
    public CharacterGoal[] goals;

    [Header("Settings")]
    public GameObject prefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
}