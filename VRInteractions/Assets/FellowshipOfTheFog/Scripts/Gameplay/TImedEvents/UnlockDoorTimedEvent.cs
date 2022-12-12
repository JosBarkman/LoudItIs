using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockDoorTimedEvent : MonoBehaviour, ITimedEvent
{
    #region Properties

    public float seconds { get; set; }
    public DoorLock door;

    #endregion

    public void DefaultState() { }

    public void Execute()
    {
        door.UnlockDoor();
    }
}
