using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using Service;

namespace ExpandWorld;
public partial class Configuration
{
#nullable disable
  public static ConfigEntry<bool> configMultipleEvents;
  public static bool MultipleEvents => configMultipleEvents.Value;
  public static ConfigEntry<string> configEventMinimumDistance;
  public static float EventMinimumDistance => ConfigWrapper.Floats[configEventMinimumDistance];


  public static ConfigEntry<string> configEventInterval;
  public static float EventInterval => ConfigWrapper.Floats[configEventInterval];
  public static ConfigEntry<string> configEventChance;
  public static float EventChance => ConfigWrapper.Floats[configEventChance];
  public static CustomSyncedValue<string> valueEventData;

#nullable enable
  public static void Init(ConfigWrapper wrapper)
  {
    var section = "1. General";
    configMultipleEvents = wrapper.Bind(section, "Multiple events", false, false, "If enabled, multiple events can be active at the same time.");
    configEventMinimumDistance = wrapper.BindFloat(section, "Minimum distance between events", 100, false, "The minimum distance between events.");

    configEventChance = wrapper.BindFloat(section, "Random event chance", 20, false, "The chance to try starting a random event.");
    configEventChance.SettingChanged += (s, e) => RandomEventSystem.Setup(RandEventSystem.instance);
    configEventInterval = wrapper.BindFloat(section, "Random event interval", 46, false, "How often the random events are checked (minutes).");
    configEventInterval.SettingChanged += (s, e) => RandomEventSystem.Setup(RandEventSystem.instance);
    RandomEventSystem.Setup(RandEventSystem.instance);
    valueEventData = wrapper.AddValue("event_data");
    valueEventData.ValueChanged += () => Event.Manager.FromSetting(valueEventData.Value);
  }
}

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.Awake))]
public class RandomEventSystem
{
  public static void Setup(RandEventSystem rs)
  {
    if (!rs) return;
    rs.m_eventChance = Configuration.EventChance;
    rs.m_eventIntervalMin = Configuration.EventInterval;
  }
  static void Postfix(RandEventSystem __instance) => Setup(__instance);
}

