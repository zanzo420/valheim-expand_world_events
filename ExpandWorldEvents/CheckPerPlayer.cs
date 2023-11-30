using System.Collections.Generic;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Event;

[HarmonyPatch(typeof(RandEventSystem))]
public class CheckPerPlayer
{

  [HarmonyPatch(nameof(RandEventSystem.UpdateRandomEvent)), HarmonyPrefix]
  static void FixedUpdate(RandEventSystem __instance, float dt)
  {
    if (Helper.IsClient() || !Configuration.CheckPerPlayer) return;
    __instance.m_eventTimer += dt;
    // Inverse below
    if (Game.m_eventRate == 0f) return;
    if (__instance.m_eventTimer <= __instance.m_eventIntervalMin * 60f * Game.m_eventRate) return;
    // Negative dt because original function runs.
    __instance.m_eventTimer = -dt;
    RandEventSystem.RefreshPlayerEventData();
    var players = RandEventSystem.playerEventDatas.ToList();
    foreach (var player in players)
    {
      if (Random.Range(0f, 100f) > __instance.m_eventChance / Game.m_eventRate) continue;
      RandEventSystem.playerEventDatas = [player];
      var events = GetPossibleRandomEvents(__instance, player);
      if (events.Count == 0) continue;
      var ev = events[Random.Range(0, events.Count)];
      __instance.SetRandomEvent(ev.Key, ev.Value);
    }
  }

  private static List<KeyValuePair<RandomEvent, Vector3>> GetPossibleRandomEvents(RandEventSystem obj, RandEventSystem.PlayerEventData player)
  {
    obj.m_lastPossibleEvents.Clear();
    foreach (RandomEvent randomEvent in obj.m_events)
    {
      List<RandEventSystem.PlayerEventData> data = [player];
      if (!randomEvent.m_enabled || !randomEvent.m_random || !obj.HaveGlobalKeys(randomEvent, data))
        continue;

      var validEventPoints = obj.GetValidEventPoints(randomEvent, data);
      if (validEventPoints.Count == 0)
        continue;

      var value = validEventPoints[Random.Range(0, validEventPoints.Count)];
      obj.m_lastPossibleEvents.Add(new(randomEvent, value));

    }
    return obj.m_lastPossibleEvents;
  }
}
