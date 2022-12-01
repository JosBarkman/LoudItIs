using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MenuControllerMainMenu : MonoBehaviour
{
    #region Properties

    [Header("External controllers")]
    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private MenuControllerSessionList sessionListController;

    [Header("External components")]
    [SerializeField]
    private GameObject defaultMenu;

    [SerializeField]
    private GameObject vrMenu;

    #endregion

    #region Public Methods

    public void HostSession(string text, int maxCharacters)
    {
        networkManager.HostSession(text, maxCharacters);
    }

    public void JoinSession(SessionInfo sessionInfo)
    {
        networkManager.Join(sessionInfo);
    }

    #endregion

    #region Unity Events

    void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }

        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);

        if (displaySubsystems.Count != 0)
        {
            vrMenu.SetActive(true);
            defaultMenu.SetActive(true);
        }
        else
        {
            vrMenu.SetActive(false);
            defaultMenu.SetActive(true);
        }

        if (sessionListController == null)
        {
            sessionListController = vrMenu.activeInHierarchy ? 
                vrMenu.GetComponentInChildren<MenuControllerSessionList>() : 
                defaultMenu.GetComponentInChildren<MenuControllerSessionList>();
        }

        networkManager.OnSessionListUpdatedEvent += sessionListController.UpdateSessionList;
    }

    private void OnDestroy()
    {
        networkManager.OnSessionListUpdatedEvent -= sessionListController.UpdateSessionList;        
    }

    private void OnEnable()
    {
        if (networkManager != null)
        {
            networkManager.OnSessionListUpdatedEvent += sessionListController.UpdateSessionList;
        }
    }

    private void OnDisable()
    {
        networkManager.OnSessionListUpdatedEvent -= sessionListController.UpdateSessionList;
    }

    #endregion
}
