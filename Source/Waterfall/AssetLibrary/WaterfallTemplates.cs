using System;
using System.Collections.Generic;

namespace Waterfall
{
  /// <summary>
  ///   Class for loading and retrieving effect templates
  /// </summary>
  public static class WaterfallTemplates
  {
    public static Dictionary<string, WaterfallTemplate> Library = new();

    /// <summary>
    ///   Get an effect template by name
    /// </summary>
    /// <param name="name">The config name of the template</param>
    /// <returns>The template, if it exists</returns>
    public static WaterfallTemplate GetTemplate(string name)
    {
      if (Library.ContainsKey(name))
      {
        return Library[name];
      }

      Utils.LogError($"[Template Libary]: Can't find effect template {name}");
      return null;
    }

    /// <summary>
    ///   Load all templates from config
    /// </summary>
    public static void LoadTemplates()
    {
      Utils.Log("[Template Libary]: Loading effect templates");
      var nodes = GameDatabase.Instance.GetConfigNodes(WaterfallConstants.TemplateLibraryNodeName);

      foreach (var node in nodes)
      {
        if (!Library.ContainsKey(node.GetValue("templateName")))
        {
          try
          {
            Library.Add(node.GetValue("templateName"), new(node));
            Utils.Log($"[Template Libary]: Added template {node.GetValue("templateName")}");
          }
          catch (Exception)
          {
            Utils.LogError($"[Template Libary]: Exception loading template {node.GetValue("templateName")}. This is likely a config error");
          }
        }
      }
    }
  }
}