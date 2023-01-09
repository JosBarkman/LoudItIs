using Fusion;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

public enum Role
{
    Character = 0,
    Spectator = 1,
}

public class RoleSelectionController : NetworkBehaviour
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

    private MenuControllerRoleSelector currentRoleSelector;

    private NetworkManager manager;

    [HideInInspector]
    [Networked]
    [Capacity(4)]
    public NetworkDictionary<string, NetworkBool> lockedCharacters => default;

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

            currentRoleSelector = vrMenu.GetComponentInChildren<MenuControllerRoleSelector>();
        }
        else
        {
            vrMenu.SetActive(false);
            defaultMenu.SetActive(true);

            currentRoleSelector = defaultMenu.GetComponentInChildren<MenuControllerRoleSelector>();
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

        lockedCharacters.Clear();

        foreach (var item in manager.characters)
        {
            lockedCharacters.Add(item.name.Substring(0, 4), false);
        }
    }

    #endregion

    #region RPC's

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_PickRoleAndCharacter(string characterName, float scale, RpcInfo info = default)
    {
        CharacterSheet sheet = manager.characters.Find(x => x.name == characterName);

        currentRoleSelector.DisableCharacter(sheet);

        if (!Runner.IsServer)
        {
            return;
        }

        if (lockedCharacters[sheet.name.Substring(0, 4)])
        {
            return;
        }

        manager.SpawnCharacter(info.Source, sheet, scale);
        lockedCharacters.Set(sheet.name.Substring(0, 4), true);

        RPC_CharacterSpawned(info.Source, true);

        // show start game menu
        if (info.Source == Runner.LocalPlayer)
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

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_CharacterSpawned([RpcTarget] PlayerRef target, NetworkBool spawned)
    {
        if (spawned)
        {
            vrMenu.SetActive(false);
            defaultMenu.SetActive(false);
        }
    }

    #endregion
}
