using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class IKConstraint
{
    public Transform track;
    public Transform target;

    [SerializeField]
    private Vector3 positionOffset;
    [SerializeField]
    private Vector3 rotationOffeset;

    public void Update()
    {
        Vector3 movement = Vector3.Scale(-track.forward.normalized, new Vector3(1.0f, 0.0f, 1.0f)) * positionOffset.z;        

        target.position = track.position + movement;
        target.rotation = track.rotation * Quaternion.Euler(rotationOffeset);
    }
}

[OrderAfter(typeof(NetworkTransform), typeof(NetworkRigidbody))]
public class NetworkPlayerRig : NetworkBehaviour
{

    #region Properties

    [Header("Settings")]
    public float headFeetOffset;

    [HideInInspector]
    [Networked()] public float networkedHeadFeetOffset { get; set; }

    [Header("Components")]
    [SerializeField]
    private NetworkTransform headset;

    [SerializeField]
    private NetworkTransform leftHand;

    [SerializeField]
    private NetworkTransform rightHand;

    [SerializeField]
    private Transform rigVisuals;

    private LocalPlayerRig playerRig;

    [Header("IK Contraints")]
    [SerializeField]
    private IKConstraint leftHandConstraint;
    [SerializeField]
    private IKConstraint rightHandConstraint;
    [SerializeField]
    private IKConstraint headConstraint;

    #endregion

    #region Fusion Event Functions

    public override void Spawned()
    {
        base.Spawned();
        if (Object.HasInputAuthority)
        {
            playerRig = FindObjectOfType<LocalPlayerRig>();

            rigVisuals.gameObject.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        // update the rig at each network tick
        // if true means that we are on the server
        if (GetInput<RigInput>(out var input))
        {
            leftHand.transform.position = input.leftHandPosition;
            leftHand.transform.rotation = input.leftHandRotation;

            rightHand.transform.position = input.rightHandPosition;
            rightHand.transform.rotation = input.rightHandRotation;

            headset.transform.position = input.headsetPosition;
            headset.transform.rotation = input.headsetRotation;

            transform.position = headset.transform.position + Vector3.up * networkedHeadFeetOffset;
            transform.forward = Vector3.ProjectOnPlane(headset.transform.forward, Vector3.up);
        }

        leftHandConstraint.Update();
        rightHandConstraint.Update();
        headConstraint.Update();
    }

    public override void Render()
    {
        base.Render();
        if (Object.HasInputAuthority)
        {
            leftHand.InterpolationTarget.position = playerRig.leftHand.transform.position;
            leftHand.InterpolationTarget.rotation = playerRig.leftHand.transform.rotation;
            leftHandConstraint.Update();

            rightHand.InterpolationTarget.position = playerRig.rightHand.transform.position;
            rightHand.InterpolationTarget.rotation = playerRig.rightHand.transform.rotation;

            headset.InterpolationTarget.position = playerRig.headset.transform.position;
            headset.InterpolationTarget.rotation = playerRig.headset.transform.rotation;
        }
    }

    #endregion

}
