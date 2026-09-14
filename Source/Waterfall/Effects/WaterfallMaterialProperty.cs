using System;
using UnityEngine;

namespace Waterfall
{
  public enum WaterfallMaterialPropertyType
  {
    Texture,
    Color,
    Float,
    Vector4
  }

  public class WaterfallMaterialProperty
  {
    public string                        propertyName;
    public WaterfallMaterialPropertyType propertyType;

    public virtual void Load(ConfigNode node) { }

    public virtual ConfigNode Save() => null;

    public virtual void Initialize(Material m) { }
  }


  public class WaterfallMaterialTextureProperty : WaterfallMaterialProperty
  {
    public string  texturePath;
    public Vector2 textureScale  = Vector2.one;
    public Vector2 textureOffset = Vector2.zero;

    public WaterfallMaterialTextureProperty()
    {
      propertyType = WaterfallMaterialPropertyType.Texture;
    }

    public WaterfallMaterialTextureProperty(ConfigNode node)
    {
      Load(node);
      propertyType = WaterfallMaterialPropertyType.Texture;
    }

    public override void Load(ConfigNode node)
    {
      node.TryGetValue("textureSlotName", ref propertyName);
      node.TryGetValue("texturePath",     ref texturePath);
      node.TryGetValue("textureScale",    ref textureScale);
      node.TryGetValue("textureOffset",   ref textureOffset);

      Utils.Log(String.Format("[WaterfallMaterialTextureProperty]: Loading new texture for slot {0} ", propertyName));
    }

    public override void Initialize(Material m)
    {
      Utils.Log(String.Format("[WaterfallMaterialTextureProperty]: Setting new texture for slot {0} ", propertyName));
      Texture loadedTexture = GameDatabase.Instance.GetTexture(texturePath, false);
      m.SetTexture(propertyName, loadedTexture);
      m.SetTextureScale(propertyName, textureScale);
      m.SetTextureOffset(propertyName, textureOffset);
    }

    public override ConfigNode Save()
    {
      var node = new ConfigNode();
      node.name = WaterfallConstants.TextureNodeName;
      node.AddValue("textureSlotName", propertyName);
      node.AddValue("texturePath",     texturePath);
      node.AddValue("textureScale",    textureScale);
      node.AddValue("textureOffset",   textureOffset);

      return node;
    }
  }

  public class WaterfallMaterialFloatProperty : WaterfallMaterialProperty
  {
    public float propertyValue;

    public WaterfallMaterialFloatProperty()
    {
      propertyType = WaterfallMaterialPropertyType.Float;
    }

    public WaterfallMaterialFloatProperty(ConfigNode node)
    {
      Load(node);
      propertyType = WaterfallMaterialPropertyType.Float;
    }

    public override void Load(ConfigNode node)
    {
      node.TryGetValue("floatName", ref propertyName);
      node.TryGetValue("value",     ref propertyValue);
    }

    public override void Initialize(Material m)
    {
      m.SetFloat(propertyName, propertyValue);
    }

    public override ConfigNode Save()
    {
      var node = new ConfigNode();
      node.name = WaterfallConstants.FloatNodeName;
      node.AddValue("floatName", propertyName);
      node.AddValue("value",     propertyValue);

      return node;
    }
  }

  public class WaterfallMaterialColorProperty : WaterfallMaterialProperty
  {
    public Color propertyValue;


    public WaterfallMaterialColorProperty()
    {
      propertyType = WaterfallMaterialPropertyType.Color;
    }

    public WaterfallMaterialColorProperty(ConfigNode node)
    {
      Load(node);
      propertyType = WaterfallMaterialPropertyType.Color;
    }

    public override void Load(ConfigNode node)
    {
      node.TryGetValue("colorName",  ref propertyName);
      node.TryGetValue("colorValue", ref propertyValue);
      Utils.Log($"[WaterfallMaterialColorProperty]: loaded color {propertyName} with value {propertyValue.ToString()}");
    }

    public override void Initialize(Material m)
    {
      m.SetColor(propertyName, propertyValue);
    }

    public override ConfigNode Save()
    {
      var node = new ConfigNode();
      node.name = WaterfallConstants.ColorNodeName;
      node.AddValue("colorName",  propertyName);
      node.AddValue("colorValue", propertyValue);

      return node;
    }
  }

  public class WaterfallMaterialVector4Property : WaterfallMaterialProperty
  {
    public Vector4 propertyValue;


    public WaterfallMaterialVector4Property()
    {
      propertyType = WaterfallMaterialPropertyType.Vector4;
    }

    public WaterfallMaterialVector4Property(ConfigNode node)
    {
      Load(node);
      propertyType = WaterfallMaterialPropertyType.Vector4;
    }

    public override void Load(ConfigNode node)
    {
      node.TryGetValue("vectorName", ref propertyName);
      node.TryGetValue("value",      ref propertyValue);
      Utils.Log($"[WaterfallMaterialVector4Property]: loaded Vector4 {propertyName} with value {propertyValue.ToString()}");
    }

    public override void Initialize(Material m)
    {
      m.SetVector(propertyName, propertyValue);
    }

    public override ConfigNode Save()
    {
      var node = new ConfigNode();
      node.name = WaterfallConstants.Vector4NodeName;
      node.AddValue("vectorName", propertyName);
      node.AddValue("value",      propertyValue);

      return node;
    }
  }
}