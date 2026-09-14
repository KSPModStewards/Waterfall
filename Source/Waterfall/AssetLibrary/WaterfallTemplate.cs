using System.Collections.Generic;

namespace Waterfall
{
  /// <summary>
  ///   Defines an Effect Template, which is a set of WaterfallEffects defined in a separate config
  /// </summary>
  public class WaterfallTemplate
  {
    public string templateName;


    public List<WaterfallEffect> allFX;

    public WaterfallTemplate() { }

    public WaterfallTemplate(ConfigNode node)
    {
      Load(node);
    }

    public void Load(ConfigNode node)
    {
      node.TryGetValue("templateName", ref templateName);
      // Load up all the effects
      var effectNodes = node.GetNodes(WaterfallConstants.EffectNodeName);

      Utils.Log($"[WaterfallTemplate]: Loading effects on {templateName}");
      allFX = new();
      foreach (var fxDataNode in effectNodes)
      {
        allFX.Add(new(fxDataNode));
      }
    }
  }
}