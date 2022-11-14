using Fusion;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Interaction.Toolkit;
using static FingerGrababble;

[System.Serializable]
public class IKConstraint
{
    public Transform track = null;
    public Transform target = null;

    [SerializeField]
    private Vector3 positionOffset = Vector3.zero;
    [SerializeField]
    private Vector3 rotationOffeset = Vector3.zero;

    public void Update()
    {
        if (track == null || target == null)
        {
            return;
        }

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
    [Networked(OnChanged = "OnLeftHandStateChanegd", OnChangedTargets = OnChangedTargets.All)] public byte leftHandState { get; set; }

    [HideInInspector]
    [Networked(OnChanged = "OnRightHandStateChanged", OnChangedTargets = OnChangedTargets.All)] public byte rightHandState { get; set; }

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
    private XRBaseControllerInteractor leftHandInteractor;

    [SerializeField]
    private ActionBasedController rightHandXRController;

    [SerializeField]
    private XRBaseControllerInteractor righttHandInteractor;

    public FingersIK leftFingers;
    public FingersIK rightFingers;

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
    
    public IKConstraint leftHandIndexConstraint = new IKConstraint();
    public IKConstraint leftHandMiddleConstraint = new IKConstraint();
    public IKConstraint leftHandRingConstraint = new IKConstraint();
    public IKConstraint leftHandPinkyConstraint = new IKConstraint();
    public IKConstraint leftHandThumbConstraint = new IKConstraint();
    
    public IKConstraint rightHandIndexConstraint = new IKConstraint();
    public IKConstraint rightHandMiddleConstraint = new IKConstraint();
    public IKConstraint rightHandRingConstraint = new IKConstraint();
    public IKConstraint rightHandPinkyConstraint = new IKConstraint();
    public IKConstraint rightHandThumbConstraint = new IKConstraint();

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (leftHandXRController == null)
        {
            leftHandXRController = leftHand.GetComponentInChildren<ActionBasedController>();
        }

        if (leftHandInteractor == null)
        {
            leftHandInteractor = leftHand.GetComponentInChildren<XRBaseControllerInteractor>();
        }

        if (rightHandXRController == null)
        {
            rightHandXRController = rightHand.GetComponentInChildren<ActionBasedController>();
        }

        if (righttHandInteractor == null)
        {
            righttHandInteractor = rightHand.GetComponentInChildren<XRBaseControllerInteractor>();
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
        // if true means that we are on the server (actually this is not true)
        // client with InputAuthority can run this code too
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

            // We update server controller state so that he is the one selecting the objects
            if (Runner.IsServer)
            {
                // Left controller
                XRControllerState leftControllerState = new XRControllerState();
                leftControllerState.selectInteractionState = new InteractionState();

                leftControllerState.selectInteractionState.active = ((byte) (input.leftControllerButtonsPressed & (byte) RigInput.VrControllerButtons.Trigger)) == (byte) RigInput.VrControllerButtons.Trigger;

                leftHandXRController.currentControllerState = leftControllerState;

                // Update left hand state
                byte fingersState = (byte) FingerIKFlags.None;

                IXRSelectInteractable selectedInteractable = leftHandInteractor.firstInteractableSelected;
                if (selectedInteractable != null)
                {
                    FingerGrababble grababble = selectedInteractable.colliders[0].GetComponentInParent<FingerGrababble>();

                    fingersState |= grababble.indexIKPosition != null ? (byte) FingerIKFlags.Index : (byte) FingerIKFlags.None;
                    fingersState |= grababble.middleIKPosition != null ? (byte) FingerIKFlags.Middle : (byte) FingerIKFlags.None;
                    fingersState |= grababble.ringIKPosition != null ? (byte) FingerIKFlags.Ring : (byte) FingerIKFlags.None;
                    fingersState |= grababble.pinkyIKPosition != null ? (byte) FingerIKFlags.Pinky : (byte) FingerIKFlags.None;
                    fingersState |= grababble.thumbIKPosition != null ? (byte) FingerIKFlags.Thumb : (byte) FingerIKFlags.None;

                    if (grababble.indexIKPosition != null)
                    {
                        leftHandIndexConstraint.track = grababble.indexIKPosition;
                    }

                    if (grababble.middleIKPosition != null)
                    {
                        leftHandMiddleConstraint.track = grababble.middleIKPosition;
                    }

                    if (grababble.ringIKPosition != null)
                    {
                        leftHandRingConstraint.track = grababble.ringIKPosition;
                    }

                    if (grababble.pinkyIKPosition != null)
                    {
                        leftHandPinkyConstraint.track = grababble.pinkyIKPosition;
                    }

                    if (grababble.thumbIKPosition != null)
                    {
                        leftHandThumbConstraint.track = grababble.thumbIKPosition;
                    }
                }

                // Update left hand figners state based on the grabbed object
                leftHandState = fingersState;

                // Update left hand fingers IK targets
                leftHandIndexConstraint.Update();
                leftHandMiddleConstraint.Update();
                leftHandRingConstraint.Update();
                leftHandPinkyConstraint.Update();
                leftHandThumbConstraint.Update();

                if (playerRig != null)
                {
                    playerRig.leftFingers.UpdateTargets(
                        leftHandIndexConstraint.track != null ? leftHandIndexConstraint.track.position : Vector3.zero,
                        leftHandMiddleConstraint.track != null ? leftHandMiddleConstraint.track.position : Vector3.zero,
                        leftHandRingConstraint.track != null ? leftHandRingConstraint.track.position : Vector3.zero,
                        leftHandPinkyConstraint.track != null ? leftHandPinkyConstraint.track.position : Vector3.zero,
                        leftHandThumbConstraint.track != null ? leftHandThumbConstraint.track.position : Vector3.zero);
                }

                // ---- Right controller ----
                XRControllerState rightControllerState = new XRControllerState();
                rightControllerState.selectInteractionState = new InteractionState();

                rightControllerState.selectInteractionState.active = ((byte) (input.rightControllerButtonsPressed & (byte) RigInput.VrControllerButtons.Trigger)) == (byte) RigInput.VrControllerButtons.Trigger;

                rightHandXRController.currentControllerState = rightControllerState;

                // Update right hand state
                fingersState = (byte) FingerIKFlags.None;

                selectedInteractable = righttHandInteractor.firstInteractableSelected;
                if (selectedInteractable != null)
                {
                    FingerGrababble grababble = selectedInteractable.colliders[0].GetComponentInParent<FingerGrababble>();
                    fingersState |= grababble.indexIKPosition != null ? (byte)FingerIKFlags.Index : (byte)FingerIKFlags.None;
                    fingersState |= grababble.middleIKPosition != null ? (byte)FingerIKFlags.Middle : (byte)FingerIKFlags.None;
                    fingersState |= grababble.ringIKPosition != null ? (byte)FingerIKFlags.Ring : (byte)FingerIKFlags.None;
                    fingersState |= grababble.pinkyIKPosition != null ? (byte)FingerIKFlags.Pinky : (byte)FingerIKFlags.None;
                    fingersState |= grababble.thumbIKPosition != null ? (byte)FingerIKFlags.Thumb : (byte)FingerIKFlags.None;

                    if (grababble.indexIKPosition != null)
                    {
                        rightHandIndexConstraint.track = grababble.indexIKPosition;
                    }

                    if (grababble.middleIKPosition != null)
                    {
                        rightHandMiddleConstraint.track = grababble.middleIKPosition;
                    }

                    if (grababble.ringIKPosition != null)
                    {
                        rightHandRingConstraint.track = grababble.ringIKPosition;
                    }

                    if (grababble.pinkyIKPosition != null)
                    {
                        rightHandPinkyConstraint.track = grababble.pinkyIKPosition;
                    }

                    if (grababble.thumbIKPosition != null)
                    {
                        rightHandThumbConstraint.track = grababble.thumbIKPosition;
                    }
                }

                // Update left hand figners state based on the grabbed object
                rightHandState = fingersState;

                rightHandIndexConstraint.Update();
                rightHandMiddleConstraint.Update();
                rightHandRingConstraint.Update();
                rightHandPinkyConstraint.Update();
                rightHandThumbConstraint.Update();

                if (playerRig != null)
                {
                    playerRig.rightFingers.UpdateTargets(
                        rightHandIndexConstraint.track != null ? rightHandIndexConstraint.track.position : Vector3.zero,
                        rightHandMiddleConstraint.track != null ? rightHandMiddleConstraint.track.position : Vector3.zero,
                        rightHandRingConstraint.track != null ? rightHandRingConstraint.track.position : Vector3.zero,
                        rightHandPinkyConstraint.track != null ? rightHandPinkyConstraint.track.position : Vector3.zero,
                        rightHandThumbConstraint.track != null ? rightHandThumbConstraint.track.position : Vector3.zero);
                }
            }
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
        changed.Behaviour.leftFingers.UpdateWeights(changed.Behaviour.leftHandState);

        // Player rig will be just valid on the local client
        if (changed.Behaviour.playerRig != null)
        {
            changed.Behaviour.playerRig.UpdateLeftHandContraint(changed.Behaviour.leftHandState);
        }
    }

    public static void OnRightHandStateChanged(Changed<NetworkPlayerRig> changed)
    {
        changed.Behaviour.rightFingers.UpdateWeights(changed.Behaviour.rightHandState);

        // Player rig will be just valid on the local client
        if (changed.Behaviour.playerRig != null)
        {
            changed.Behaviour.playerRig.UpdateRightHandConstraint(changed.Behaviour.rightHandState);
        }
    }

    #endregion

}
