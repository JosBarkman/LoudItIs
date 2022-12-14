using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNotificationProvider : MonoBehaviour
{
    #region Properties

    [Header("Components")]
    [SerializeField] private Text notificationText;
    [SerializeField] private Text timerText;

    #endregion

    #region Public Methods

    public void Show(string notification, bool timer, float? seconds)
    {
        gameObject.SetActive(true);
        notificationText.text = notification;
        timerText.gameObject.SetActive(timer);
        UpdateTimer(seconds);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateTimer(float? seconds)
    {
        timerText.text = seconds.HasValue ? ((int)seconds).ToString() : "0";
    }

    #endregion
}
