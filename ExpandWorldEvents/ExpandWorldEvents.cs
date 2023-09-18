using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
namespace EWE;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInIncompatibility("expand_world")]
public class EWE : BaseUnityPlugin
{
  public const string GUID = "expand_world_events";
  public const string NAME = "Expand World Events";
  public const string VERSION = "1.0";
#nullable disable
  public static ManualLogSource Log;
  public static EWE Instance;
#nullable enable
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public static string ConfigName = $"{GUID}.cfg";
  public static bool NeedsMigration = File.Exists(Path.Combine(Paths.ConfigPath, "expand_world.cfg")) && !File.Exists(Path.Combine(Paths.ConfigPath, ConfigName));
  public static string YamlDirectory = Path.Combine(Paths.ConfigPath, "expand_world");
  public void InvokeRegenerate()
  {
    // Nothing to regenerate because the world hasn't been generated yet.
    if (WorldGenerator.instance?.m_world?.m_menu != false) return;
    // Debounced for smooth config editing.
    CancelInvoke("Regenerate");
    Invoke("Regenerate", 1.0f);
  }
  public void Awake()
  {
    Instance = this;
    Log = Logger;
    if (!Directory.Exists(YamlDirectory))
      Directory.CreateDirectory(YamlDirectory);
    ConfigWrapper wrapper = new("expand_config", Config, ConfigSync, InvokeRegenerate);
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
        EventManager.SetupWatcher();
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
