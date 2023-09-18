using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
namespace EWE;

public class EventManager
{
  public static string FileName = "expand_events.yaml";
  public static string FilePath = Path.Combine(EWE.YamlDirectory, FileName);
  public static string Pattern = "expand_events*.yaml";
  public static Dictionary<RandomEvent, List<string>> EventToRequirentEnvironment = [];
  public static List<RandomEvent> Originals = [];

  public static RandomEvent FromData(EventData data)
  {
    RandomEvent random = new()
    {
      m_name = data.name,
      m_spawn = data.spawns.Select(SpawnManager.FromData).ToList(),
      m_enabled = data.enabled,
      m_random = data.random,
      m_duration = data.duration,
      m_nearBaseOnly = data.nearBaseOnly,
      m_pauseIfNoPlayerInArea = data.pauseIfNoPlayerInArea,
      m_biome = DataManager.ToBiomes(data.biome),
      m_requiredGlobalKeys = DataManager.ToList(data.requiredGlobalKeys),
      m_notRequiredGlobalKeys = DataManager.ToList(data.notRequiredGlobalKeys),
      m_altRequiredPlayerKeysAny = DataManager.ToList(data.requiredPlayerKeys),
      m_altNotRequiredPlayerKeys = DataManager.ToList(data.notRequiredPlayerKeys),
      m_altRequiredKnownItems = DataManager.ToItemList(data.requiredKnownItems),
      m_altRequiredNotKnownItems = DataManager.ToItemList(data.notRequiredKnownItems),
      m_startMessage = data.startMessage,
      m_endMessage = data.endMessage,
      m_forceMusic = data.forceMusic,
      m_forceEnvironment = data.forceEnvironment
    };
    EventToRequirentEnvironment[random] = DataManager.ToList(data.requiredEnvironments);
    return random;
  }
  public static EventData ToData(RandomEvent random)
  {
    EventData data = new()
    {
      name = random.m_name,
      spawns = random.m_spawn.Select(SpawnManager.ToData).ToArray(),
      enabled = random.m_enabled,
      random = random.m_random,
      duration = random.m_duration,
      nearBaseOnly = random.m_nearBaseOnly,
      pauseIfNoPlayerInArea = random.m_pauseIfNoPlayerInArea,
      biome = DataManager.FromBiomes(random.m_biome),
      requiredGlobalKeys = DataManager.FromList(random.m_requiredGlobalKeys),
      notRequiredGlobalKeys = DataManager.FromList(random.m_notRequiredGlobalKeys),
      requiredPlayerKeys = DataManager.FromList(random.m_altRequiredPlayerKeysAny),
      notRequiredPlayerKeys = DataManager.FromList(random.m_altNotRequiredPlayerKeys),
      requiredKnownItems = DataManager.FromList(random.m_altRequiredKnownItems),
      notRequiredKnownItems = DataManager.FromList(random.m_altRequiredNotKnownItems),
      startMessage = random.m_startMessage,
      endMessage = random.m_endMessage,
      forceMusic = random.m_forceMusic,
      forceEnvironment = random.m_forceEnvironment
    };
    return data;
  }

  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    if (File.Exists(FilePath)) return;
    var yaml = DataManager.Serializer().Serialize(RandEventSystem.instance.m_events.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    Set(DataManager.Read(Pattern));
    // No point to send duplicate events to clients.
    var uniqueEvents = RandEventSystem.instance.m_events.Distinct(new Comparer());
    Configuration.valueEventData.Value = DataManager.Serializer().Serialize(RandEventSystem.instance.m_events.Select(ToData).ToList());
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (Helper.IsServer() && Originals.Count == 0)
      Originals = [.. RandEventSystem.instance.m_events];
    EventToRequirentEnvironment.Clear();
    if (yaml == "") return;
    try
    {
      var data = DataManager.Deserialize<EventData>(yaml, FileName).Select(FromData).ToList();
      if (data.Count == 0)
      {
        EWD.Log.LogWarning($"Failed to load any event data.");
        return;
      }
      if (ExpandWorldData.Configuration.DataMigration && Helper.IsServer() && AddMissingEntries(data))
      {
        // Watcher triggers reload.
        return;
      }
      EWD.Log.LogInfo($"Reloading event data ({data.Count} entries).");
      RandEventSystem.instance.m_events = data;
    }
    catch (Exception e)
    {
      EWD.Log.LogError(e.Message);
      EWD.Log.LogError(e.StackTrace);
    }
  }
  private static bool AddMissingEntries(List<RandomEvent> entries)
  {
    var missingKeys = Originals.Select(e => e.m_name).Distinct().ToHashSet();
    foreach (var item in entries)
      missingKeys.Remove(item.m_name);
    if (missingKeys.Count == 0) return false;
    var missing = Originals.Where(item => missingKeys.Contains(item.m_name)).ToList();
    EWD.Log.LogWarning($"Adding {missing.Count} missing events to the expand_events.yaml file.");
    foreach (var item in missing)
      EWD.Log.LogWarning(item.m_name);
    var yaml = File.ReadAllText(FilePath);
    var data = DataManager.Serializer().Serialize(missing.Select(ToData));
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
      EventManager.ToFile();
      EventManager.FromFile();
    }
  }
}


[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.SetRandomEvent)), HarmonyPriority(Priority.First)]
public class MultipleConfigurationsPerEvent
{
  static void Prefix(RandEventSystem __instance, ref RandomEvent ev)
  {
    // Clients automatically get the first event by name.
    // So this is for the server (self host) if multiple configurations are used.
    ev = __instance.GetEvent(ev?.m_name);
  }
}
public class Comparer : IEqualityComparer<RandomEvent>
{
  public bool Equals(RandomEvent x, RandomEvent y) => x.m_name == y.m_name;

  public int GetHashCode(RandomEvent obj) => obj.m_name.GetStableHashCode();
}