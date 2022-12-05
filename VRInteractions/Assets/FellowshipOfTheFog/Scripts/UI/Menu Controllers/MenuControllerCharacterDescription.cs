using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControllerCharacterDescription : MonoBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField]
    private GameObject goalItemPrefab;

    [Header("Components")]
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

    private List<ItemControllerGoal> goalItemList = new List<ItemControllerGoal>();

    #endregion

    #region Unity Events

    #endregion

    #region Public Methods

    public void UpdateDescription(CharacterSheet sheet)
    {
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

    #endregion
}
