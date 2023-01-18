using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using UnityEngine.XR.Interaction.Toolkit;

public class EndingPhoneInteractable : XRSimpleInteractable
{
    #region Properties

    [Space]
    [Header("Components")]

    [SerializeField]
    private EndingSequenceController endingController;

    [SerializeField]
    private Text voteNotification;

    #endregion

    #region Public Methods

    public void NotifyVote(bool voted)
    {
        voteNotification.text = "You " + (voted ? "HAVE" : "HAVEN'T") + " voted";
    }

    #endregion

    #region Unity Events

    protected override void Awake()
    {
        base.Awake();

        if (endingController == null)
        {
            endingController = FindObjectOfType<EndingSequenceController>();
        }
    }

    private void Start()
    {
        NotifyVote(false);
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
