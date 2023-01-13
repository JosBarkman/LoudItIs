using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class MenuControllerVotingMenu : MonoBehaviour
{
    #region Properties

    [Header("Settigns")]
    [SerializeField] private GameObject voteCharacterItemPrefab;
    [SerializeField] private Transform characterListParent;

    private Dictionary<PlayerRef, ItemControllerCharacterVoting> characters = new Dictionary<PlayerRef, ItemControllerCharacterVoting>();

    [Header("External Components")]
    [SerializeField] private EndingSequenceController controller;

    #endregion

    #region Public Methods

    public void AddCharacterItems(Dictionary<PlayerRef, CharacterSheet> characters)
    {
        foreach (var character in characters)
        {
            GameObject item = Instantiate(voteCharacterItemPrefab, characterListParent);
            ItemControllerCharacterVoting votingItem = item.GetComponent<ItemControllerCharacterVoting>();
            this.characters.Add(character.Key, votingItem);

            votingItem.SetCharacter(character.Value, () => { 
                controller.VotePlayer(character.Key); 
            });
        }

        Canvas.ForceUpdateCanvases();

        var enumerator = this.characters.GetEnumerator();
        enumerator.MoveNext();
        enumerator.Current.Value.gameObject.SetActive(false);
        enumerator.Current.Value.gameObject.SetActive(true);
    }

    public void UpdatePlayerVotes(PlayerRef player, int votes)
    {
        if (characters.ContainsKey(player))
        {
            characters[player].UpdateVotes(votes);
        }
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (controller == null)
        {
            controller = FindObjectOfType<EndingSequenceController>();
        }
    }

    #endregion
}
