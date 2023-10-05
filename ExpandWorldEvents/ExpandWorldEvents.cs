using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ExpandWorld.Event;
using HarmonyLib;
using Service;
using UnityEngine;
namespace ExpandWorld;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", BepInDependency.DependencyFlags.HardDependency)]
public class EWE : BaseUnityPlugin
{
  public const string GUID = "expand_world_events";
  public const string NAME = "Expand World Events";
  public const string VERSION = "1.2";
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public static void LogWarning(string message) => Log.LogWarning(message);
  public static void LogError(string message) => Log.LogError(message);
  public static void LogInfo(string message) => Log.LogInfo(message);
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public static bool ConfigExists = File.Exists(Path.Combine(Paths.ConfigPath, "expand_world_events.cfg"));
  public void Awake()
  {
    Log = Logger;
    ConfigWrapper wrapper = new("expand_events_config", Config, ConfigSync, () => { });
    Configuration.Init(wrapper);
    if (!ConfigExists)
    {
      MigrateOldConfig();
      MigrateDataConfig();
    }
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    try
    {
      if (ExpandWorldData.Configuration.DataReload)
      {
        ExpandWorldData.DataManager.SetupWatcher(Config);
        Manager.SetupWatcher();
      }
    }
    catch (Exception e)
    {
      Log.LogError(e);
    }
  }

  public static RandomEvent GetCurrentRandomEvent(Vector3 pos)
  {
    if (Configuration.MultipleEvents)
      return MultipleEvents.Events.OrderBy(x => Utils.DistanceXZ(x.m_pos, pos)).FirstOrDefault();
    return RandEventSystem.instance.GetCurrentRandomEvent();
  }
  private void MigrateOldConfig()
  {
    if (!File.Exists(Path.Combine(Paths.ConfigPath, "expand_world.cfg"))) return;
    Log.LogWarning("Migrating from old config file.");
    Config.Save();
    var from = File.ReadAllLines(Path.Combine(Paths.ConfigPath, "expand_world.cfg"));
    var to = File.ReadAllLines(Config.ConfigFilePath);
    foreach (var line in from)
    {
      var split = line.Split('=');
      if (split.Length != 2) continue;
      for (var i = 0; i < to.Length; ++i)
      {
        if (to[i].StartsWith(split[0]))
          to[i] = line;
      }
    }
    File.WriteAllLines(Config.ConfigFilePath, to);
    Config.Reload();
  }
  private void MigrateDataConfig()
  {
    if (!File.Exists(Path.Combine(Paths.ConfigPath, "expand_world_data.cfg"))) return;
    Log.LogWarning("Migrating from data config file.");
    Config.Save();
    var from = File.ReadAllLines(Path.Combine(Paths.ConfigPath, "expand_world_data.cfg"));
    var to = File.ReadAllLines(Config.ConfigFilePath);
    foreach (var line in from)
    {
      var split = line.Split('=');
      if (split.Length != 2) continue;
      for (var i = 0; i < to.Length; ++i)
      {
        if (to[i].StartsWith(split[0]))
          to[i] = line;
      }
    }
    File.WriteAllLines(Config.ConfigFilePath, to);
    Config.Reload();
  }
#pragma warning disable IDE0051
  private void OnDestroy()
  {
    Config.Save();
  }
#pragma warning restore IDE0051
}
