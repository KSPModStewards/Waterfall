using System;
using System.Collections.Generic;
using UnityEngine;

namespace Waterfall
{
  public class WaterfallLight
  {
    public string    transformName     = "";
    public string    baseTransformName = "";
    [Persistent] public float     intensity = 1f;
    [Persistent] public float     range = 1f;
    [Persistent] public Color     color;
    [Persistent] public LightType lightType = LightType.Point;
    [Persistent] public float     angle;

    public List<Light> lights;

    public WaterfallLight()
    {
      color     = Color.white;
    }

    public WaterfallLight(ConfigNode node) : this()
    {
      Load(node);
    }

    public void Load(ConfigNode node)
    {
      ConfigNode.LoadObjectFromConfig(this, node);
      node.TryGetValue("transform",     ref transformName);
      node.TryGetValue("baseTransform", ref baseTransformName);

      Utils.Log($"[WaterfallLight]: Loading new light for {transformName}");
    }

    public ConfigNode Save()
    {
      var node = ConfigNode.CreateConfigFromObject(this);
      node.name = WaterfallConstants.LightNodeName;
      node.AddValue("transform", transformName);
      node.AddValue("baseTransform", baseTransformName);
      return node;
    }

    public void Initialize(Transform parentTransform)
    {
      lights = new(); // parentTransform.GetComponentsInChildren<Light>().ToList();

      if (baseTransformName != "")
      {
        var candidates = parentTransform.GetComponentsInChildren<Transform>();
        foreach (var t in candidates)
        {
          var l = t.GetComponent<Light>();
          if (l != null)
          {
            Utils.Log($"[WaterfallLight]: Added light material from {t.name}");
            lights.Add(l);
          }
        }
      }
      else
      {
        var materialTarget = parentTransform.FindDeepChild(transformName);
        var l              = materialTarget.GetComponent<Light>();

        l.enabled = Settings.EnableLights;
        lights.Add(l);
      }

      Utils.Log($"[WaterfallLight]: Initialized WaterfallLight at {parentTransform}, {lights.Count} Count");

      foreach (var l in lights)
      {
        l.range     = range;
        l.type      = lightType;
        l.intensity = intensity;
        l.spotAngle = angle;
        l.color     = color;
      }
    }

    public void SetRange(float value)
    {
      range = value;
      foreach (var l in lights)
      {
        l.range = range;
      }
    }

    public void SetAngle(float value)
    {
      angle = value;
      foreach (var l in lights)
      {
        l.spotAngle = angle;
      }
    }

    public void SetIntensity(float value)
    {
      intensity = value;
      foreach (var l in lights)
      {
        l.intensity = intensity;
      }
    }

    public void SetColor(Color value)
    {
      color = value;
      foreach (var l in lights)
      {
        l.color = color;
      }
    }

    public void SetLightType(LightType lType)
    {
      lightType = lType;
      foreach (var l in lights)
      {
        l.type = lightType;
      }
    }
  }
}
