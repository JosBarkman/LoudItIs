using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemControllerGoal : MonoBehaviour
{
    #region Properties

    [SerializeField]
    private Text goalText;

    #endregion

    #region Public Methods

    public void SetContent(string content)
    {
        goalText.text = "- " + content;
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (goalText == null)
        {
            goalText = GetComponentInChildren<Text>();
        }
    }

    #endregion
}
