using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Fusion;

public class ItemControllerSession : MonoBehaviour
{
    #region Properties

    [Header("Components")]
    [SerializeField]
    private Text sessionName;

    [SerializeField]
    private Text players;

    [SerializeField]
    private Button joinButton;

    private SessionInfo sessionInfo;
    private MenuControllerSessionList controller;

    #endregion

    #region Public Methods

    public void SetContent(MenuControllerSessionList controller, SessionInfo sessionInfo)
    {
        this.controller = controller;
        this.sessionInfo = sessionInfo;

        sessionName.text = sessionInfo.Name;
        players.text = sessionInfo.PlayerCount + "/" + sessionInfo.MaxPlayers;

        joinButton.interactable = sessionInfo.PlayerCount != sessionInfo.MaxPlayers;
        joinButton.onClick.AddListener(Join);
    }

    #endregion

    #region Private Methods

    private void Join()
    {
        controller.Join(sessionInfo);
    }

    #endregion
}
