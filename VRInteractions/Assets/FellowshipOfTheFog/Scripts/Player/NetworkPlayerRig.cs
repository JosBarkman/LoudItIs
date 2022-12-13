using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Voice.Unity;

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

    [HideInInspector]
    [Networked(OnChanged = "OnShowingMapChanged", OnChangedTargets = OnChangedTargets.All)] public NetworkBool showingMap { get; set; }

    [HideInInspector]
    [Networked(OnChanged = "OnMapFloorChanged", OnChangedTargets = OnChangedTargets.All)] public NetworkBool mapFloor { get; set; }

    [HideInInspector, Capacity(32)]
    [Networked(OnChanged = "OnCharacterNameChanged", OnChangedTargets = OnChangedTargets.InputAuthority)] public string characterName { get; set; }

    private NetworkObject leftHandSelectedObject = null;
    private NetworkObject rightHandSelectedObject = null;

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

    [SerializeField]
    private GameplayMapController gameplayMapController;

    [SerializeField]
    private Canvas characterCanvas;

    [SerializeField]
    private Image talkingIcon;

    public FingersIK leftFingers;
    public FingersIK rightFingers;

    [SerializeField]
    private Transform rigVisuals;

    // This variables are only useful in the machine of the player controling this rig
    private LocalPlayerRig playerRig;
    private Speaker speaker;
    public CharacterSheet sheet;

    [Header("IK Contraints")]
    [SerializeField]
    private IKConstraint leftHandConstraint;
    [SerializeField]
    private IKConstraint rightHandConstraint;
    [SerializeField]
    private IKConstraint headConstraint;

    private NetworkManager manager;
    
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
            manager = FindObjectOfType<NetworkManager>();

            rigVisuals.gameObject.SetActive(false);

            playerRig.leftHandIndexConstraint.track = leftHandIndexConstraint.target;
            playerRig.leftHandMiddleConstraint.track = leftHandMiddleConstraint.target;
            playerRig.leftHandRingConstraint.track = leftHandRingConstraint.target;
            playerRig.leftHandPinkyConstraint.track = leftHandPinkyConstraint.target;
            playerRig.leftHandThumbConstraint.track = leftHandThumbConstraint.target;            

            playerRig.rightHandIndexConstraint.track = rightHandIndexConstraint.target;
            playerRig.rightHandMiddleConstraint.track = rightHandMiddleConstraint.target;
            playerRig.rightHandRingConstraint.track = rightHandRingConstraint.target;
            playerRig.rightHandPinkyConstraint.track = rightHandPinkyConstraint.target;
            playerRig.rightHandThumbConstraint.track = rightHandThumbConstraint.target;
        }

        speaker = GetComponentInParent<Speaker>();

        if (gameplayMapController == null)
        {
            gameplayMapController = GetComponentInChildren<GameplayMapController>();
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

            // HEHE
            transform.parent.parent.position = headset.transform.position + Vector3.up * networkedHeadFeetOffset;
            transform.parent.parent.forward = Vector3.ProjectOnPlane(headset.transform.forward, Vector3.up);

            // We update server controller state so that he is the one selecting the objects
            if (Runner.IsServer)
            {
                // Left controller
                XRControllerState leftControllerState = new XRControllerState();
                leftControllerState.selectInteractionState = new InteractionState();

                leftControllerState.selectInteractionState.active = ((byte)(input.leftControllerButtonsPressed & (byte)RigInput.VrControllerButtons.Trigger)) == (byte)RigInput.VrControllerButtons.Trigger;

                leftHandXRController.currentControllerState = leftControllerState;

                // Update left hand state
                byte fingersState = (byte)FingerIKFlags.None;

                IXRSelectInteractable selectedInteractable = leftHandInteractor.firstInteractableSelected;
                if (selectedInteractable != null)
                {
                    HandGrabable grababble = selectedInteractable as HandGrabable;

                    if (grababble != null)
                    {
                        fingersState |= grababble.leftHandFignersPosition.indexIKPosition != null ? (byte)FingerIKFlags.Index : (byte)FingerIKFlags.None;
                        fingersState |= grababble.leftHandFignersPosition.middleIKPosition != null ? (byte)FingerIKFlags.Middle : (byte)FingerIKFlags.None;
                        fingersState |= grababble.leftHandFignersPosition.ringIKPosition != null ? (byte)FingerIKFlags.Ring : (byte)FingerIKFlags.None;
                        fingersState |= grababble.leftHandFignersPosition.pinkyIKPosition != null ? (byte)FingerIKFlags.Pinky : (byte)FingerIKFlags.None;
                        fingersState |= grababble.leftHandFignersPosition.thumbIKPosition != null ? (byte)FingerIKFlags.Thumb : (byte)FingerIKFlags.None;

                        if (grababble.leftHandFignersPosition.indexIKPosition != null)
                        {
                            leftHandIndexConstraint.track = grababble.leftHandFignersPosition.indexIKPosition;
                        }

                        if (grababble.leftHandFignersPosition.middleIKPosition != null)
                        {
                            leftHandMiddleConstraint.track = grababble.leftHandFignersPosition.middleIKPosition;
                        }

                        if (grababble.leftHandFignersPosition.ringIKPosition != null)
                        {
                            leftHandRingConstraint.track = grababble.leftHandFignersPosition.ringIKPosition;
                        }

                        if (grababble.leftHandFignersPosition.pinkyIKPosition != null)
                        {
                            leftHandPinkyConstraint.track = grababble.leftHandFignersPosition.pinkyIKPosition;
                        }

                        if (grababble.leftHandFignersPosition.thumbIKPosition != null)
                        {
                            leftHandThumbConstraint.track = grababble.leftHandFignersPosition.thumbIKPosition;
                        }

                        leftHandState = fingersState;
                    }

                    leftHandSelectedObject = selectedInteractable.transform.GetComponentInParent<NetworkObject>();

                    if (leftHandSelectedObject != null && selectedInteractable.transform.GetComponent<Memories>() != null)
                    {
                        RPC_ShowMemoryClue(leftHandSelectedObject.Id);
                    }

                }
                else
                {
                    if (leftHandSelectedObject != null)
                    {
                        RPC_HideMemoryClue(leftHandSelectedObject.Id);
                        leftHandSelectedObject = null;
                    }

                    leftHandState = (byte)FingerIKFlags.None;
                }

                // Update left hand figners state based on the grabbed object

                // Update left hand fingers IK targets
                leftHandIndexConstraint.Update();
                leftHandMiddleConstraint.Update();
                leftHandRingConstraint.Update();
                leftHandPinkyConstraint.Update();
                leftHandThumbConstraint.Update();

                // ---- Right controller ----

                // We just have to select when not showing map
                if (!showingMap)
                {
                    XRControllerState rightControllerState = new XRControllerState();
                    rightControllerState.selectInteractionState = new InteractionState();

                    rightControllerState.selectInteractionState.active = ((byte)(input.rightControllerButtonsPressed & (byte)RigInput.VrControllerButtons.Trigger)) == (byte)RigInput.VrControllerButtons.Trigger;

                    rightHandXRController.currentControllerState = rightControllerState;
                }
                else if (((byte)(input.rightControllerButtonsPressed & (byte)RigInput.VrControllerButtons.Trigger)) == (byte)RigInput.VrControllerButtons.Trigger)
                {
                    mapFloor = !mapFloor;
                }

                // Update right hand state
                selectedInteractable = righttHandInteractor.firstInteractableSelected;
                if (selectedInteractable != null)
                {
                    HandGrabable grababble = selectedInteractable as HandGrabable;

                    if (grababble != null)
                    {
                        fingersState = (byte)FingerIKFlags.None;

                        fingersState |= grababble.rightHandFignersPosition.indexIKPosition != null ? (byte)FingerIKFlags.Index : (byte)FingerIKFlags.None;
                        fingersState |= grababble.rightHandFignersPosition.middleIKPosition != null ? (byte)FingerIKFlags.Middle : (byte)FingerIKFlags.None;
                        fingersState |= grababble.rightHandFignersPosition.ringIKPosition != null ? (byte)FingerIKFlags.Ring : (byte)FingerIKFlags.None;
                        fingersState |= grababble.rightHandFignersPosition.pinkyIKPosition != null ? (byte)FingerIKFlags.Pinky : (byte)FingerIKFlags.None;
                        fingersState |= grababble.rightHandFignersPosition.thumbIKPosition != null ? (byte)FingerIKFlags.Thumb : (byte)FingerIKFlags.None;

                        if (grababble.rightHandFignersPosition.indexIKPosition != null)
                        {
                            rightHandIndexConstraint.track = grababble.rightHandFignersPosition.indexIKPosition;
                        }

                        if (grababble.rightHandFignersPosition.middleIKPosition != null)
                        {
                            rightHandMiddleConstraint.track = grababble.rightHandFignersPosition.middleIKPosition;
                        }

                        if (grababble.rightHandFignersPosition.ringIKPosition != null)
                        {
                            rightHandRingConstraint.track = grababble.rightHandFignersPosition.ringIKPosition;
                        }

                        if (grababble.rightHandFignersPosition.pinkyIKPosition != null)
                        {
                            rightHandPinkyConstraint.track = grababble.rightHandFignersPosition.pinkyIKPosition;
                        }

                        if (grababble.rightHandFignersPosition.thumbIKPosition != null)
                        {
                            rightHandThumbConstraint.track = grababble.rightHandFignersPosition.thumbIKPosition;
                        }

                        rightHandState = fingersState;
                    }

                    rightHandSelectedObject = selectedInteractable.transform.GetComponentInParent<NetworkObject>();

                    if (rightHandSelectedObject != null & selectedInteractable.transform.GetComponent<Memories>())
                    {
                        RPC_ShowMemoryClue(rightHandSelectedObject.Id);
                    }
                }
                else
                {
                    if (rightHandSelectedObject != null)
                    {
                        RPC_HideMemoryClue(rightHandSelectedObject.Id);
                        rightHandSelectedObject = null;
                    }
                    else if (((byte)(input.rightControllerButtonsPressed & (byte)RigInput.VrControllerButtons.Menu)) == (byte)RigInput.VrControllerButtons.Menu)
                    {
                        showingMap = !showingMap;

                        rightHandIndexConstraint.track = gameplayMapController.indexPosition;
                        rightHandMiddleConstraint.track = gameplayMapController.middlePosition;
                        rightHandRingConstraint.track = gameplayMapController.ringPosition;
                        rightHandPinkyConstraint.track = gameplayMapController.pinkyPosition;
                        rightHandThumbConstraint.track = gameplayMapController.thumbPosition;
                    }
                    else
                    {
                        rightHandState = (byte)FingerIKFlags.None;
                    }
                }

                // Update left hand figners state based on the grabbed object

                rightHandIndexConstraint.Update();
                rightHandMiddleConstraint.Update();
                rightHandRingConstraint.Update();
                rightHandPinkyConstraint.Update();
                rightHandThumbConstraint.Update();
            }
        }

        leftHandConstraint.Update();
        rightHandConstraint.Update();
        headConstraint.Update();

        if (speaker.IsPlaying)
        {
            talkingIcon.gameObject.SetActive(true);
        }
        else
        {
            talkingIcon.gameObject.SetActive(false);
        }

        characterCanvas.transform.LookAt(Camera.main.transform, Vector3.up);
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

    public static void OnShowingMapChanged(Changed<NetworkPlayerRig> changed)
    {
        if (changed.Behaviour.showingMap)
        {
            changed.Behaviour.gameplayMapController.ShowMap();

            NetworkRunner runner = FindObjectOfType<NetworkRunner>();

            if (runner.IsServer || changed.Behaviour.HasInputAuthority)
            {
                if (runner.IsServer)
                {
                    byte fingersState = (byte)FingerIKFlags.Index | (byte)FingerIKFlags.Middle | (byte)FingerIKFlags.Ring
                    | (byte)FingerIKFlags.Pinky | (byte)FingerIKFlags.Thumb;

                    changed.Behaviour.rightHandState = fingersState;
                }
            }
        }
        else
        {
            changed.Behaviour.gameplayMapController.HideMap();

            byte fingersState = (byte)FingerIKFlags.None;
            changed.Behaviour.rightHandState = fingersState;
        }        
    }

    public static void OnMapFloorChanged(Changed<NetworkPlayerRig> changed)
    {
        if (changed.Behaviour.showingMap)
        {
            changed.Behaviour.gameplayMapController.SwitchFloor();
        }
    }
    public static void OnCharacterNameChanged(Changed<NetworkPlayerRig> changed)
    {
        // This should be always true because input authority should always have local rig
        if (changed.Behaviour.playerRig != null)
        {
            CharacterSheet sheet = changed.Behaviour.manager.characters.Find(x => x.name == changed.Behaviour.characterName);
            changed.Behaviour.playerRig.SetCharacter(sheet, false);
        }
    }

    #endregion

    #region RPCs

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_ShowMemoryClue(NetworkId selectedObject)
    {
        Memories memories = Runner.FindObject(selectedObject).GetComponentInChildren<Memories>();

        if (memories == null)
        {
            return;
        }

        memories.ShowMemory(playerRig.GetCharacter());
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_HideMemoryClue(NetworkId selectedObject)
    {
        Memories memories = Runner.FindObject(selectedObject).GetComponentInChildren<Memories>();

        if (memories == null)
        {
            return;
        }

        memories.HideMemory();
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_TeleportAndLock(Vector3 location, Quaternion rotation)
    {
        // Just doublecheck
        if (playerRig == null)
        {
            return;
        }

        playerRig.Mute();
        playerRig.TeleportAndLock(location, rotation);
        playerRig.ShowNotification("Wait for your turn to speak");
    }

    #endregion

}
