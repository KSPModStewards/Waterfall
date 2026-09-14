using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall
{
  /// <summary>
  ///   Material color modifier
  /// </summary>
  public class EffectColorFromLightModifier : DirectModifier
  {
    protected override string ConfigNodeName => WaterfallConstants.ColorFromLightNodeName;

    [Persistent] public string colorName;
    [Persistent] public string lightTransformName;
    [Persistent] public float colorBlend;
    public Light[] lights;

    private Material[] m;
    private int colorPropertyID;
    public override bool ValidForIntegrator => false;

    public EffectColorFromLightModifier() : base()
    {
      modifierTypeName = "Material Color From Light";
    }

    public EffectColorFromLightModifier(ConfigNode node) : base(node)
    {
      colorPropertyID = Shader.PropertyToID(colorName);
    }

    public override void Init(WaterfallEffect parentEffect)
    {
      base.Init(parentEffect);
      m      = new Material[xforms.Count];
      lights = parentEffect.parentModule.GetComponentsInChildren<Light>().Where(x => x.transform.name == parentEffect.parentName).ToArray();
      for (int i = 0; i < xforms.Count; i++)
      {
        m[i] = xforms[i].GetComponent<Renderer>().material;
      }
    }

    public Material GetMaterial() => m[0];

    public void ApplyColorName(string newColorName)
    {
      colorName = newColorName;
      colorPropertyID = Shader.PropertyToID(colorName);
    }

    public void ApplyLightName(string newLightName)
    {
      lightTransformName = newLightName;
      lights             = parentEffect.parentModule.GetComponentsInChildren<Light>().Where(x => x.transform.name == lightTransformName).ToArray();
    }

    public override void Apply(float[] strengthList)
    {
      // NOTE: not currently used.
      UpdateRandomValue();

      for (int i = 0; i < m.Length; i++)
      {
        if (lights != null && lights.Length > i)
          m[i].SetColor(colorPropertyID, lights[i].color * colorBlend + Color.white * (1f - colorBlend));
        else if (lights != null && lights.Length > 0)
          m[i].SetColor(colorPropertyID, lights[0].color * colorBlend + Color.white * (1f - colorBlend));
      }
    }
  }
}
