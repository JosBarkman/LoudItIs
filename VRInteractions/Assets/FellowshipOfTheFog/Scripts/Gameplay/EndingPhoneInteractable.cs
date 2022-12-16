using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.XR.Interaction.Toolkit;

public class EndingPhoneInteractable : XRSimpleInteractable
{
    #region Properties

    [SerializeField]
    private EndingSequenceController endingController;

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (endingController == null)
        {
            endingController = FindObjectOfType<EndingSequenceController>();
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        NetworkObject obj = args.interactorObject.transform.GetComponentInParent<NetworkObject>();
        
        if (obj == null)
        {
            return;
        }

        endingController.Vote(obj.InputAuthority);
    }

    #endregion
}
