using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall.UI
{
  public class UIParticleData
  {
    public string name;
    public ParticleData parameterData;
    public WaterfallParticle particle;

    protected readonly Vector2 curveButtonDims = new(100f, 50f);
    protected readonly float headerWidth = 200f;
    protected int curveTexWidth = 80;
    protected int curveTexHeight = 30;
    protected int modeID;
    protected string[] modeList;
    protected int savedModeID;
    public UIParticleData(ParticleData data, WaterfallParticle system)
    {
      name = data.name;
      parameterData = data;
      particle = system;
    }

    public virtual void Draw() { }



  }
  public class UIFloatParticleData : UIParticleData
  {
    public float constant1;
    public float constant2;

    public string constant1String;
    public string constant2String;

    public FastFloatCurve curve1;
    public FastFloatCurve curve2;

    protected ParticleSystemCurveMode curveMode;

    protected readonly CurveUpdateFunction curve1Function;
    protected readonly CurveUpdateFunction curve2Function;

    private Texture2D miniCurve1;
    private Texture2D miniCurve2;


    public UIFloatParticleData(ParticleData data, WaterfallParticle system) : base(data, system)
    {
      modeList = data.validModes.Select(x => x.ToString()).ToArray();

      curveMode = ParticleUtils.GetParticleSystemMode(data.name, system.systems[0]);
      modeID = modeList.IndexOf(curveMode.ToString());

      InitParticleData();

      if (modeList[savedModeID] == "Curve" || modeList[savedModeID] == "TwoCurves")
      {
        GenerateCurveThumbs();
      }
      curve1Function = UpdateCurve1;
      curve2Function = UpdateCurve2;
    }

    protected void InitParticleData()
    {
      curve1 = new FastFloatCurve();
      curve2 = new FastFloatCurve();
      if (curveMode == ParticleSystemCurveMode.Constant)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], out constant1);
        constant1String = constant1.ToString();
        constant2 = constant1;
        constant2String = constant2.ToString();
      }
      if (curveMode == ParticleSystemCurveMode.Curve)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], ref curve1);
      }
      if (curveMode == ParticleSystemCurveMode.TwoConstants)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], out constant1, out constant2);
        constant1String = constant1.ToString();
        constant2String = constant2.ToString();
      }
      if (curveMode == ParticleSystemCurveMode.TwoCurves)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], ref curve1, ref curve2);
      }
    }
    public override void Draw()
    {
      GUILayout.BeginVertical();
      DrawSelector();
      if (modeList[savedModeID] == "Constant")
      {
        DrawSingleMode();
      }
      if (modeList[savedModeID] == "TwoConstants")
      {
        DrawRangeMode();
      }
      if (modeList[savedModeID] == "Curve")
      {
        DrawCurveMode();
      }
      if (modeList[savedModeID] == "TwoCurves")
      {
        DrawCurveRangeMode();
      }
      GUILayout.EndVertical();
    }
    void DrawSelector()
    {
      GUILayout.BeginHorizontal();

      GUILayout.Label($"<b> -> {name}</b>", GUILayout.Width(headerWidth));
      GUILayout.FlexibleSpace();
      modeID = GUILayout.SelectionGrid(modeID, modeList, 2, UIResources.GetStyle("radio_text_button"));
      if (modeID != savedModeID)
      {
        savedModeID = modeID;

        curveMode = (ParticleSystemCurveMode)Enum.Parse(typeof(ParticleSystemCurveMode), modeList[modeID]);
        ParticleUtils.SetParticleSystemMode(name, particle.systems[0], curveMode);
        particle.SetParticleCurveMode(name, curveMode);
        if (modeList[savedModeID] == "Curve" || modeList[savedModeID] == "CurveRange")
        {
          GenerateCurveThumbs();
        }
      }
      GUILayout.EndHorizontal();
    }
    void DrawSingleMode()
    {
      float sliderVal;
      string textVal;

      GUILayout.BeginHorizontal();
      sliderVal = GUILayout.HorizontalSlider(constant1,
                                             parameterData.floatRange.x,
                                             parameterData.floatRange.y);
      if (sliderVal != constant1)
      {
        constant1 = sliderVal;
        constant1String = sliderVal.ToString();
        particle.SetParticleValue(name, constant1);
      }

      textVal = GUILayout.TextArea(constant1String, GUILayout.Width(90f));
      if (textVal != constant1String)
      {
        if (Single.TryParse(textVal, out float outVal))
        {
          constant1 = outVal;
          particle.SetParticleValue(name, constant1);
        }
        constant1String = textVal;
      }
      GUILayout.EndHorizontal();
    }
    void DrawRangeMode()
    {
      float sliderValX;
      string textValX;
      float sliderValY;
      string textValY;

      // Low End
      GUILayout.BeginHorizontal();
      GUILayout.Label("<b>Low:</b>");
      sliderValX = GUILayout.HorizontalSlider(constant1,
                                             parameterData.floatRange.x,
                                             parameterData.floatRange.y);
      if (sliderValX != constant1)
      {
        constant1 = sliderValX;
        constant1String = sliderValX.ToString();
        particle.SetParticleValue(name, constant1, constant2);
      }

      textValX = GUILayout.TextArea(constant1String, GUILayout.Width(90f));
      if (textValX != constant1String)
      {
        float outVal;
        if (Single.TryParse(textValX, out outVal))
        {
          constant1 = outVal;
          particle.SetParticleValue(name, constant1, constant2);
        }
        constant1String = textValX;
      }
      GUILayout.EndHorizontal();
      // High end
      GUILayout.BeginHorizontal();
      GUILayout.Label("<b>High:</b>");
      sliderValY = GUILayout.HorizontalSlider(constant2,
                                             parameterData.floatRange.x,
                                             parameterData.floatRange.y);
      if (sliderValY != constant2)
      {
        constant2 = sliderValY;
        constant2String = sliderValY.ToString();
        particle.SetParticleValue(name, constant1, constant2);
      }

      textValY = GUILayout.TextArea(constant2String, GUILayout.Width(90f));
      if (textValY != constant2String)
      {
        float outVal;
        if (Single.TryParse(textValY, out outVal))
        {
          constant2 = outVal;
          particle.SetParticleValue(name, constant1, constant2);
        }
        constant2String = textValY;
      }
      GUILayout.EndHorizontal();

    }
    void DrawCurveMode()
    {
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

      var buttonRect = GUILayoutUtility.GetRect(curveButtonDims.x, curveButtonDims.y, GUILayout.Width(125));
      var imageRect = new Rect(buttonRect.xMin + 10f, buttonRect.yMin + 10, buttonRect.width - 20, buttonRect.height - 20);
      if (GUI.Button(buttonRect, ""))
      {
        EditCurve(curve1, curve1Function);
      }
      GUI.DrawTexture(imageRect, miniCurve1);
      GUILayout.EndHorizontal();
    }
    void DrawCurveRangeMode()
    {
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      GUILayout.Label("<b>Low:</b>");
      var buttonRect = GUILayoutUtility.GetRect(curveButtonDims.x, curveButtonDims.y, GUILayout.Width(125));
      var imageRect = new Rect(buttonRect.xMin + 10f, buttonRect.yMin + 10, buttonRect.width - 20, buttonRect.height - 20);
      if (GUI.Button(buttonRect, ""))
      {
        EditCurve(curve1, curve1Function);
      }
      GUI.DrawTexture(imageRect, miniCurve1);
      GUILayout.Label("<b>High:</b>");
      buttonRect = GUILayoutUtility.GetRect(curveButtonDims.x, curveButtonDims.y, GUILayout.Width(125));
      imageRect = new Rect(buttonRect.xMin + 10f, buttonRect.yMin + 10, buttonRect.width - 20, buttonRect.height - 20);
      if (GUI.Button(buttonRect, ""))
      {
        EditCurve(curve2, curve2Function);
      }
      GUI.DrawTexture(imageRect, miniCurve2);
      GUILayout.EndHorizontal();
    }
    protected void EditCurve(FastFloatCurve toEdit, CurveUpdateFunction updateFunction)
    {
      Utils.Log($"Started editing curve {toEdit}");
      WaterfallUI.Instance.OpenCurveEditor(toEdit, updateFunction);
    }
    protected void UpdateCurve1(FastFloatCurve curve)
    {
      curve1 = curve;
      if (curveMode == ParticleSystemCurveMode.Curve)
      {
        particle.SetParticleValue(name, curve1);
      }
      else if (curveMode == ParticleSystemCurveMode.TwoCurves)
      {
        particle.SetParticleValue(name, curve1, curve2);
      }
      GenerateCurveThumbs();
    }
    protected void UpdateCurve2(FastFloatCurve curve)
    {
      curve2 = curve;
      if (curveMode == ParticleSystemCurveMode.TwoCurves)
      {
        particle.SetParticleValue(name, curve1, curve2);
      }
      GenerateCurveThumbs();
    }
    protected void GenerateCurveThumbs()
    {
      if (curve1 != null)
      {
        miniCurve1 = GraphUtils.GenerateCurveTexture(curveTexWidth, curveTexHeight, curve1, Color.red);
      }
      if (curve2 != null)
      {
        miniCurve2 = GraphUtils.GenerateCurveTexture(curveTexWidth, curveTexHeight, curve2, Color.green);
      }
    }
  }

  public class UIColorParticleData : UIParticleData
  {
    public Color color1;
    public Color color2;
    public Gradient gradient1;
    public Gradient gradient2;
    protected ParticleSystemGradientMode colorMode;

    protected readonly GradientUpdateFunction gradient1Function;
    protected readonly GradientUpdateFunction gradient2Function;

    private Texture2D colorTexture1;
    private Texture2D colorTexture2;


    private Texture2D gradientTexture1;
    private Texture2D gradientTexture2;

    public UIColorParticleData(ParticleData data, WaterfallParticle system) : base(data, system)
    {
      modeList = data.validModes.Select(x => x.ToString()).ToArray();

      colorMode = ParticleUtils.GetParticleSystemColorMode(data.name, system.systems[0]);
      modeID = modeList.IndexOf(colorMode.ToString());

      InitParticleData();


      colorTexture1 = TextureUtils.GenerateColorTexture(64, 32, color1);
      colorTexture2 = TextureUtils.GenerateColorTexture(64, 32, color2);
      gradientTexture1 = TextureUtils.GenerateGradientTexture(curveTexWidth, curveTexHeight, Color.clear, Color.white);
      gradientTexture2 = TextureUtils.GenerateGradientTexture(curveTexWidth, curveTexHeight, Color.clear, Color.white);

      gradient1Function = UpdateGradient1;
      gradient2Function = UpdateGradient2;
    }

    protected void InitParticleData()
    {
      color1 = new();
      color2 = new();
      gradient1 = new();
      gradient2 = new();
      if (colorMode == ParticleSystemGradientMode.Color)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], out color1);
      }
      if (colorMode == ParticleSystemGradientMode.Gradient)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], out gradient1);
      }
      if (colorMode == ParticleSystemGradientMode.TwoColors)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], out color1, out color2);
      }
      if (colorMode == ParticleSystemGradientMode.TwoGradients)
      {
        ParticleUtils.GetParticleSystemValue(name, particle.systems[0], out gradient1, out gradient2);
      }
    }

    public override void Draw()
    {
      GUILayout.BeginVertical();
      DrawSelector();
      if (modeList[savedModeID] == "Color")
      {
        DrawColorMode();
      }
      if (modeList[savedModeID] == "TwoColors")
      {
        DrawTwoColorMode();
      }
      if (modeList[savedModeID] == "Gradient")
      {
        DrawGradientMode();
      }
      if (modeList[savedModeID] == "TwoGradients")
      {
        DrawTwoGradientMode();
      }
      GUILayout.EndVertical();
    }
    void DrawSelector()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label($"<b>{name}</b>", GUILayout.Width(headerWidth));
      GUILayout.FlexibleSpace();
      modeID = GUILayout.SelectionGrid(modeID, modeList, 2, UIResources.GetStyle("radio_text_button"));
      if (modeID != savedModeID)
      {
        savedModeID = modeID;
        colorMode = (ParticleSystemGradientMode)Enum.Parse(typeof(ParticleSystemGradientMode), modeList[modeID]);
        particle.SetParticleColorMode(name, colorMode);
        GenerateColorThumbs();
        GenerateGradientThumbs();        
      }
      GUILayout.EndHorizontal();
    }
    void DrawColorMode()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(name, GUILayout.Width(headerWidth));
      GUILayout.FlexibleSpace();

      // Button to set that we are toggling the color picker
      if (GUILayout.Button("", GUILayout.Width(60)))
      {
        WaterfallUI.Instance.OpenColorEditWindow(color1, UpdateColor1);
      }

      var tRect = GUILayoutUtility.GetLastRect();
      tRect = new(tRect.x + 3, tRect.y + 3, tRect.width - 6, tRect.height - 6);
      GUI.DrawTexture(tRect, colorTexture1);

      GUILayout.EndHorizontal();
    }
    void DrawTwoColorMode()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(name, GUILayout.Width(headerWidth));
      GUILayout.FlexibleSpace();

      GUILayout.Label("<b>Low:</b>");
      // Button to set that we are toggling the color picker
      if (GUILayout.Button("", GUILayout.Width(60)))
      {
        WaterfallUI.Instance.OpenColorEditWindow(color1, UpdateColor1);
      }

      var tRect = GUILayoutUtility.GetLastRect();
      tRect = new(tRect.x + 3, tRect.y + 3, tRect.width - 6, tRect.height - 6);
      GUI.DrawTexture(tRect, colorTexture1);


      GUILayout.Label("<b>High:</b>");
      // Button to set that we are toggling the color picker
      if (GUILayout.Button("", GUILayout.Width(60)))
      {
        WaterfallUI.Instance.OpenColorEditWindow(color2, UpdateColor2);
      }

      tRect = GUILayoutUtility.GetLastRect();
      tRect = new(tRect.x + 3, tRect.y + 3, tRect.width - 6, tRect.height - 6);
      GUI.DrawTexture(tRect, colorTexture2);
      GUILayout.EndHorizontal();
    }

    void DrawGradientMode()
    {
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

      var buttonRect = GUILayoutUtility.GetRect(curveButtonDims.x, curveButtonDims.y, GUILayout.Width(125));
      var imageRect = new Rect(buttonRect.xMin + 10f, buttonRect.yMin + 10, buttonRect.width - 20, buttonRect.height - 20);
      if (GUI.Button(buttonRect, ""))
      {
        EditGradient(gradient1, gradient1Function);
      }
      GUI.DrawTexture(imageRect, gradientTexture1);
      GUILayout.EndHorizontal();
    }

    void DrawTwoGradientMode()
    {

    }
    protected void UpdateColor1(Color color)
    {
      color1 = color;
      if (colorMode == ParticleSystemGradientMode.Color)
      {
        particle.SetParticleValue(name, color1);
      }
      else if (colorMode == ParticleSystemGradientMode.TwoColors)
      {
        particle.SetParticleValue(name, color1, color2);
      }
      GenerateColorThumbs();
    }
    protected void UpdateColor2(Color color)
    {
      color2 = color;
      if (colorMode == ParticleSystemGradientMode.TwoColors)
      {
        particle.SetParticleValue(name, color1, color2);
      }
      GenerateColorThumbs();
    }
    protected void UpdateGradient1(Gradient curve, float lower, float upper)
    {
      gradient1 = curve;
      if (colorMode == ParticleSystemGradientMode.Gradient)
      {
        particle.SetParticleValue(name, gradient1);
      }
      else if (colorMode == ParticleSystemGradientMode.TwoGradients)
      {
        particle.SetParticleValue(name, gradient1, gradient2);
      }
      GenerateGradientThumbs();
    }
    protected void UpdateGradient2(Gradient grad, float lower, float upper)
    {
      gradient2 = grad;
      if (colorMode == ParticleSystemGradientMode.TwoGradients)
      {
        particle.SetParticleValue(name, gradient1, gradient2);
      }
      GenerateGradientThumbs();
    }
    protected void EditGradient(Gradient toEdit, GradientUpdateFunction updateFunction)
    {
      Utils.Log($"Started editing gradient {toEdit}");
      WaterfallUI.Instance.OpenGradientEditor(toEdit, updateFunction);
    }
    protected void GenerateGradientThumbs()
    {
      if (gradient1 != null)
      {
        gradientTexture1 = TextureUtils.GenerateGradientTexture(curveTexWidth, curveTexHeight, gradient1);
      }
      if (gradient2 != null)
      {
        gradientTexture2 = TextureUtils.GenerateGradientTexture(curveTexWidth, curveTexHeight, gradient2);
      }
    }
    protected void GenerateColorThumbs()
    {
      if (color1 != null)
      {
        colorTexture1 = TextureUtils.GenerateColorTexture(64, 32, color1);
      }
      if (color2 != null)
      {
        colorTexture2 = TextureUtils.GenerateColorTexture(64, 32, color2);
      }
    }
  }

}
