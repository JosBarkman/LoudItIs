using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Linq;
using System;

public class EndingSequenceController : NetworkBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField] private int requiredVotes = 3;
    [SerializeField] private float initialSequenceWaitSeconds = 5.0f;
    [SerializeField] private float speakingTimeSeconds = 30.0f;

    [SerializeField] private Transform[] endPositions;

    [Header("Settings")]
    [SerializeField] private GameObject endGameVrMenu;
    [SerializeField] private GameObject endGameDefaultMenu;

    private Dictionary<PlayerRef, bool> votes;
    private NetworkPlayerRig[] rigs;
    private Queue<KeyValuePair<PlayerRef, PlayerRef>> votingQueue;

    private TickTimer[] timers = new TickTimer[5] { TickTimer.None, TickTimer.None, TickTimer.None, TickTimer.None, TickTimer.None };

    private MenuControllerVotingMenu currentVotingMenu = null;

    [Networked()]
    [Capacity(16)]
    private NetworkDictionary<PlayerRef, PlayerRef> playerVotes => default;

    #endregion

    #region Public Methods

    public void Vote(PlayerRef player)
    {
        if (!Runner.IsServer)
        {
            return;
        }

        if (votes.ContainsKey(player))
        {
            votes.Remove(player);
        }
        else
        {
            votes.Add(player, true);
        }

        if (votes.Count < requiredVotes)
        {
            return;
        }

        int i = 0;
        foreach (PlayerRef activePlayer in Runner.ActivePlayers)
        {
            NetworkObject obj = Runner.GetPlayerObject(activePlayer);
            if (obj != null)
            {
                NetworkPlayerRig rig = obj.GetComponentInChildren<NetworkPlayerRig>();
                if (rig != null)
                {
                    rigs[i] = rig;

                    rig.RPC_TeleportAndLock(endPositions[i].position, endPositions[i].rotation);

                    i++;
                }
            }
        }

        timers[0] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds);
        timers[1] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + speakingTimeSeconds);
        timers[2] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + (speakingTimeSeconds * 2));
        timers[3] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + (speakingTimeSeconds * 3));
        timers[4] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + (speakingTimeSeconds * 4));
    }

    public void VotePlayer(PlayerRef player)
    {
        RPC_QueueVote(player);
    }

    #endregion

    #region Unity Events

    private void Start()
    {
        votes = new Dictionary<PlayerRef, bool>();
        rigs = new NetworkPlayerRig[4];
        votingQueue = new Queue<KeyValuePair<PlayerRef, PlayerRef>>();

        endGameVrMenu.SetActive(false);
        endGameDefaultMenu.SetActive(false);
    }

    #endregion

    #region Netowrk Behaviour Events

    public override void FixedUpdateNetwork()
    {
        if (Keyboard.current[Key.Space].isPressed)
        {
            Vote(Runner.LocalPlayer);
        }

        if (currentVotingMenu != null)
        {
            // This no bueno, poor performance, but nmo changed event for networkdictionaries, sad :(
            foreach (var player in Runner.ActivePlayers)
            {
                currentVotingMenu.UpdatePlayerVotes(player, playerVotes.Count(x => x.Value == player));
            }
        }

        if (!Runner.IsServer)
        {
            return;
        }

        // update player voting
        // we dequeue one item per tick
        if (votingQueue.Count != 0)
        {
            var item = votingQueue.Dequeue();
            if (!playerVotes.ContainsKey(item.Key))
            {
                playerVotes.Add(item.Key, item.Value);
            }
            else
            {
                playerVotes.Set(item.Key, item.Value);
            }
        }

        for (int i = 0; i < timers.Length - 1; i++)
        {
            if (timers[i].Expired(Runner))
            {
                timers[i] = TickTimer.None;
                if (rigs[i] != null)
                {
                    rigs[i].RPC_Unmute(speakingTimeSeconds);
                }
            }
        }

        if (timers[4].Expired(Runner))
        {
            timers[4] = TickTimer.None;
            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i] != null)
                {
                    rigs[i].RPC_Unmute(0.0f);
                }
            }

            RPC_ShowVotingMenu();
        }
    }

    #endregion

    #region RPCs

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_ShowVotingMenu()
    {
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);

        // VRVRVRVRVRVRVRVRVRVR
        if (displaySubsystems.Count != 0)
        {
            endGameVrMenu.SetActive(true);
            currentVotingMenu = endGameVrMenu.GetComponentInChildren<MenuControllerVotingMenu>();
        }
        else
        {
            endGameDefaultMenu.SetActive(true);
            currentVotingMenu = endGameDefaultMenu.GetComponentInChildren<MenuControllerVotingMenu>();
        }

        Dictionary<PlayerRef, CharacterSheet> playerCharacters = new Dictionary<PlayerRef, CharacterSheet>();
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            NetworkObject obj = Runner.GetPlayerObject(player);
            if (obj == null)
            {
                continue;
            }

            NetworkPlayerRig rig = obj.GetComponentInChildren<NetworkPlayerRig>();
            if (rig == null)
            {
                continue;
            }

            playerCharacters.Add(player, rig.character);
        }

        currentVotingMenu.AddCharacterItems(playerCharacters);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_QueueVote(PlayerRef target, RpcInfo info = default)
    {
        votingQueue.Enqueue(new KeyValuePair<PlayerRef, PlayerRef>(info.Source, target));
    }

    #endregion
}
