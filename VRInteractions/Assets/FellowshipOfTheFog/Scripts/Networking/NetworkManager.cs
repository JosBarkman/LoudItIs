using Fusion;
using Fusion.Sockets;
using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the connection general networking functionalities like
///     - Connecting and disconnecting from sessios
///     - Spawning player prefabs when user connects
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    #region Properties

    [Header("Settings")]
    [SerializeField]
    private float voiceDetectionThreshold = .1f;

    public List<CharacterSheet> characters = new List<CharacterSheet>();

    private NetworkRunner runner;
    private Recorder recorder;
    public bool spectator = false;

    public delegate void SessionListUpdatedEvent(List<SessionInfo> sessionInfos);
    public event SessionListUpdatedEvent OnSessionListUpdatedEvent;

    #endregion

    #region Unity Event

    private void Awake()
    {
        runner = gameObject.GetComponent<NetworkRunner>();
        runner.ProvideInput = true;
        recorder = GetComponentInChildren<Recorder>();
    }

    private async void Start()
    {
        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    private void Update()
    {
        if (recorder.VoiceDetectionThreshold != voiceDetectionThreshold)
        {
            recorder.VoiceDetectionThreshold = voiceDetectionThreshold;
        }
    }

    #endregion

    #region Public methods

    public async void HostSession(string sessionName, int maxCharacters)
    {
        Dictionary<string, SessionProperty> properties = new Dictionary<string, SessionProperty>();

        // TODO: HARDCODED STRING VALUES
        properties["MAX_CHARACTERS"] = maxCharacters;
        properties["CURRENT_CHARACTERS"] = 0;

        StartGameArgs args = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            //SessionProperties = properties,
            // TODO: Magic number
            PlayerCount = 10,
            // TODO: Magic number
            Scene = 1,
            SceneManager = gameObject.AddComponent<NetworkSceneManager>()
        };

        await runner.StartGame(args);

    }

    public async void Join(SessionInfo info)
    {
        StartGameArgs args = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = info.Name,
            SceneManager = gameObject.AddComponent<NetworkSceneManager>()

        };

        await runner.StartGame(args);
    }

    public NetworkPlayerRig SpawnCharacter(PlayerRef source, CharacterSheet sheet, float scale)
    {
        if (!runner.IsServer)
        {
            return null;
        }

        // Position and rotation here doesn't matter as the networked player will track the local rig
        NetworkObject networkPlayerObject = runner.Spawn(sheet.prefab, new Vector3(999.0f, 999.0f, 999.0f), Quaternion.identity, source);
        networkPlayerObject.transform.localScale = Vector3.one * scale;

        NetworkPlayerRig networkRig = networkPlayerObject.GetComponentInChildren<NetworkPlayerRig>();
        networkRig.sheet = sheet;

        networkRig.networkedHeadFeetOffset = networkRig.headFeetOffset * scale;

        runner.SetPlayerObject(source, networkPlayerObject);
        return networkRig;
    }

    #endregion

    #region INetworkRunnerCallbacks

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkObject networkPlayerObject = runner.GetPlayerObject(player);

            if (networkPlayerObject != null)
            {
                runner.Despawn(networkPlayerObject);
            }
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        OnSessionListUpdatedEvent?.Invoke(sessionList);
    }

    #endregion

    #region Unused INetworkRunnerCallbacks

    public void OnConnectedToServer(NetworkRunner runner) {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}

    public void OnDisconnectedFromServer(NetworkRunner runner) {}

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}

    public void OnInput(NetworkRunner runner, NetworkInput input) {}

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) {}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}

    public void OnSceneLoadDone(NetworkRunner runner) {}

    public void OnSceneLoadStart(NetworkRunner runner) {}

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}

    #endregion
}
