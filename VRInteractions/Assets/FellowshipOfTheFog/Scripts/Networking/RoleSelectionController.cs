using Fusion;
using UnityEngine;
using System.Collections.Generic;

public enum Role
{
    Character = 0,
    Spectator = 1,
}

public class RoleSelectionController : NetworkBehaviour
{
    #region Properties

    [Header("Settings")]
    public List<CharacterSheet> characters = new List<CharacterSheet>();

    [Header("External components")]
    [SerializeField]
    private LocalPlayerRig playerRig;

    private NetworkManager manager;

    #endregion

    #region Public Methods

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_PickRoleAndCharacter([RpcTarget] PlayerRef targetPlayer, string characterName, float scale, RpcInfo info = default)
    {
        CharacterSheet sheet = characters.Find(x => x.name == characterName);

        manager.SpawnCharacter(info.Source, sheet.prefab, scale);
    }

    public void PickRoleAndCharacter(Role role, CharacterSheet sheet)
    {
        // Actually server doesn't care if a spectator has joined, there is nothing to spawn for them
        // So we don't need to call the rpc
        if (role == Role.Character)
        {
            // We use playerref.none to target the rpc call to the server
            RPC_PickRoleAndCharacter(PlayerRef.None, sheet.name, playerRig.headset.position.y / playerRig.cameraOffset.cameraYOffset);

            playerRig.transform.position = sheet.spawnPosition;
            playerRig.transform.rotation = Quaternion.Euler(sheet.spawnRotation);
        }
        else
        {
            // TODO: if we define the spectator as someone who can see the "play" in vr or computer
            // what we have to do should be something like: playerRig.SetVR(isVREnabled)
            // there configure a spectator for vr or for mouse/keyboard.
            // if we take an step further we could also have non vr actors.
            playerRig.SetSpectator();
        }

        // Bad
        Cursor.lockState = CursorLockMode.Locked;

        return;
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
}
