using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Waterfall.UI
{
  public class UIMaterialData
  {
    public WaterfallMaterialPropertyType PropertyType { get { return parameterData.type; } }
    public string name;
    protected MaterialData parameterData;
    protected WaterfallModel model;
    protected WaterfallMaterial material;
    protected Material matInternal;
    protected readonly float headerWidth = 120f;

    public UIMaterialData(MaterialData data, WaterfallModel modelToEdit, WaterfallMaterial matToEdit)
    {
      name = data.name;
      parameterData = data;
      model = modelToEdit;
      material = matToEdit;
      matInternal = material.materials[0];

    }

    public virtual void Draw() { }
  }

  public class UIMaterialFloatData : UIMaterialData
  {
    public float value;
    private UILabelledFloatSlider slider;

    public UIMaterialFloatData(MaterialData data, WaterfallModel modelToEdit, WaterfallMaterial matToEdit) : base(data, modelToEdit, matToEdit)
    {
      value = matInternal.GetFloat(name);
      slider = new(name, value, parameterData.floatRange, UpdateFloat, headerWidth);
    }
    public override void Draw()
    {
      if (slider != null)
      slider.Draw();
    }

    public void UpdateFloat(float newFloat)
    {
      value = newFloat;
      model.SetFloat(material, name, value);
    }
  }
  public class UIMaterialVector4Data : UIMaterialData
  {
    public Vector4 value;
    private string[] valueString;
    public UIMaterialVector4Data(MaterialData data, WaterfallModel modelToEdit, WaterfallMaterial matToEdit) : base(data, modelToEdit, matToEdit)
    {
      value = matInternal.GetVector(name);
      valueString = new[] { $"{value.x}", $"{value.y}", $"{value.z}", $"{value.w}" };
    }
    public override void Draw()
    {

      Vector3 vecVal;
      GUILayout.BeginHorizontal();
      GUILayout.Label(name, GUILayout.Width(headerWidth));
      vecVal = UIUtils.Vector3InputField(GUILayoutUtility.GetRect(200f, 30f), value, valueString, GUI.skin.label, GUI.skin.textArea);
      var temp = new Vector4(vecVal.x, vecVal.y, vecVal.z, 0f);
      if (temp != value)
      {
        value = temp;
        material.SetVector4(name, value);
      }
      GUILayout.EndHorizontal();
    }
  }
  public class UIMaterialColorData : UIMaterialData
  {
    public Color color;
    private Texture2D colorTexture;

    public UIMaterialColorData(MaterialData data, WaterfallModel modelToEdit, WaterfallMaterial matToEdit) : base(data, modelToEdit, matToEdit)
    {
      color = matInternal.GetColor(name);
      GenerateTextures();
    }
    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(name, GUILayout.Width(headerWidth));
      GUILayout.Space(10);

      if (GUILayout.Button("", GUILayout.Width(60)))
      {
        WaterfallUI.Instance.OpenColorEditWindow(color, UpdateColor);
        Utils.Log($"[UIMaterialColorData] Open Color Picker for {name}");
      }

      var tRect = GUILayoutUtility.GetLastRect();
      tRect = new(tRect.x + 3, tRect.y + 3, tRect.width - 6, tRect.height - 6);
      GUI.DrawTexture(tRect, colorTexture);
      GUILayout.EndHorizontal();
    }
    protected void UpdateColor(Color c)
    {
      Utils.Log($"[UIMaterialColorData] Applied color {name}");
      color = c;
      colorTexture = TextureUtils.GenerateColorTexture(64, 32, color);
      model.SetColor(material, name, color);
    }

    protected void GenerateTextures()
    {
      colorTexture = TextureUtils.GenerateColorTexture(32, 32, color);
    }
  }
  public class UIMaterialTextureData : UIMaterialData
  {
    public Vector2 textureScale;
    public Vector2 textureOffset;
    public string texture;

    private readonly string[] textureScaleString;
    private readonly string[] textureOffsetString;
    public UIMaterialTextureData(MaterialData data, WaterfallModel modelToEdit, WaterfallMaterial matToEdit) : base(data, modelToEdit, matToEdit)
    {
      if (matInternal.GetTexture(name))
      {
        texture = matInternal.GetTexture(name).name;
      }
      else
      {
        texture = null;
      }
      textureScale = matInternal.GetTextureScale(name);
      textureOffset = matInternal.GetTextureOffset(name);
      textureOffsetString = new[] { $"{textureOffset.x}", $"{textureOffset.y}" };
      textureScaleString = new[] { $"{textureScale.x}", $"{textureScale.y}" };
    }

    public override void Draw()
    {

      GUILayout.BeginHorizontal();
      GUILayout.Label(name, GUILayout.Width(headerWidth));

      GUILayout.Label("Texture Path");
      // Button to set that we are toggling the texture picker
      if (GUILayout.Button(texture))
      {
        WaterfallUI.Instance.OpenTextureEditWindow(texture, UpdateTexture);
        Utils.Log("[TP] Open Window");
      }

      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Space(headerWidth);
      GUILayout.Label("UV Scale", GUILayout.Width(headerWidth));
      textureScale = UIUtils.Vector2InputField(GUILayoutUtility.GetRect(200f, 30f), textureScale, textureScaleString, GUI.skin.label, GUI.skin.textArea, out bool changed);
      if (changed)
      {
        model.SetTextureScale(material, name, textureScale);
      }

      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Space(headerWidth);
      GUILayout.Label("UV Offset", GUILayout.Width(headerWidth));
      textureOffset = UIUtils.Vector2InputField(GUILayoutUtility.GetRect(200f, 30f), textureOffset, textureOffsetString, GUI.skin.label, GUI.skin.textArea, out changed);
      if (changed)
      {
        model.SetTextureOffset(material, name, textureOffset);
      }

      GUILayout.EndHorizontal();
    }
    protected void UpdateTexture(string newTexture)
    {
      texture = newTexture;
      Utils.Log($"[UIMaterialTextureData] Applied texture {name}");
      model.SetTexture(material, name, texture);
    }
  }
}
