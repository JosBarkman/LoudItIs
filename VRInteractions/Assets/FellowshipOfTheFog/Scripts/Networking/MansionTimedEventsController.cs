using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MansionTimedEventsController : NetworkBehaviour
{

    #region Properties

    [Header("External Components")]
    [SerializeField]
    private GameObject tenMinuteClue;

    [Networked] private TickTimer timer { get; set; }

    #endregion

    #region Fusion Events

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            // 10 minutes
            timer = TickTimer.CreateFromSeconds(Runner, 600.0f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!timer.Expired(Runner))
        {
            return;
        }

        if (Runner.IsServer)
        {
            timer = TickTimer.None;
        }

        tenMinuteClue.SetActive(true);
    }

    #endregion

}
