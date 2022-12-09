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
    private Text characterCommonBioInfo;

    [SerializeField]
    private Text characterPrivateInfo;

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
        characterCommonBioInfo.text = sheet.commonBioInfo;
        
        if (characterPrivateInfo != null)
        {
            characterPrivateInfo.text = sheet.privateInfo;
        }

        characterPortrait.sprite = sheet.portrait;

        if (characterGoalsPanel != null)
        {
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
        }

        // Hack to ensure that the goal list resizes properly, prabably not best solution
        Canvas.ForceUpdateCanvases();

        if (characterGoalsPanel != null)
        {
            goalItemList[0].gameObject.SetActive(false);
            goalItemList[0].gameObject.SetActive(true);
        }

        characterCommonBioInfo.gameObject.SetActive(false);
        characterCommonBioInfo.gameObject.SetActive(true);

        if (characterPrivateInfo != null)
        {
            characterPrivateInfo.gameObject.SetActive(false);
            characterPrivateInfo.gameObject.SetActive(true);
        }
    }

    #endregion
}
