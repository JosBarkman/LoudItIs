using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EndingSequenceController : NetworkBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField] private int requiredVotes = 3;

    private Dictionary<PlayerRef, bool> votes;

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

        Debug.Log("Votes: " + votes.Count);

        if (votes.Count < requiredVotes)
        {
            return;
        }

        Debug.Log("PEPOPEPOSHY!");
    }

    #endregion

    #region Unity Events

    private void Start()
    {
        votes = new Dictionary<PlayerRef, bool>();
    }

    #endregion
}
