using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControllerHostSession : MonoBehaviour
{
    #region Properties

    [Header("Components")]
    [SerializeField]
    private Button hostSessionButton;

    [SerializeField]
    private InputField sessionName;
    
    [SerializeField]
    private InputField maxCharacters;

    [Header("External controllers")]
    [SerializeField]
    private MenuControllerMainMenu controller;

    #endregion

    #region Public Methods

    private void HostSession()
    {
        if (sessionName.text.Length == 0)
        {
            // TODO: Should tell player that session name can't be empty
            return;
        }

        // TODO: maxCharacters.text might contain non numeric characters which will raise an exception.
        // DISABLED MAX CHARACTERS FOR NOW
        controller.HostSession(sessionName.text, 10);
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (controller == null)
        {
            controller = FindObjectOfType<MenuControllerMainMenu>();
        }
    }

    private void Start()
    {
        hostSessionButton.onClick.AddListener(HostSession);
    }

    #endregion
}
