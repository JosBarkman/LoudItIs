using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DoorLock : NetworkBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField]
    private GameObject key;

    [SerializeField]
    private bool locked;

    [HideInInspector]
    [Networked(OnChanged = "OnOpenChanegd", OnChangedTargets = OnChangedTargets.All)]
    public NetworkBool open { get; set; }

    [SerializeField]
    private HandGrabable doorHandling = null;

    #endregion

    #region Public Methods

    public void UnlockDoor()
    {
        if (Runner.IsServer)
        {
            open = true;
        }
    }

    public void LockDoor()
    {
        if (Runner.IsServer)
        {
            open = false;
        }
    }

    #endregion

    #region Private Methods

    private void SetUnlocked()
    {
        doorHandling.enabled = true;
    }

    private void SetLocked()
    {
        doorHandling.enabled = false;
    }

    #endregion

    #region Unity Events

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == key)
        {
            UnlockDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == key)
        {
            UnlockDoor();
        }
    }

    #endregion

    #region Fusion Events

    public override void Spawned()
    {
        if (locked)
        {
            LockDoor();
        }

        if (doorHandling == null)
        {
            HandGrabable[] doors = GetComponentsInChildren<HandGrabable>();

            for (int i = 0; i < doors.Length && doorHandling == null; i++)
            {
                if (doors[i].doorHandling)
                {
                    doorHandling = doors[i];
                }
            }
        }

        // In case that some joins the saesion later
        if (open)
        {
            SetUnlocked();
        }
        else
        {
            SetLocked();
        }
    }

    #endregion

    #region Property Changed Callbacks

    public static void OnOpenChanegd(Changed<DoorLock> changed)
    {
        if (changed.Behaviour.open)
        {
            changed.Behaviour.SetUnlocked();
        }
        else
        {
            changed.Behaviour.SetLocked();
        }        
    }

    #endregion
}
