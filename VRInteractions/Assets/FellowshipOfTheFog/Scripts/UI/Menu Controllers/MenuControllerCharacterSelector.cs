using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuControllerCharacterSelector : MonoBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField]
    private GameObject characterPortraitItemPrefab;

    [SerializeField]
    private GameObject goalItemPrefab;

    [Header("Components")]
    [SerializeField]
    private Transform characterSelectorGrid;

    [Header("External controllers")]
    [SerializeField]
    private MenuControllerRoleSelector controller;

    [SerializeField]
    private MenuControllerCharacterDescription characterDescriptionController;

    private CharacterSheet currentCharacter = null;

    #endregion

    #region Public Methods

    public void UpdateCharacter(CharacterSheet sheet)
    {
        currentCharacter = sheet;
        characterDescriptionController.UpdateDescription(sheet);
    }

    public void PickCurrentCharacter()
    {
        controller.SelectCharacter(currentCharacter);
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponentInParent<MenuControllerRoleSelector>();
        }

        if (characterDescriptionController == null)
        {
            characterDescriptionController = GetComponentInChildren<MenuControllerCharacterDescription>();
        }
    }

    private void Start()
    {
        foreach (CharacterSheet sheet in controller.roleSelectionController.characters)
        {
            GameObject portrait = Instantiate(characterPortraitItemPrefab, characterSelectorGrid);
            portrait.GetComponent<ItemControllerCharacterPortrait>().SetContent(sheet, () => {
                UpdateCharacter(sheet);
            });
        }

        UpdateCharacter(controller.roleSelectionController.characters[0]); 
    }

    #endregion
}
