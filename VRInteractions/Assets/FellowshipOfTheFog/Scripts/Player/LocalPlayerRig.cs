using Fusion;
using Fusion.Sockets;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Network input strcut
/// </summary>
public struct RigInput : INetworkInput
{
    public enum VrControllerButtons : byte
    {
        None = 0x0,
        Trigger = 1 << 7,
        Menu = 1 << 6,
    }

    public Vector3 position;
    public Vector3 headsetPosition;
    public Vector3 leftHandPosition;
    public Vector3 rightHandPosition;

    public Quaternion rotation;
    public Quaternion headsetRotation;
    public Quaternion leftHandRotation;
    public Quaternion rightHandRotation;

    public byte leftControllerButtonsPressed;
    public byte rightControllerButtonsPressed;
}

[System.Serializable]
public class FingersIK
{
    public ChainIKConstraint indexIKConstraint;
    public ChainIKConstraint middleIKConstraint;
    public ChainIKConstraint ringIKConstraint;
    public ChainIKConstraint pinkyIKConstraint;
    public ChainIKConstraint thumbIKConstraint;

    public void UpdateWeights(byte state)
    {
        indexIKConstraint.weight = (state & (int) FingerIKFlags.Index) == (int) FingerIKFlags.Index ? 1.0f : 0.0f;
        middleIKConstraint.weight = (state & (int) FingerIKFlags.Middle) == (int) FingerIKFlags.Middle ? 1.0f : 0.0f;
        ringIKConstraint.weight = (state & (int) FingerIKFlags.Ring) == (int) FingerIKFlags.Ring ? 1.0f : 0.0f;
        pinkyIKConstraint.weight = (state & (int) FingerIKFlags.Pinky) == (int) FingerIKFlags.Pinky ? 1.0f : 0.0f;
        thumbIKConstraint.weight = (state & (int) FingerIKFlags.Thumb) == (int) FingerIKFlags.Thumb ? 1.0f : 0.0f;
    }

    public void UpdateTargets(Vector3 indexPosition, Vector3 middlePosition, 
        Vector3 ringPosition, Vector3 pinkyPosition, Vector3 thumbPosition)
    {
        indexIKConstraint.data.target.position = indexPosition;
        middleIKConstraint.data.target.position = middlePosition;
        ringIKConstraint.data.target.position = ringPosition;
        thumbIKConstraint.data.target.position = thumbPosition;
        pinkyIKConstraint.data.target.position = pinkyPosition;
    }
}

/// <summary>
/// Represents the local player, the network player which will be replicated will track this rig.
/// </summary>
public class LocalPlayerRig : MonoBehaviour, INetworkRunnerCallbacks
{
    #region Properties

    [Header("Components")]
    public Transform headset;
    public Transform leftHand;
    public Transform rightHand;

    public XROrigin xrOrigin;

    [SerializeField]
    private TrackedPoseDriver trackedPoseDriver;

    [SerializeField]
    private XRRayInteractor leftHandRayInteractor;

    [SerializeField]
    private XRRayInteractor rightHandRayInteractor;

    [SerializeField]
    private SkinnedMeshRenderer leftHandVisuals;

    [SerializeField]
    private SkinnedMeshRenderer rightHandVisuals;

    [SerializeField]
    private TeleportationProvider teleportationProvider;

    [SerializeField]
    private PlayerNotificationProvider notification;

    [SerializeField]
    private MenuControllerCharacterDescription leftHandCharacterDescription;

    public FingersIK leftFingers;
    public FingersIK rightFingers;  

    [Header("IK Contraints")]
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

    private NetworkRunner runner;
    private UnityEngine.XR.InputDevice leftHardwareController;
    private UnityEngine.XR.InputDevice rightHardwareController;

    private CharacterSheet sheet = null;
   
    private bool lastLeftMenuButtonPressed = false;
    private bool lastRightMenuButtonPressed = false;
    private bool lastRightTriggerButtonPressed = false;
    private bool showingMap = false;

    private Recorder recorder;
    private LayerMask previousLeftControllerLayerMask;
    private LayerMask previousRightControllerLayerMask;
    private TickTimer notificationTimer;

    #endregion

    #region Public Methods

