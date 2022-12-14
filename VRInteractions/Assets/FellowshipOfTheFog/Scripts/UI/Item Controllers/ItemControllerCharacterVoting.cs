using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemControllerCharacterVoting : MonoBehaviour
{
    #region Properties

    [Header("Components")]
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Text characterName;
    [SerializeField] private Text characterNickname;
    [SerializeField] private Text votes;
    [SerializeField] private Button button;

    #endregion

    #region Public Methods

    public void SetCharacter(CharacterSheet character, UnityAction action)
    {
        characterPortrait.sprite = character.portrait;
        characterName.text = character.name;
        characterNickname.text = character.nickname;
        votes.text = "0";

        button.onClick.AddListener(action);
    }

    public void UpdateVotes(int votes)
    {
        this.votes.text = votes.ToString();
    }

    #endregion
}
