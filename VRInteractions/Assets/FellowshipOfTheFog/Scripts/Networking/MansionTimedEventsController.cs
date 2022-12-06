using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MansionTimedEventsController : NetworkBehaviour
{

    #region Properties

    [Header("Settings")]
    [SerializeField]
    private float activationTimeInSeconds = 600.0f;

    [Header("External Components")]
    [SerializeField]
    private GameObject activationGameObject;

    [Networked] private TickTimer timer { get; set; }

    #endregion

    #region Fusion Events

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            // 10 minutes
            timer = TickTimer.CreateFromSeconds(Runner, activationTimeInSeconds);
        }

        activationGameObject.SetActive(timer.IsRunning ? false : true);
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

        activationGameObject.SetActive(true);
    }

    #endregion

}
