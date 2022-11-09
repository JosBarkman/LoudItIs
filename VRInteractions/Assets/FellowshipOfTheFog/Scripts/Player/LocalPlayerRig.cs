using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

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
    private Transform leftHandVisuals;
    
    [SerializeField]
    private Transform rightHandVIsuals;

    [SerializeField]
    private TrackedPoseDriver trackedPoseDriver;

    private NetworkRunner runner;
    private InputDevice leftHardwareController;
    private InputDevice rightHardwareController;

    #endregion

    #region Public Methods

    public void SetSpectator()
    {
        trackedPoseDriver.enabled = false;
        GetComponentInChildren<SpectatorCamera>().enabled = true;

        leftHandVisuals.gameObject.SetActive(false);
        rightHandVIsuals.gameObject.SetActive(false);

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

        if (rightHardwareController != null)
        {
            rightHardwareController.TryGetFeatureValue(CommonUsages.triggerButton, out buttonPressed);
            rigInput.rightControllerButtonsPressed |= (byte)(buttonPressed ? RigInput.VrControllerButtons.Trigger : RigInput.VrControllerButtons.None);
        }

        Debug.Log(string.Format("@Button pressed: {0} / Bytefield: {1}", buttonPressed.ToString(), Convert.ToString(rigInput.leftControllerButtonsPressed, 2).PadLeft(8, '0')));

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
