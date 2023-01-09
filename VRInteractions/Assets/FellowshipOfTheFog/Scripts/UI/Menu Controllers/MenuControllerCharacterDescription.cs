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
    
    [SerializeField]
    private Image characterFullBody;

    [SerializeField]
    private Button pickCharacterButton;

    private List<ItemControllerGoal> goalItemList = new List<ItemControllerGoal>();

    #endregion

    #region Unity Events

    #endregion

    #region Public Methods

    public void UpdateDescription(CharacterSheet sheet)
    {
        if (sheet == null)
        {
            characterName.text = "";
            characterNickname.text = "";
            characterCommonBioInfo.text = "";

            if (characterPrivateInfo != null)
            {
                characterPrivateInfo.text = "";
            }

            if (characterPortrait != null)
            {
                characterPortrait.enabled = false;
            }

            if (characterFullBody != null)
            {
                characterPortrait.enabled = false;
            }

            if (characterGoalsPanel != null)
            {
                for (int i = 0; i < goalItemList.Count; i++)
                {
                    goalItemList[i].gameObject.SetActive(false);
                }
            }

            pickCharacterButton.interactable = false;
        }
        else
        {
            characterName.text = sheet.name.ToUpper();
            characterNickname.text = sheet.nickname.ToUpper();
            characterCommonBioInfo.text = sheet.commonBioInfo;

            if (characterPrivateInfo != null)
            {
                characterPrivateInfo.text = sheet.privateInfo;
            }

            if (characterPortrait != null)
            {
                characterPortrait.enabled = true;
                characterPortrait.sprite = sheet.portrait;
            }

            if (characterFullBody != null)
            {
                characterFullBody.sprite = sheet.fullBody;
            }

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

            pickCharacterButton.interactable = true;

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
    }

    #endregion
}
