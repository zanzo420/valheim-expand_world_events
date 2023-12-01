using System.Collections.Generic;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Event;

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.InValidBiome))]
public class ExtraChecks
{
  static bool Postfix(bool result, RandomEvent ev, Vector3 pos)
  {
    if (!result) return false;
    if (!Loader.ExtraData.TryGetValue(ev, out var data)) return true;
    if (!EnvCheck(pos, data.RequiredEnvironments)) return false;
    return true;
  }
  private static bool EnvCheck(Vector3 pos, List<string> required)
  {
    if (required.Count == 0) return true;
    var biome = WorldGenerator.instance.GetBiome(pos);
    var em = EnvMan.instance;
    var availableEnvironments = em.GetAvailableEnvironments(biome);
    if (availableEnvironments == null || availableEnvironments.Count == 0) return false;
    Random.State state = Random.state;
    var num = (long)ZNet.instance.GetTimeSeconds() / em.m_environmentDuration;
    Random.InitState((int)num);
    var env = em.SelectWeightedEnvironment(availableEnvironments);
    Random.state = state;
    return required.Contains(env.m_name.ToLower());
  }
  private static bool PlayerCheck(Vector3 pos, Range<int>? limit, float distance)
  {
    if (limit == null) return true;
    var within = RandEventSystem.RefreshPlayerEventData().Where(p => Utils.DistanceXZ(pos, p.position) <= distance).Count();
    return limit.Min <= within && within <= limit.Max;
  }
}

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.CheckBase))]
public class CheckBase
{
  static bool Prefix(RandomEvent ev, RandEventSystem.PlayerEventData player, ref bool __result)
  {
    if (!Loader.ExtraData.TryGetValue(ev, out var data)) return true;
    __result = player.baseValue >= data.BaseValue;
    return false;
  }
}