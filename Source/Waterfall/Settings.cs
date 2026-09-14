using UnityEngine;

namespace Waterfall
{
  /// <summary>
  ///   Static class to hold settings and configuration
  /// </summary>
  public static class Settings
  {
    /// Settings go here
    public static bool ShowEffectEditor = false;

    public static double AtmosphereDensityExponent = 0.5128;
    public static float MinimumEffectIntensity = 0.005f;
    public static float MinimumLightIntensity = 0.05f;

    public static int TransparentQueueBase = 3000;
    public static int QueueDepth = 750;
    public static float SortedDepth = 1000f;
    public static int DistortQueue = TransparentQueueBase + QueueDepth + 2;

    public static bool EnableLights = true;
    public static bool EnableDistortion = true;
    public static bool EnableLegacyBlendModes = false;
    public static bool ForceAllControllersAwake = false;
    public static bool RandomControllersAwake = true;

    private static bool _loadedOnce = false;
    /// <summary>
    ///   Load data from configuration
    /// </summary>
    public static void Load()
    {
      if (_loadedOnce) return;

      _loadedOnce = true;
      var settingsNode = GameDatabase.Instance.GetConfigNode("Waterfall/WaterfallSettings/WATERFALL_SETTINGS");

      Utils.Log("[Settings]: Started loading");
      if (settingsNode != null)
      {
        Utils.Log("[Settings]: Using specified settings");
        // Setting parsing goes here

        settingsNode.TryGetValue("ShowEffectEditor", ref ShowEffectEditor);
        settingsNode.TryGetValue("AtmosphereDensityExponent", ref AtmosphereDensityExponent);
        settingsNode.TryGetValue("MinimumEffectIntensity", ref MinimumEffectIntensity);
        settingsNode.TryGetValue("MinimumLightIntensity", ref MinimumLightIntensity);
        settingsNode.TryGetValue("TransparentQueueBase", ref TransparentQueueBase);
        settingsNode.TryGetValue("DistortQueue", ref DistortQueue);
        settingsNode.TryGetValue("QueueDepth", ref QueueDepth);
        settingsNode.TryGetValue("SortedDepth", ref SortedDepth);
        settingsNode.TryGetValue("EnableLights", ref EnableLights);
        settingsNode.TryGetValue("EnableDistortion", ref EnableDistortion);
        settingsNode.TryGetValue("EnableLegacyBlendModes", ref EnableLegacyBlendModes);
        settingsNode.TryGetValue(nameof(ForceAllControllersAwake), ref ForceAllControllersAwake);
        settingsNode.TryGetValue(nameof(RandomControllersAwake), ref RandomControllersAwake);
      }
      else
      {
        Utils.Log("[Settings]: Couldn't find settings file, using defaults");
      }

      Utils.Log("[Settings]: Finished loading");
    }
  }
}