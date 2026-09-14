using System.Collections.Generic;
using UnityEngine;

namespace Waterfall
{
  /// <summary>
  ///   Material UV scrolling modifier
  /// </summary>
  public class EffectUVScrollModifier : DirectModifier
  {
    protected override string ConfigNodeName => WaterfallConstants.UVScrollModifierNodeName;

    public FastFloatCurve scrollCurveX = new();
    public FastFloatCurve scrollCurveY = new();
    [Persistent] public string textureName;
    private Material[] m;
    public override bool ValidForIntegrator => false;

    public EffectUVScrollModifier() : base()
    {
      modifierTypeName = GetType().Name;
    }

    public EffectUVScrollModifier(ConfigNode node) : base(node) { }

    public override void Load(ConfigNode node)
    {
      base.Load(node);
      scrollCurveX.Load(node.GetNode("scrollCurveX"));
      scrollCurveY.Load(node.GetNode("scrollCurveY"));
    }

    public override ConfigNode Save()
    {
      var node = base.Save();

      node.AddNode(Utils.SerializeFloatCurve("scrollCurveX", scrollCurveX));
      node.AddNode(Utils.SerializeFloatCurve("scrollCurveY", scrollCurveY));
      return node;
    }

    public override void Init(WaterfallEffect parentEffect)
    {
      base.Init(parentEffect);
      m = new Material[xforms.Count];
      for (int i = 0; i < xforms.Count; i++)
      {
        m[i] = xforms[i].GetComponent<Renderer>().material;
      }
    }

    public override void Apply(float[] strengthList)
    {
      // NOTE: not currently used.  But I can't find any cfgs actually using this class.
      // We should create a random normalized vec2 for the direction, then scale it by the random value
      UpdateRandomValue();

      float strength = strengthList[0];
      for (int i = 0; i < m.Length; i++)
      {
        var   original = m[i].GetTextureOffset(textureName);
        float x = Mathf.Repeat(original.x + scrollCurveX.Evaluate(strength) * Time.deltaTime, 1f);
        float y = Mathf.Repeat(original.y + scrollCurveY.Evaluate(strength) * Time.deltaTime, 1f);
        m[i].SetTextureOffset(textureName, new(x, y));
      }
    }
  }
}
