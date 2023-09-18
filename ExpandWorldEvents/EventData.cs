using System.ComponentModel;
using ExpandWorldData;
namespace EWE;

public class EventData
{
  public string name = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue(60f)]
  public float duration = 60f;
  [DefaultValue(true)]
  public bool nearBaseOnly = true;
  [DefaultValue("")]
  public string biome = "";
  [DefaultValue("")]
  public string requiredGlobalKeys = "";
  [DefaultValue("")]
  public string notRequiredGlobalKeys = "";
  [DefaultValue("")]
  public string requiredPlayerKeys = "";
  [DefaultValue("")]
  public string notRequiredPlayerKeys = "";
  [DefaultValue("")]
  public string requiredKnownItems = "";
  [DefaultValue("")]
  public string notRequiredKnownItems = "";
  [DefaultValue("")]
  public string requiredEnvironments = "";
  [DefaultValue("")]
  public string startMessage = "";
  [DefaultValue("")]
  public string endMessage = "";
  public string forceMusic = "";
  [DefaultValue("")]
  public string forceEnvironment = "";
  public SpawnData[] spawns = [];
  [DefaultValue(true)]
  public bool pauseIfNoPlayerInArea = true;
  [DefaultValue(true)]
  public bool random = true;
}