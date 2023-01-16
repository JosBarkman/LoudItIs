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

    [SerializeField] private Transform spectatorsEndPosition;
    [SerializeField] private Transform[] endPositions;

    [Header("Settings")]
    [SerializeField] private GameObject endGameVrMenu;
    [SerializeField] private GameObject endGameDefaultMenu;

    private Dictionary<PlayerRef, bool> startingSequenceVotes;
    private Dictionary<PlayerRef, PlayerRef> playerActorVotes;
    private Queue<KeyValuePair<PlayerRef, PlayerRef>> votingQueue;

    private NetworkPlayerRig[] rigs;

    private TickTimer[] sequenceTimers = new TickTimer[5] { TickTimer.None, TickTimer.None, TickTimer.None, TickTimer.None, TickTimer.None };

    private MenuControllerVotingMenu currentVotingMenu = null;

    #endregion

    #region Public Methods

    public void Vote(PlayerRef player)
    {
        if (!Runner.IsServer)
        {
            return;
        }

        if (startingSequenceVotes.ContainsKey(player))
        {
            startingSequenceVotes.Remove(player);
        }
        else
        {
            startingSequenceVotes.Add(player, true);
        }

        if (startingSequenceVotes.Count < requiredVotes)
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
            else
            {
                RPC_TeleportSpectator(activePlayer, spectatorsEndPosition.position, spectatorsEndPosition.rotation);
            }
        }

        sequenceTimers[0] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds);
        sequenceTimers[1] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + speakingTimeSeconds);
        sequenceTimers[2] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + (speakingTimeSeconds * 2));
        sequenceTimers[3] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + (speakingTimeSeconds * 3));
        sequenceTimers[4] = TickTimer.CreateFromSeconds(Runner, initialSequenceWaitSeconds + (speakingTimeSeconds * 4));
    }

    public void VotePlayer(PlayerRef player)
    {
        RPC_QueueVote(player);
    }

    #endregion

    #region Unity Events

    private void Start()
    {
        startingSequenceVotes = new Dictionary<PlayerRef, bool>();
        playerActorVotes = new Dictionary<PlayerRef, PlayerRef>();
        votingQueue = new Queue<KeyValuePair<PlayerRef, PlayerRef>>();
        rigs = new NetworkPlayerRig[4];

        endGameVrMenu.SetActive(false);
        endGameDefaultMenu.SetActive(false);
    }

    #endregion

    #region Netowrk Behaviour Events

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer)
        {
            return;
        }

        if (Keyboard.current[Key.Space].isPressed)
        {
            Vote(Runner.LocalPlayer);
        }

        // update player voting
        // we dequeue one item per tick
        if (votingQueue.Count != 0)
        {
            List<PlayerRef> players = new List<PlayerRef>();
            List<int> newVotes = new List<int>();

            for (int i = 0; i < votingQueue.Count; i++)
            {
                var item = votingQueue.Dequeue();
                if (!playerActorVotes.ContainsKey(item.Key))
                {
                    playerActorVotes.Add(item.Key, item.Value);
                }
                else
                {
                    if (playerActorVotes[item.Key] == item.Value)
                    {
                        playerActorVotes.Remove(item.Key);
                    }
                    else
                    {
                        PlayerRef oldVote = playerActorVotes[item.Key];

                        playerActorVotes[item.Key] = item.Value;

                        players.Add(oldVote);
                        newVotes.Add(playerActorVotes.Count(x => x.Value == oldVote));
                    }
                }

                players.Add(item.Value);
                newVotes.Add(playerActorVotes.Count(x => x.Value == item.Value));

                RPC_VoteQueueProcessed(players.ToArray(), newVotes.ToArray());
            }            
        }

        for (int i = 0; i < sequenceTimers.Length - 1; i++)
        {
            if (sequenceTimers[i].Expired(Runner))
            {
                sequenceTimers[i] = TickTimer.None;
                if (rigs[i] != null)
                {
                    rigs[i].RPC_Unmute(speakingTimeSeconds);
                }
            }
        }

        if (sequenceTimers[4].Expired(Runner))
        {
            sequenceTimers[4] = TickTimer.None;
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

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_QueueVote(PlayerRef target, RpcInfo info = default)
    {
        votingQueue.Enqueue(new KeyValuePair<PlayerRef, PlayerRef>(info.Source, target));
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_VoteQueueProcessed(PlayerRef[] players, int[] votes)
    {
        if (currentVotingMenu == null)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            currentVotingMenu.UpdatePlayerVotes(players[i], votes[i]);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_TeleportSpectator([RpcTarget] PlayerRef player, Vector3 position, Quaternion rotation)
    {
        FindObjectOfType<LocalPlayerRig>().TeleportSpectator(position, rotation);
    }

    #endregion
}
