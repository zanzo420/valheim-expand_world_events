using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
namespace ExpandWorld;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", BepInDependency.DependencyFlags.HardDependency)]
public class EWE : BaseUnityPlugin
{
  public const string GUID = "expand_world_events";
  public const string NAME = "Expand World Events";
  public const string VERSION = "1.0";
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
  public static string ConfigName = $"{GUID}.cfg";
  public static bool NeedsMigration = File.Exists(Path.Combine(Paths.ConfigPath, "expand_world.cfg")) && !File.Exists(Path.Combine(Paths.ConfigPath, ConfigName));
  public void Awake()
  {
    Log = Logger;
    ConfigWrapper wrapper = new("expand_events_config", Config, ConfigSync, () => { });
    Configuration.Init(wrapper);
    if (NeedsMigration)
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
        SetupWatcher();
        Event.Manager.SetupWatcher();
      }
    }
    catch (Exception e)
    {
      Log.LogError(e);
    }
  }
  private void MigrateOldConfig()
  {
    Log.LogWarning("Migrating from old config file.");
    Config.Save();
    var from = File.ReadAllLines(Path.Combine(Paths.ConfigPath, "expand_world.cfg"));
    var to = File.ReadAllLines(Path.Combine(Paths.ConfigPath, ConfigName));
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
    File.WriteAllLines(Path.Combine(Paths.ConfigPath, ConfigName), to);
    Config.Reload();
  }
  private void MigrateDataConfig()
  {
    Log.LogWarning("Migrating from data config file.");
    Config.Save();
    var from = File.ReadAllLines(Path.Combine(Paths.ConfigPath, "expand_world_data.cfg"));
    var to = File.ReadAllLines(Path.Combine(Paths.ConfigPath, ConfigName));
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
    File.WriteAllLines(Path.Combine(Paths.ConfigPath, ConfigName), to);
    Config.Reload();
  }
#pragma warning disable IDE0051
  private void OnDestroy()
  {
    Config.Save();
  }
#pragma warning restore IDE0051

  private void SetupWatcher()
  {
    FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigName);
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  private void ReadConfigValues(object sender, FileSystemEventArgs e)
  {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try
    {
      Log.LogDebug("ReadConfigValues called");
      Config.Reload();
    }
    catch
    {
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
    }
  }
}
