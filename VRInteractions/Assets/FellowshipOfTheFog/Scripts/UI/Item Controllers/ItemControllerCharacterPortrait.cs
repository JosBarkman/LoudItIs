using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ItemControllerCharacterPortrait : MonoBehaviour
{
    #region Properties

    [Header("Components")]
    
    [SerializeField]
    private Image portrait;
    
    [SerializeField]
    private Button button;

    #endregion

    #region Public Methods

    public void SetContent(CharacterSheet sheet, UnityAction action)
    {
        portrait.sprite = sheet.portrait;
        button.onClick.AddListener(action);
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (portrait == null)
        {
            portrait = GetComponent<Image>();
        }
    }

    #endregion
}
