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

    [SerializeField]
    private Text characterName;

    [SerializeField]
    private Text characterNickname;

    [SerializeField]
    private Text characterBackground;

    [SerializeField]
    private Transform characterGoalsPanel;

    [SerializeField]
    private Image characterPortrait;

    [Header("External controllers")]
    [SerializeField]
    private MenuControllerRoleSelector controller;

    private CharacterSheet currentCharacter = null;
    private List<ItemControllerGoal> goalItemList = new List<ItemControllerGoal>();

    #endregion

    #region Public Methods

    public void UpdateCharacter(CharacterSheet sheet)
    {
        currentCharacter = sheet;

        characterName.text = sheet.name.ToUpper();
        characterNickname.text = sheet.nickname.ToUpper();
        characterBackground.text = sheet.background;

        characterPortrait.sprite = sheet.portrait;

        int i = 0;
        // Update existing items
        for (; i < goalItemList.Count && i < sheet.goals.Length; i++)
        {
            goalItemList[i].gameObject.SetActive(true);
            goalItemList[i].SetContent(sheet.goals[i].goal);
        }

        // Add new nedded items
        for (; i < sheet.goals.Length; i++)
        {
            GameObject goal = Instantiate(goalItemPrefab, characterGoalsPanel);
            ItemControllerGoal goalItem = goal.GetComponent<ItemControllerGoal>();
            
            goalItem.SetContent(sheet.goals[i].goal);
            goalItemList.Add(goalItem);
        }

        // Hide unused existing items
        for (i = goalItemList.Count; i > sheet.goals.Length; i--)
        {
            goalItemList[i - 1].gameObject.SetActive(false);
        }

        // Hack to ensure that the goal list resizes properly, prabably not best solution
        Canvas.ForceUpdateCanvases();
        goalItemList[0].gameObject.SetActive(false);
        goalItemList[0].gameObject.SetActive(true);
        characterBackground.gameObject.SetActive(false);
        characterBackground.gameObject.SetActive(true);
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
