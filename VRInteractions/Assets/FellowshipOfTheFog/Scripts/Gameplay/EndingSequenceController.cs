using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EndingSequenceController : NetworkBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField] private int requiredVotes = 3;
    [SerializeField] private float initialSequenceWaitSeconds = 5.0f;
    [SerializeField] private float speakingTimeSeconds = 30.0f;

    [SerializeField] private Transform[] endPositions;

    private Dictionary<PlayerRef, bool> votes;
    private IEnumerable<PlayerRef> players;
    private NetworkPlayerRig[] rigs = new NetworkPlayerRig[4];

    private TickTimer[] timers = new TickTimer[5] { TickTimer.None, TickTimer.None, TickTimer.None, TickTimer.None, TickTimer.None };

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

        players = Runner.ActivePlayers;

        int i = 0;
        foreach (PlayerRef activePlayer in players)
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

    #endregion

    #region Unity Events

    private void Start()
    {
        votes = new Dictionary<PlayerRef, bool>();
    }

    #endregion

    #region Netowrk Behaviour Events

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer)
        {
            return;
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
        }
    }

    #endregion
}
