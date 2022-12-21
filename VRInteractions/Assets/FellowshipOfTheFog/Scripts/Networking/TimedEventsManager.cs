using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEventsManager : NetworkBehaviour
{
    #region Properties

    [Header("Settings")]
    [SerializeField]
    private TimedEvent[] events;

    [Networked, Capacity(8)]
    NetworkArray<TickTimer> timers => default;

    #endregion

    #region Fusion Events

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            if (events.Length > timers.Length)
            {
                Log.Error("Events lenght rgeater than timers");
            }
            for (int i = 0; i < events.Length; i++)
            {
                timers.Set(i, TickTimer.CreateFromSeconds(Runner, events[i].seconds));
            }
        }

        for (int i = 0; i < events.Length; i++)
        {
            if (timers.Get(i).IsRunning)
            {
                events[i].DefaultState();
            }
            else
            {
                events[i].Execute();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        for (int i = 0; i < events.Length; i++)
        {
            if (timers.Get(i).Expired(Runner))
            {
                if (Runner.IsServer)
                {
                    timers.Set(i, TickTimer.None);
                }

                events[i].Execute();
            }
        }
    }

    #endregion
}
