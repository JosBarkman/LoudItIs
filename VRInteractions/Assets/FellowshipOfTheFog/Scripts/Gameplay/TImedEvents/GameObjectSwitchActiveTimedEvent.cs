using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSwitchActiveTimedEvent : TimedEvent
{
    #region Properties

    public GameObject theObject;
    public bool activeToDisabled;

    #endregion

    public override void DefaultState() 
    {
        theObject.SetActive(activeToDisabled ? true : false);
    }

    public override void Execute()
    {
        theObject.SetActive(!theObject.activeInHierarchy);
    }

    public override void UpdateEvent(float remainignTime)
    {
        
    }
}
