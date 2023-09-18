using System.Collections.Generic;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using UnityEngine;

namespace EWE;

[HarmonyPatch(typeof(RandEventSystem))]
public class MultipleEvents
{
  private static readonly List<RandomEvent> Events = [];


  [HarmonyPatch(nameof(RandEventSystem.FixedUpdate)), HarmonyPrefix]
  static bool FixedUpdate(RandEventSystem __instance)
  {
    if (Helper.IsClient() || !Configuration.MultipleEvents) return true;
    var dt = Time.fixedDeltaTime;
    var obj = __instance;
    obj.UpdateForcedEvents(dt);
    obj.UpdateRandomEvent(dt);
    obj.m_forcedEvent?.Update(true, true, true, dt);
    var expired = Events.Where(x =>
    {
      var playerInArea = __instance.IsAnyPlayerInEventArea(x);
      return x.Update(true, true, playerInArea, Time.fixedDeltaTime);
    }).ToList();
    Events.RemoveAll(expired.Contains);
    if (obj.m_forcedEvent != null)
      obj.SetActiveEvent(obj.m_forcedEvent);
    else if (Player.m_localPlayer)
    {
      // This is simply for testing in single player.
      // Dedicated server doesn't need m_randomEvent for anything.
      var randomEvent = Events.OrderBy(x => Utils.DistanceXZ(x.m_pos, Player.m_localPlayer.transform.position)).FirstOrDefault();
      obj.m_randomEvent = randomEvent;
      if (randomEvent != null && obj.IsInsideRandomEventArea(randomEvent, Player.m_localPlayer.transform.position))
        __instance.SetActiveEvent(randomEvent);
      else
        obj.SetActiveEvent(null);
    }
    else
      obj.SetActiveEvent(null);
    return false;
  }

  [HarmonyPatch(nameof(RandEventSystem.SetRandomEvent)), HarmonyPrefix]
  static bool SetRandomEvent(RandEventSystem __instance, RandomEvent ev, Vector3 pos)
  {
    if (Helper.IsClient() || !Configuration.MultipleEvents) return true;
    if (ev == null)
    {
      Events.ForEach(x => x.OnStop());
      Events.Clear();
      return false;
    }
    var nearbyEvents = Events.Where(x => Utils.DistanceXZ(x.m_pos, pos) < Configuration.EventMinimumDistance).ToList();
    nearbyEvents.ForEach(x => x.OnStop());
    Events.RemoveAll(nearbyEvents.Contains);

    var randomEvent = ev.Clone();
    randomEvent.m_pos = pos;
    randomEvent.OnStart();
    Events.Add(randomEvent);
    __instance.SendCurrentRandomEvent();
    return false;
  }

  [HarmonyPatch(nameof(RandEventSystem.SendCurrentRandomEvent)), HarmonyPrefix]
  static bool SendCurrentRandomEvent()
  {
    if (Helper.IsClient() || !Configuration.MultipleEvents || Events.Count == 0) return true;
    if (Events.Count == 1)
    {
      var randomEvent = Events.First();
      ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SetEvent", [randomEvent.m_name, randomEvent.m_time, randomEvent.m_pos]);
      return false;
    }
    var peers = ZNet.instance.GetPeers();
    peers.ForEach(peer =>
    {
      if (peer.m_rpc == null) return;
      var randomEvent = Events.OrderBy(x => Utils.DistanceXZ(x.m_pos, peer.m_refPos)).First();
      peer.m_rpc.Invoke("SetEvent", [randomEvent.m_name, randomEvent.m_time, randomEvent.m_pos]);
    });
    return false;
  }
}
