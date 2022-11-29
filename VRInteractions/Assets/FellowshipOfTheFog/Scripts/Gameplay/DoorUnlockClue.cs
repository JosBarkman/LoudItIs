using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DoorUnlockClue : NetworkBehaviour
{

    #region Properties

    [SerializeField]
    private HandGrabable keyClue;

    [HideInInspector]
    [Networked(OnChanged = "OnOpenChanegd", OnChangedTargets = OnChangedTargets.All)]
    public NetworkBool open { get; set; }

    #endregion

    #region Unity Events

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == keyClue.gameObject && Runner.IsServer)
        {
            open = true; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == keyClue.gameObject && Runner.IsServer)
        {
            open = true;
        }
    }

    #endregion

    #region Fusion Events

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            open = false;
        }

        // In case that some joins the saesion later
        if (open)
        {
            gameObject.SetActive(false);
        }
    }

    #endregion

    #region Property Changed Callbacks

    public static void OnOpenChanegd(Changed<DoorUnlockClue> changed)
    {
        if (!changed.Behaviour.open)
        {
            return;
        }
        

        changed.Behaviour.gameObject.SetActive(false);
    }

    #endregion

}
