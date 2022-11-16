using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static FingerGrabable;

/// <summary>
/// Network input strcut
/// </summary>
public struct RigInput : INetworkInput
{
    public enum VrControllerButtons : byte
    {
        None = 0x0,
        Trigger = 1 << 7,
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

    public FingersIK leftFingers;
    public FingersIK rightFingers;

    [SerializeField]
    private Rig rightHandRigConstraints;

    [SerializeField]
    private Transform leftHandVisuals;
    
    [SerializeField]
    private Transform rightHandVisuals;

    [SerializeField]
    private TrackedPoseDriver trackedPoseDriver;

    private NetworkRunner runner;
    private InputDevice leftHardwareController;
    private InputDevice rightHardwareController;

    #endregion

    #region Public Methods

    public void SetSpectator(bool vr)
    {
        if (vr)
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

    public void SetCharacter(CharacterSheet sheet)
    {
        transform.position = sheet.spawnPosition;
        transform.rotation = Quaternion.Euler(sheet.spawnRotation);

        leftHandVisuals.GetComponent<MeshFilter>().mesh = sheet.handsMesh;
        leftHandVisuals.GetComponent<MeshRenderer>().material = new Material(sheet.handsMaterial);

        rightHandVisuals.GetComponent<MeshFilter>().mesh = sheet.handsMesh;
        rightHandVisuals.GetComponent<MeshRenderer>().material = new Material(sheet.handsMaterial);
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        runner = FindObjectOfType<NetworkRunner>();

        if (trackedPoseDriver == null)
        {
            trackedPoseDriver = GetComponentInChildren<TrackedPoseDriver>();
        }

        if (xrOrigin == null)
        {
            xrOrigin = GetComponentInChildren<XROrigin>();
        }
    }

    private void Start()
    {
        if (runner != null)
        {
            runner.AddCallbacks(this);
        }

        List<InputDevice> devices = new List<InputDevice>();

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

        if (leftHardwareController != null)
        {
            leftHardwareController.TryGetFeatureValue(CommonUsages.triggerButton, out buttonPressed);
            rigInput.leftControllerButtonsPressed |= (byte) (buttonPressed ? RigInput.VrControllerButtons.Trigger : RigInput.VrControllerButtons.None);
        }

        Debug.Log(string.Format("@Left Button pressed: {0} / Bytefield: {1}", buttonPressed.ToString(), Convert.ToString(rigInput.leftControllerButtonsPressed, 2).PadLeft(8, '0')));

        if (rightHardwareController != null)
        {
            rightHardwareController.TryGetFeatureValue(CommonUsages.triggerButton, out buttonPressed);
            rigInput.rightControllerButtonsPressed |= (byte)(buttonPressed ? RigInput.VrControllerButtons.Trigger : RigInput.VrControllerButtons.None);
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
