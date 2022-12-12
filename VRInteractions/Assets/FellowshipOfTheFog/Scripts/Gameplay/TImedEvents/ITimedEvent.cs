using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimedEvent
{
    #region Properties

    public float seconds { get; set; }

    #endregion

    #region Methods

    void Execute();
    void DefaultState();

    #endregion
}
