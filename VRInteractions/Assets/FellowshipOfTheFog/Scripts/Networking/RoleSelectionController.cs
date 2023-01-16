using Fusion;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using Fusion.Sockets;
using System;

public enum Role
{
    Character = 0,
    Spectator = 1,
}

public class RoleSelectionController : NetworkBehaviour, INetworkRunnerCallbacks
{
    #region Properties

    [Header("External components")]
    [SerializeField]
    private LocalPlayerRig playerRig;

    [SerializeField]
    private GameObject vrMenu;
    
    [SerializeField]
    private GameObject defaultMenu;

    [SerializeField]
    private GameObject startGameDefaultMenu;

    [SerializeField]
    private GameObject startGameVrMenu;

    private MenuControllerRoleSelector currentMenuRoleSelector;

    private NetworkManager manager;

    private bool vr = false;

    [HideInInspector]
    [Networked]
    [Capacity(4)]
    public NetworkDictionary<NetworkString<_32>, NetworkBool> lockedCharacters => default;

    public Queue<KeyValuePair<string, bool>> characterLockQueue = new Queue<KeyValuePair<string, bool>>();

    #endregion

    #region Public Methods

    public void PickRoleAndCharacter(Role role, CharacterSheet sheet)
    {
        // Actually server doesn't care if a spectator has joined, there is nothing to spawn for them
        // So we don't need to call the rpc
        if (role == Role.Character)
        {
            // We use playerref.none to target the rpc call to the server even though the target is already server, so this should not be necesary.
            RPC_PickRoleAndCharacter(sheet.name, playerRig.headset.position.y / playerRig.xrOrigin.CameraYOffset);

            playerRig.SetCharacter(sheet, true);
        }
        else
        {
            playerRig.SetSpectator();
            manager.spectator = true;

            if (Runner.IsServer)
            {
                if (vrMenu.activeInHierarchy)
                {
                    startGameVrMenu.SetActive(true);
                }
                else
                {
                    startGameDefaultMenu.SetActive(true);
                }
            }
        }

        return;
    }

    public void StartGame()
    {
        // TODO: Hardcoded Scene
        Runner.SetActiveScene(2);
    }

    public void ShowMenu()
    {
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);

        if (displaySubsystems.Count != 0)
        {
            vrMenu.SetActive(true);
            defaultMenu.SetActive(false);

            currentMenuRoleSelector = vrMenu.GetComponentInChildren<MenuControllerRoleSelector>();
            vr = true;
        }
        else
        {
            vrMenu.SetActive(false);
            defaultMenu.SetActive(true);

            currentMenuRoleSelector = defaultMenu.GetComponentInChildren<MenuControllerRoleSelector>();

            vr = false;
        }
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        manager = FindObjectOfType<NetworkManager>();

        if (playerRig == null)
        {
            playerRig = FindObjectOfType<LocalPlayerRig>();
        }
    }

    #endregion

    #region Fusion Events

    public override void Spawned()
    {
        if (!Runner.IsServer)
        {
            return;
        }

        Runner.AddCallbacks(this);

        lockedCharacters.Clear();

        foreach (var item in manager.characters)
        {
            lockedCharacters.Add(item.name, false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer)
        {
            return;
        }

        if (characterLockQueue.Count != 0)
        {
            while (characterLockQueue.Count != 0)
            {
                var item = characterLockQueue.Dequeue();
                lockedCharacters.Set(item.Key, item.Value);
            }

            RPC_LockedCharacterChanged();
        }
    }

    #endregion

    #region RPC's

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_PickRoleAndCharacter(string characterName, float scale, RpcInfo info = default)
    {
        if (!Runner.IsServer)
        {
            return;
        }

        CharacterSheet sheet = manager.characters.Find(x => x.name == characterName);

        if (lockedCharacters[sheet.name])
        {
            return;
        }

        manager.SpawnCharacter(info.Source, sheet, scale);
        characterLockQueue.Enqueue(new KeyValuePair<string, bool>(sheet.name, true));

        RPC_CharacterSpawned(info.Source, true);

        // show start game menu
        if (info.Source == Runner.LocalPlayer)
        {
            if (vr)
            {
                startGameVrMenu.SetActive(true);
            }
            else
            {
                startGameDefaultMenu.SetActive(true);
            }
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_CharacterSpawned([RpcTarget] PlayerRef target, NetworkBool spawned)
    {
        if (spawned)
        {
            vrMenu.SetActive(false);
            defaultMenu.SetActive(false);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_LockedCharacterChanged()
    {
        if (currentMenuRoleSelector == null)
        {
            return;
        }

        currentMenuRoleSelector.UpdateLockedCharacters();
    }

    #endregion

    #region Fusion Network Callbacks

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        if (!runner.IsServer)
        {
            return;
        }

        NetworkObject networkPlayerObject = runner.GetPlayerObject(player);

        if (networkPlayerObject == null)
        {
            return;
        }

        NetworkPlayerRig networkRig = networkPlayerObject.GetComponentInChildren<NetworkPlayerRig>();

        if (networkRig == null)
        {
            return;
        }

        characterLockQueue.Enqueue(new KeyValuePair<string, bool>(networkRig.character.name, false));
        runner.Despawn(networkPlayerObject);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {}

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}

    public void OnConnectedToServer(NetworkRunner runner) {}

    public void OnDisconnectedFromServer(NetworkRunner runner) {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) {}

    public void OnSceneLoadDone(NetworkRunner runner) {}

    public void OnSceneLoadStart(NetworkRunner runner) {}

    #endregion
}
