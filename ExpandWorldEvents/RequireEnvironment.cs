using HarmonyLib;
using UnityEngine;

namespace EWE;

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.InValidBiome))]
public class RequiredEnvironment
{
  static bool Postfix(bool result, RandomEvent ev, Vector3 point)
  {
    if (!result) return false;
    if (!EventManager.EventToRequirentEnvironment.TryGetValue(ev, out var required)) return true;
    if (required.Count == 0) return true;
    var biome = WorldGenerator.instance.GetBiome(point);
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
}
