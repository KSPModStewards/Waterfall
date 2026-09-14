using System;
using System.Linq;
using UnityEngine;

namespace Waterfall.UI
{
  public class UILightEditWindow : UIPopupWindow
  {
    protected string windowTitle = "";

    private readonly string[] typeOptions = new string[2] { "Spot", "Point" };
    private WaterfallModel model;
    private WaterfallLight light;

    private float intensityValue;
    private float rangeValue;
    private float angleValue;

    private Color colorValue;
    private bool colorEdit;

    private string intensityString;
    private string rangeString;
    private string angleString;
    private string[] colorString;
    private int typeFlag;

    private Texture2D colorTexture;

    public UILightEditWindow(WaterfallModel modelToEdit, bool show) : base(show)
    {
      model = modelToEdit;
      Utils.Log($"[UILightEditWindow]: Started editing lights on {modelToEdit}");


      light = modelToEdit.lights.First();
      GetLightValues();
      if (!showWindow)
        WindowPosition = new(Screen.width / 2 - 200, Screen.height / 2f, 400, 100);
    }

    public void ChangeLight(WaterfallModel modelToEdit)
    {
      model = modelToEdit;
      Utils.Log($"[UILightEditWindow]: Started editing lights on {modelToEdit}");

      light = modelToEdit.lights.First();
      GetLightValues();
      showWindow = true;
      GUI.BringWindowToFront(windowID);
    }

    protected override void InitUI()
    {
      windowTitle = "Light Editor";
      base.InitUI();
    }

    protected override void DrawWindow(int windowId)
    {
      // Draw the header/tab controls
      DrawTitle();
      DrawLightEdit();
      GUI.DragWindow();
    }

    protected void DrawTitle()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(windowTitle, UIResources.GetStyle("window_header"), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f));

      GUILayout.FlexibleSpace();

      var buttonRect = GUILayoutUtility.GetRect(22f, 22f);
      GUI.color = UIResources.GetColor("cancel_color");
      if (GUI.Button(buttonRect, "", UIResources.GetStyle("button_cancel")))
      {
        ToggleWindow();
      }

      GUI.DrawTextureWithTexCoords(buttonRect, UIResources.GetIcon("cancel").iconAtlas, UIResources.GetIcon("cancel").iconRect);
      GUI.color = Color.white;
      GUILayout.EndHorizontal();
    }

    protected void DrawLightEdit()
    {
      float headerWidth = 120f;
      bool delta = false;
      float sliderVal;
      string textVal;

      GUILayout.Label("<b>Light Parameters</b>");

      GUILayout.BeginHorizontal();
      GUILayout.Label("Light Color", GUILayout.Width(headerWidth));
      GUILayout.Space(10);

      // Button to set that we are toggling the color picker
      if (GUILayout.Button("", GUILayout.Width(60)))
      {
        WaterfallUI.Instance.OpenColorEditWindow(colorValue, UpdateColor);
        Utils.Log("[CP] Open Window");
      }

      var tRect = GUILayoutUtility.GetLastRect();
      tRect = new(tRect.x + 3, tRect.y + 3, tRect.width - 6, tRect.height - 6);
      GUI.DrawTexture(tRect, colorTexture);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Light Type", GUILayout.Width(headerWidth));
      int newFlag = GUILayout.SelectionGrid(typeFlag,
                                            typeOptions,
                                            2,
                                            UIResources.GetStyle("radio_text_button"));

      if (newFlag != typeFlag)
      {
        typeFlag = newFlag;
        if (typeFlag == 1)
          model.SetLightType(light, LightType.Point);
        if (typeFlag == 0)
          model.SetLightType(light, LightType.Spot);
      }

      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Intensity", GUILayout.Width(headerWidth));
      sliderVal = GUILayout.HorizontalSlider(intensityValue, 0f, 10f);
      if (sliderVal != intensityValue)
      {
        intensityValue = sliderVal;
        intensityString = sliderVal.ToString();
        model.SetLightIntensity(light, intensityValue);
      }

      textVal = GUILayout.TextArea(intensityString, GUILayout.Width(90f));
      if (textVal != intensityString)
      {
        float outVal;
        if (Single.TryParse(textVal, out outVal))
        {
          intensityValue = outVal;
          model.SetLightIntensity(light, intensityValue);
        }

        intensityString = textVal;
      }

      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      GUILayout.Label("Range", GUILayout.Width(headerWidth));
      sliderVal = GUILayout.HorizontalSlider(rangeValue, 0f, 100f);
      if (sliderVal != rangeValue)
      {
        rangeValue = sliderVal;
        rangeString = sliderVal.ToString();
        model.SetLightRange(light, rangeValue);
      }

      textVal = GUILayout.TextArea(rangeString, GUILayout.Width(90f));
      if (textVal != rangeString)
      {
        float outVal;
        if (Single.TryParse(textVal, out outVal))
        {
          rangeValue = outVal;
          model.SetLightRange(light, rangeValue);
        }

        rangeString = textVal;
      }

      GUILayout.EndHorizontal();
      if (typeFlag == 0)
      {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spot Angle", GUILayout.Width(headerWidth));
        sliderVal = GUILayout.HorizontalSlider(angleValue, 0f, 100f);
        if (sliderVal != angleValue)
        {
          angleValue = sliderVal;
          angleString = sliderVal.ToString();
          model.SetLightAngle(light, angleValue);
        }

        textVal = GUILayout.TextArea(angleString, GUILayout.Width(90f));
        if (textVal != angleString)
        {
          float outVal;
          if (Single.TryParse(textVal, out outVal))
          {
            angleValue = outVal;
            model.SetLightAngle(light, angleValue);
          }

          angleString = textVal;
        }

        GUILayout.EndHorizontal();
      }
    }

    public void UpdateColor(Color c)
    {
      colorValue = c;
      colorTexture = TextureUtils.GenerateColorTexture(64, 32, colorValue);
      model.SetLightColor(light, colorValue);
    }
    private void GetLightValues()
    {
      intensityValue = light.intensity;
      intensityString = light.intensity.ToString();

      rangeString = light.range.ToString();
      rangeValue = light.range;

      angleString = light.angle.ToString();
      angleValue = light.angle;

      colorValue = light.color;
      colorString = MaterialUtils.ColorToStringArray(colorValue);
      colorTexture = TextureUtils.GenerateColorTexture(32, 32, colorValue);

      if (light.lightType == LightType.Point)
      {
        typeFlag = 1;
      }
      else
      {
        typeFlag = 0;
      }
    }
  }
}