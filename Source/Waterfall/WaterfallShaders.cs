using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Waterfall
{
  /// <summary>
  ///   Class for loading and retrieving shaders
  /// </summary>
  public static class ShaderLoader
  {
    /// <summary>
    ///   A collection of all shaders loaded by Waterfall
    /// </summary>
    private static readonly Dictionary<string, Shader> ShaderDictionary = new();

    /// <summary>
    ///   A collection of all editable shader properies
    /// </summary>
    private static readonly Dictionary<string, MaterialData> ShaderPropertyMap = new();

    /// <summary>
    ///   Requests a shader by name
    /// </summary>
    /// <param name="shaderName"></param>
    /// <returns></returns>
    public static Shader GetShader(string shaderName)
    {
      Utils.Log("[ShaderLoader]: Getting shader " + shaderName);
      return ShaderDictionary.ContainsKey(shaderName) ? ShaderDictionary[shaderName] : null;
    }


    public static Dictionary<string, MaterialData> GetShaderPropertyMap() => ShaderPropertyMap;

    /// <summary>
    ///   Requests a shader by name
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllShadersNames() => ShaderDictionary.Keys.ToList();

    /// <summary>
    ///   Loads all shaders in the plugin's data directory
    /// </summary>
    public static void LoadShaders()
    {


      string path = Path.Combine(KSPUtil.ApplicationRootPath);

      Utils.Log($"[Shaders]: Loading Shaders");

      string pathSpec;
      if (Application.platform == RuntimePlatform.WindowsPlayer && SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
      {
        pathSpec = "*-linux.waterfall"; // fixes OpenGL on windows
      }
      else if (Application.platform == RuntimePlatform.WindowsPlayer)
      {
        pathSpec = "*-windows.waterfall";
      }
      else if (Application.platform == RuntimePlatform.LinuxPlayer)
      {
        pathSpec = "*-linux.waterfall";
      }
      else
      {
        pathSpec = "*-macos.waterfall";
      }

      List<string> bundlePaths = Directory.GetFiles(path, pathSpec, SearchOption.AllDirectories).ToList();
      List<string> orderedBundles = bundlePaths.OrderBy(x => Path.GetFileNameWithoutExtension(x)).ToList();

      foreach (string bundle in orderedBundles)
      {
        LoadAssetBundleAtPath(bundle);
      }
    }

    /// <summary>
    ///   Manually load Shader Asset bundles by path
    /// </summary>
    public static void LoadAssetBundleAtPath(string bundlePath)
    {
      Utils.Log($"[Shaders]: Loading shaders from {Path.GetFileNameWithoutExtension(bundlePath)}");
      var bundle = AssetBundle.LoadFromFile(bundlePath);
      if (bundle == null) return;
      var shaders = bundle.LoadAllAssets<Shader>();
      foreach (var shader in shaders)
      {
        Utils.Log($"[Shaders]: Adding {shader.name} ({Path.GetFileNameWithoutExtension(bundlePath)})");
        if (!ShaderDictionary.ContainsKey(shader.name))
        {
          ShaderDictionary.Add(shader.name, shader);
        }
        else
        {
          ShaderDictionary[shader.name] = shader;
          Utils.LogWarning($"[ShaderLoader]: A shader with {shader.name} already exists, replacing with new version");
        }
      }
      Utils.Log($"[Shaders]: Loaded {ShaderDictionary.Count} shaders");

      bundle.Unload(false); // unload the raw asset bundle
    }

    public static void LoadShaderProperties()
    {
      foreach (var node in GameDatabase.Instance.GetConfigNodes(WaterfallConstants.ShaderPropertyNodeName))
      {
        try
        {
          string propertyName = node.GetValue("name");
          var range = Vector2.zero;

          node.TryGetValue("range", ref range);
          var t = WaterfallMaterialPropertyType.Float;
          node.TryGetEnum("type", ref t, WaterfallMaterialPropertyType.Float);
          var m = new MaterialData(propertyName, t, range);
          ShaderPropertyMap.Add(propertyName, m);
        }
        catch
        {
          Utils.LogError($"[Shaders] Issue loading shader param from node: {node}");
        }
      }
    }
  }
}