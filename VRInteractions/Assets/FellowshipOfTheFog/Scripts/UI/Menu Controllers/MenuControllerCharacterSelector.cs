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

    [SerializeField]
    private Material disabledCharacterMaterial;

    [Header("Components")]
    [SerializeField]
    private Transform characterSelectorGrid;

    [Header("External controllers")]
    [SerializeField]
    private MenuControllerRoleSelector controller;

    [SerializeField]
    private MenuControllerCharacterDescription characterDescriptionController;

    private CharacterSheet currentCharacter = null;
    private NetworkManager manager;
    private Dictionary<string, ItemControllerCharacterPortrait> portraits = new Dictionary<string, ItemControllerCharacterPortrait>();

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

    public void DisableCharacter(CharacterSheet sheet)
    {
        portraits[sheet.name].SetDisabled(disabledCharacterMaterial);
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

        manager = FindObjectOfType<NetworkManager>();
    }

    private void Start()
    {
        foreach (CharacterSheet sheet in manager.characters)
        {
            GameObject portrait = Instantiate(characterPortraitItemPrefab, characterSelectorGrid);
            ItemControllerCharacterPortrait portraitItem = portrait.GetComponent<ItemControllerCharacterPortrait>();

            portraits.Add(sheet.name, portraitItem);

            portraitItem.SetContent(sheet, () => {
                UpdateCharacter(sheet);
            });
        }

        UpdateCharacter(manager.characters[0]); 
    }

    #endregion
}
