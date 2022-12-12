using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockDoorTimedEvent : TimedEvent
{
    #region Properties

    public DoorLock door;

    #endregion

    public override void DefaultState() { }

    public override void Execute()
    {
        door.UnlockDoor();
    }
}
