using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControllerRoleSelector : MonoBehaviour
{
    #region Properties

    [Header("Components")]
    [SerializeField]
    private Button spectatorRoleButton;

    [SerializeField]
    private Button actorRoleButton;

    [Header("External controllers")]
    [SerializeField]
    private MenuControllerCharacterSelector characterSelectorController;

    public RoleSelectionController roleSelectionController;

    #endregion

    #region Public Methods

    public void ShowCharacterSelectorMenu()
    {
        characterSelectorController.gameObject.SetActive(true);
    }

    public void SelectCharacter(CharacterSheet sheet)
    {
        roleSelectionController.PickRoleAndCharacter(Role.Character, sheet);
    }

    public void DisableCharacter(CharacterSheet sheet)
    {
        characterSelectorController.DisableCharacter(sheet);
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (roleSelectionController == null)
        {
            roleSelectionController = FindObjectOfType<RoleSelectionController>();
        }
    }

    private void Start()
    {
        spectatorRoleButton.onClick.AddListener(() =>
        {
            roleSelectionController.PickRoleAndCharacter(Role.Spectator, null);
        });

        bool characterAvailable = false;
        var enumerator = roleSelectionController.lockedCharacters.GetEnumerator();

        while (characterAvailable == false && enumerator.MoveNext())
        {
            characterAvailable = !enumerator.Current.Value;
        }

        if (!characterAvailable)
        {
            actorRoleButton.interactable = false;
        }
    }   

    #endregion
}
