using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockDoorTimedEvent : TimedEvent
{
    #region Properties

    public DoorLock doorLock;

    #endregion

    public override void DefaultState() {}

    public override void Execute()
    {
        doorLock.UnlockDoor();
    }

    public override void UpdateEvent(float remainingSeconds)
    {
        doorLock.UpdateLockedDoorText("The door is locked, it will be unlocked in: " + ((int)remainingSeconds).ToString() + " seconds, take your time a look at YOUR room");
    }
}
