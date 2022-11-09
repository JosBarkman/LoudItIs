using Fusion;
using UnityEngine;
using UnityEngine.Animations.Rigging;
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

    [HideInInspector]
    [Networked(OnChanged = "OnLeftHandStateChanegd", OnChangedTargets = OnChangedTargets.All)] public NetworkBool leftHandState { get; set; }

    [HideInInspector]
    [Networked(OnChanged = "OnRightHandStateChanged", OnChangedTargets = OnChangedTargets.All)] public NetworkBool rightHandState { get; set; }

    [Header("Components")]
    [SerializeField]
    private NetworkTransform headset;

    [SerializeField]
    private NetworkTransform leftHand;

    [SerializeField]
    private NetworkTransform rightHand;

    [SerializeField]
    private ActionBasedController leftHandXRController;

    [SerializeField]
    private ActionBasedController rightHandXRController;

    public Rig leftHandRigContraints;
    public Rig rightHandRigContraints;

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

    #region Unity Events

    private void Awake()
    {
        if (leftHandXRController == null)
        {
            leftHandXRController = leftHand.GetComponentInChildren<ActionBasedController>();
        }

        if (rightHandXRController == null)
        {
            rightHandXRController = rightHand.GetComponentInChildren<ActionBasedController>();
        }
    }

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

            // Left controller
            XRControllerState leftControllerState = new XRControllerState();
            leftControllerState.selectInteractionState = new InteractionState();

            leftControllerState.selectInteractionState.active = ((byte) (input.leftControllerButtonsPressed & (byte) RigInput.VrControllerButtons.Trigger)) == (byte) RigInput.VrControllerButtons.Trigger;

            leftHandXRController.currentControllerState = leftControllerState;

            // Update left hand state
            leftHandState = leftControllerState.selectInteractionState.active;

            // Right controller
            XRControllerState rightControllerState = new XRControllerState();
            rightControllerState.selectInteractionState = new InteractionState();

            rightControllerState.selectInteractionState.active = ((byte)(input.rightControllerButtonsPressed & (byte)RigInput.VrControllerButtons.Trigger)) == (byte)RigInput.VrControllerButtons.Trigger;

            rightHandXRController.currentControllerState = rightControllerState;

            // Update right hand state
            rightHandState = leftControllerState.selectInteractionState.active;
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

    #region Property Changed Callbacks

    public static void OnLeftHandStateChanegd(Changed<NetworkPlayerRig> changed)
    {
        changed.Behaviour.leftHandRigContraints.weight = changed.Behaviour.leftHandState ? 1.0f : 0.0f;

        // Player rig will be just valid on the local client
        if (changed.Behaviour.playerRig != null)
        {
            changed.Behaviour.playerRig.UpdateLeftHandContraint(changed.Behaviour.leftHandState);
        }
    }

    public static void OnRightHandStateChanged(Changed<NetworkPlayerRig> changed)
    {
        changed.Behaviour.rightHandRigContraints.weight = changed.Behaviour.rightHandState ? 1.0f : 0.0f;

        // Player rig will be just valid on the local client
        if (changed.Behaviour.playerRig != null)
        {
            changed.Behaviour.playerRig.UpdateRightHandConstraint(changed.Behaviour.rightHandState);
        }
    }

    #endregion

}