    public void SetSpectator()
    {
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);

        // VRVRVRVRVRVRVRVRVRVR
        if (displaySubsystems.Count != 0)
        {
            return;
        }

        trackedPoseDriver.enabled = false;
        GetComponentInChildren<SpectatorCamera>().enabled = true;

        leftHand.gameObject.SetActive(false);
        rightHand.gameObject.SetActive(false);
    }

    public void UpdateLeftHandContraint(byte leftHandFingerStateBitfield)
    {
        leftFingers.UpdateWeights(leftHandFingerStateBitfield);
    }

    public void UpdateRightHandConstraint(byte righttHandFingerStateBitfield)
    {
        rightFingers.UpdateWeights(righttHandFingerStateBitfield);
    }

    public void SetCharacter(CharacterSheet sheet, bool waitingRoom)
    {
        if (!waitingRoom)
        {
            transform.position = sheet.spawnPosition;
            transform.rotation = Quaternion.Euler(sheet.spawnRotation);
        }

        leftHandVisuals.sharedMesh = sheet.handsMesh;
        leftHandVisuals.material = new Material(sheet.handsMaterial);

        rightHandVisuals.sharedMesh = sheet.handsMesh;
        rightHandVisuals.material = new Material(sheet.handsMaterial);

        leftHandCharacterDescription.UpdateDescription(sheet);

        this.sheet = sheet;
    }

    public CharacterSheet GetCharacter()
    {
        return sheet;
    }

    public void TeleportAndLock(Vector3 position, Quaternion rotation)
    {
        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = position,
            destinationRotation = rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        teleportationProvider.QueueTeleportRequest(request);

        trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;

        previousLeftControllerLayerMask = leftHandRayInteractor.raycastMask;
        previousRightControllerLayerMask = rightHandRayInteractor.raycastMask;

        leftHandRayInteractor.raycastMask = 0;
        rightHandRayInteractor.raycastMask = 0;
    }

    public void TeleportSpectator(Vector3 position, Quaternion rotation)
    {
        Camera.main.transform.position = position;
        Camera.main.transform.rotation = rotation;

        notification.Show("Everyone is going to explain his version of the story, pay attention.", false, 0.0f);
        StartCoroutine(hideTeleportNotification());
    }

    public IEnumerator hideTeleportNotification()
    {
        yield return new WaitForSeconds(5.0f);
        notification.Hide();
    }

    public void Unlock()
    {
        trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;

        leftHandRayInteractor.raycastMask = previousLeftControllerLayerMask;
        rightHandRayInteractor.raycastMask = previousRightControllerLayerMask;
    }

    public void ShowNotification(string notification, TickTimer timer)
    {
        notificationTimer = timer;
        this.notification.Show(notification, timer.IsRunning, timer.RemainingTime(runner));
    }

    public void HideNotification()
    {
        notification.Hide();
    }

    public void Mute()
    {
        recorder.RecordingEnabled = false;
    }

    public void UnMute()
    {
        recorder.RecordingEnabled = true;
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        runner = FindObjectOfType<NetworkRunner>();
        recorder = FindObjectOfType<Recorder>();

        if (trackedPoseDriver == null)
        {
            trackedPoseDriver = GetComponentInChildren<TrackedPoseDriver>();
        }

        if (xrOrigin == null)
        {
            xrOrigin = GetComponentInChildren<XROrigin>();
        }

        if (leftHandCharacterDescription == null)
        {
            leftHandCharacterDescription = leftHand.GetComponentInChildren<MenuControllerCharacterDescription>();
        }

        if (leftHandRayInteractor == null)
        {
            leftHandRayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();
        }

        if (rightHandRayInteractor == null)
        {
            rightHandRayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();
        }

        if (teleportationProvider == null)
        {
            teleportationProvider = FindObjectOfType<TeleportationProvider>();
        }

        if (notification == null)
        {
            notification = GetComponentInChildren<PlayerNotificationProvider>();
        }
    }

    private void Start()
    {
        if (runner != null)
        {
            runner.AddCallbacks(this);
        }

        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, devices);
        if (devices.Count != 0)
        {
            leftHardwareController = devices[0];
        }

        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, devices);
        if (devices.Count != 0)
        {
            rightHardwareController = devices[0];
        }

        if (FindObjectOfType<NetworkManager>().spectator)
        {
            SetSpectator();
        }

        notification.Hide();
        notificationTimer = default;
    }

    private void Update()
    {
        leftHandIndexConstraint.Update();
        leftHandMiddleConstraint.Update();
        leftHandRingConstraint.Update();
        leftHandPinkyConstraint.Update();
        leftHandThumbConstraint.Update();

        rightHandIndexConstraint.Update();
        rightHandMiddleConstraint.Update();
        rightHandRingConstraint.Update();
        rightHandPinkyConstraint.Update();
        rightHandThumbConstraint.Update();

        if (!notificationTimer.Expired(runner))
        {
            notification.UpdateTimer(notificationTimer.RemainingTime(runner));
        }
        else
        {
            notificationTimer = TickTimer.None;
        }  
    }

    private void OnDestroy()
    {
        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }
    }

    #endregion

    #region INetworkRunnerCallbacks

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        RigInput rigInput = new RigInput();

        rigInput.position = transform.position;
        rigInput.rotation = transform.rotation;

        rigInput.headsetPosition = headset.position;
        rigInput.headsetRotation = headset.rotation;

        rigInput.leftHandPosition = leftHand.position;
        rigInput.leftHandRotation = leftHand.rotation;

        rigInput.rightHandPosition = rightHand.position;
        rigInput.rightHandRotation = rightHand.rotation;

        rigInput.leftControllerButtonsPressed = 0;
        rigInput.rightControllerButtonsPressed = 0;

        bool buttonPressed = false;

        if (leftHardwareController == null || (leftHardwareController != null && !leftHardwareController.isValid))
        {
            List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, devices);
            if (devices.Count != 0)
            {
                leftHardwareController = devices[0];
            }
        }

        if (leftHardwareController != null)
        {
            leftHardwareController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out buttonPressed);
            rigInput.leftControllerButtonsPressed |= (byte) (buttonPressed ? RigInput.VrControllerButtons.Trigger : RigInput.VrControllerButtons.None);

            leftHardwareController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out buttonPressed);
            if (buttonPressed && sheet != null && lastLeftMenuButtonPressed == false)
            {
                 leftHandCharacterDescription.transform.parent.gameObject.SetActive(!leftHandCharacterDescription.transform.parent.gameObject.activeInHierarchy);
                 leftHandCharacterDescription.UpdateDescription(sheet);
            }

            lastLeftMenuButtonPressed = buttonPressed;
        }

        Debug.Log(string.Format("@Left Button pressed: {0} / Bytefield: {1}", buttonPressed.ToString(), Convert.ToString(rigInput.leftControllerButtonsPressed, 2).PadLeft(8, '0')));

        if (rightHardwareController == null || (rightHardwareController != null && !rightHardwareController.isValid))
        {
            List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, devices);
            if (devices.Count != 0)
            {
                rightHardwareController = devices[0];
            }
        }

        if (rightHardwareController != null)
        {
            rightHardwareController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out buttonPressed);
            if (buttonPressed && lastRightMenuButtonPressed == false)
            {
                rigInput.rightControllerButtonsPressed |= (byte) RigInput.VrControllerButtons.Menu;
                showingMap = !showingMap;
            }

            lastRightMenuButtonPressed = buttonPressed;

            rightHardwareController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out buttonPressed);
            rigInput.rightControllerButtonsPressed |= (byte)((!showingMap && buttonPressed) || (showingMap && buttonPressed && !lastRightTriggerButtonPressed) ?
                RigInput.VrControllerButtons.Trigger : RigInput.VrControllerButtons.None);
            lastRightTriggerButtonPressed = buttonPressed;
        }

        Debug.Log(string.Format("@Right Button pressed: {0} / Bytefield: {1}", buttonPressed.ToString(), Convert.ToString(rigInput.rightControllerButtonsPressed, 2).PadLeft(8, '0')));

        input.Set(rigInput);
    }

    #endregion

    #region Unused INetworkRunnerCallbacks

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnDisconnectedFromServer(NetworkRunner runner) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    #endregion
}
