using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EndingSequenceController : NetworkBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField] private int requiredVotes = 3;

    [SerializeField] private Transform[] endPositions;

    private Dictionary<PlayerRef, bool> votes;
    private IEnumerable<PlayerRef> players;


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
                    rig.RPC_TeleportAndLock(endPositions[i].position, endPositions[i].rotation);
                    i++;
                }
            }
        }
    }

    #endregion

    #region Unity Events

    private void Start()
    {
        votes = new Dictionary<PlayerRef, bool>();
    }

    #endregion
}
