using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class TimedEvent : MonoBehaviour
{
    #region Properties

    public float seconds;

    #endregion

    #region Public Methods

    public abstract void Execute();
    public abstract void DefaultState();

    #endregion
}
