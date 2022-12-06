using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Memory
{
    public CharacterSheet character;

    [TextArea(3, 9)]
    public string memory;
}

public class Memories : MonoBehaviour
{
    #region Properties

    [SerializeField]
    private Memory[] memoriesArray;

    [SerializeField]
    private Text memoryText;

    private Dictionary<string, string> memories = new Dictionary<string, string>();

    #endregion

    #region Public Methods

    public void ShowMemory(CharacterSheet character)
    {
        if (!memories.ContainsKey(character.name))
        {
            return;
        }

        string memory = memories[character.name];
        memoryText.text = memory;
        memoryText.gameObject.SetActive(true);
    }

    public void HideMemory()
    {
        memoryText.gameObject.SetActive(false);
    }

    #endregion

    #region Unity Events

    private void Start()
    {
        memoryText.gameObject.SetActive(false);

        foreach (Memory memory in memoriesArray)
        {
            memories.Add(memory.character.name, memory.memory);
        }
    }

    #endregion
}
