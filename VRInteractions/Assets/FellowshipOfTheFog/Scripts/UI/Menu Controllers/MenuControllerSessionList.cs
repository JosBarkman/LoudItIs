using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControllerSessionList : MonoBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField]
    private GameObject sessionItemPrefab;

    [Header("Components")]
    [SerializeField]
    private Transform content;

    [Header("External controllers")]
    [SerializeField]
    private MenuControllerMainMenu mainMenuController;

    private List<ItemControllerSession> sessionItemsList = null;

    #endregion

    #region Public Methods

    public void UpdateSessionList(List<SessionInfo> sessions)
    {
        int i = 0;
        // Update existing items
        for (; i < sessionItemsList.Count && i < sessions.Count; i++)
        {
            sessionItemsList[i].gameObject.SetActive(true);
            sessionItemsList[i].SetContent(this, sessions[i]);
        }

        // Add new nedded items
        for (; i < sessions.Count; i++)
        {
            GenerateSessionItem(sessions[i]);
        }

        // Hide unused existing items
        for (i = sessionItemsList.Count; i > sessions.Count; i--)
        {
            sessionItemsList[i - 1].gameObject.SetActive(false);
        }
    }

    public void Join(SessionInfo info)
    {
        mainMenuController.JoinSession(info);
    }

    #endregion

    #region Private Methods

    private void GenerateSessionItem(SessionInfo sessionInfo)
    {
        GameObject instance = Instantiate(sessionItemPrefab, content);
        ItemControllerSession sessionItem = instance.GetComponent<ItemControllerSession>();

        sessionItem.SetContent(this, sessionInfo);
        sessionItemsList.Add(sessionItem);
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        sessionItemsList = new List<ItemControllerSession>();

        if (mainMenuController == null)
        {
            mainMenuController = FindObjectOfType<MenuControllerMainMenu>();
        }
    }

    #endregion
}
