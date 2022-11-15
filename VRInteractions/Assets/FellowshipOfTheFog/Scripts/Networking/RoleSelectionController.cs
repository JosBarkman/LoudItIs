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

    [Header("Settings")]
    public List<CharacterSheet> characters = new List<CharacterSheet>();

    [Header("External components")]
    [SerializeField]
    private LocalPlayerRig playerRig;

    [SerializeField]
    private GameObject vrMenu;
    
    [SerializeField]
    private GameObject defaultMenu;

    private NetworkManager manager;

    #endregion

    #region Public Methods

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
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
            // We use playerref.none to target the rpc call to the server even though the target is already server, so this should not be necesary.
            RPC_PickRoleAndCharacter(PlayerRef.None, sheet.name, playerRig.headset.position.y / playerRig.xrOrigin.CameraYOffset);

            playerRig.SetCharacter(sheet);            
        }
        else
        {
            playerRig.SetSpectator(vrMenu.activeInHierarchy);
        }

        // Bad
        Cursor.lockState = CursorLockMode.Locked;

        vrMenu.SetActive(false);
        defaultMenu.SetActive(false);

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

    private void Start()
    {
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);

        if (displaySubsystems.Count != 0)
        {
            vrMenu.SetActive(true);
            defaultMenu.SetActive(false);
        }
        else
        {
            vrMenu.SetActive(false);
            defaultMenu.SetActive(true);
        }
    }

    #endregion
}
