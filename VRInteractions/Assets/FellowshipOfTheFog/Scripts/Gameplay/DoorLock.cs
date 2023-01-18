using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Components")]
    [SerializeField]
    private HandGrabable doorHandling = null;

    [SerializeField]
    private Rigidbody body = null;

    [SerializeField]
    private Text lockedDoorText;

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

    public void UpdateLockedDoorText(string text)
    {
        lockedDoorText.text = text;
    }

    #endregion

    #region Private Methods

    private void SetUnlocked() {
        if ( null == doorHandling ) {
            Debug.LogWarning( $"{name} is trying to access HandGrabable variable doorHandling, but it was never set. Searching children..." );
            GetGrabbables();

            if ( null == doorHandling ) {
                Debug.LogWarning( $"{name} No HandGrabable component found in children. Assign one in the inspector or add one." );
                return;
            }
        }

        doorHandling.enabled = true;
        body.isKinematic = false;

        if (lockedDoorText != null)
        {
            lockedDoorText.gameObject.SetActive(false);
        }
    }

    private void SetLocked()
    {
        if ( null == doorHandling ) {
            Debug.LogWarning( $"{name} is trying to access HandGrabable variable doorHandling, but it was never set. Searching children..." );
            GetGrabbables();

            if ( null == doorHandling ) {
                Debug.LogWarning( $"{name} No HandGrabable component found in children. Assign one in the inspector or add one." );
                return;
            }
        }

        doorHandling.enabled = false;
        body.isKinematic = true;

        if (lockedDoorText != null)
        {
            lockedDoorText.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (body == null)
        {
            body = GetComponentInChildren<Rigidbody>();
        }

        if (lockedDoorText == null)
        {
            lockedDoorText = GetComponentInChildren<Text>();
        }
    }

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
            GetGrabbables();
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

    #region Auto-Assign Fields

    private void GetGrabbables() {
        HandGrabable[] doors = GetComponentsInChildren<HandGrabable>();

        bool set = false;
        for ( int i = 0; i < doors.Length && doorHandling == null; i++ ) {
            if ( doors[i].doorHandling ) {
                doorHandling = doors[i];
                if ( set == true ) {
                    Debug.Log( "Additional HandGrabable component detected in children." );
                }
                else {
                    set = true;
                }
            }
        }
    }

    #endregion
}
