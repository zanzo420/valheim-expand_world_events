using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
namespace ExpandWorld.Event;

public class Manager
{
  public static string FileName = "expand_events.yaml";
  public static string FilePath = Path.Combine(EWD.YamlDirectory, FileName);
  public static string Pattern = "expand_events*.yaml";
  public static List<RandomEvent> Originals = [];


  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    if (File.Exists(FilePath)) return;
    var yaml = DataManager.Serializer().Serialize(RandEventSystem.instance.m_events.Select(Loader.ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    Set(DataManager.Read(Pattern));
    // No point to send duplicate events to clients.
    var uniqueEvents = RandEventSystem.instance.m_events.Distinct(new Comparer());
    Configuration.valueEventData.Value = DataManager.Serializer().Serialize(RandEventSystem.instance.m_events.Select(Loader.ToData).ToList());
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (Helper.IsServer() && Originals.Count == 0)
      Originals = [.. RandEventSystem.instance.m_events];
    Loader.ExtraData.Clear();
    if (yaml == "") return;
    try
    {
      var data = DataManager.Deserialize<Data>(yaml, FileName).Select(Loader.FromData).ToList();
      if (data.Count == 0)
      {
        EWE.LogWarning($"Failed to load any event data.");
        return;
      }
      if (ExpandWorldData.Configuration.DataMigration && Helper.IsServer() && AddMissingEntries(data))
      {
        // Watcher triggers reload.
        return;
      }
      EWE.LogInfo($"Reloading event data ({data.Count} entries).");
      RandEventSystem.instance.m_events = data;
    }
    catch (Exception e)
    {
      EWE.LogError(e.Message);
      EWE.LogError(e.StackTrace);
    }
  }
  private static bool AddMissingEntries(List<RandomEvent> entries)
  {
    var missingKeys = Originals.Select(e => e.m_name).Distinct().ToHashSet();
    foreach (var item in entries)
      missingKeys.Remove(item.m_name);
    if (missingKeys.Count == 0) return false;
    var missing = Originals.Where(item => missingKeys.Contains(item.m_name)).ToList();
    EWE.LogWarning($"Adding {missing.Count} missing events to the expand_events.yaml file.");
    foreach (var item in missing)
      EWE.LogWarning(item.m_name);
    var yaml = File.ReadAllText(FilePath);
    var data = DataManager.Serializer().Serialize(missing.Select(Loader.ToData));
    // Directly appending is risky but necessary to keep comments, etc.
    yaml += "\n" + data;
    File.WriteAllText(FilePath, yaml);
    return true;
  }
  public static void SetupWatcher()
  {
    DataManager.SetupWatcher(Pattern, FromFile);
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.Last)]
public class InitializeContent
{
  static void Postfix()
  {
    if (Helper.IsServer())
    {
      Manager.ToFile();
      Manager.FromFile();
    }
  }
}

public class Comparer : IEqualityComparer<RandomEvent>
{
  public bool Equals(RandomEvent x, RandomEvent y) => x.m_name == y.m_name;

  public int GetHashCode(RandomEvent obj) => obj.m_name.GetStableHashCode();
}