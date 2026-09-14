using System;
using UnityEngine;

namespace Waterfall.UI.EffectControllersUI
{
  public class RemapControllerUIOptions : DefaultEffectControllerUIOptions<RemapController>
  {
    private readonly Vector2 curveButtonDims = new(100f, 50f);

    private readonly int texWidth = 80;
    private readonly int texHeight = 30;
    private FastFloatCurve mappingCurve;
    private string sourceController;
    private Texture2D miniCurve;
    private UICurveEditWindow curveEditor;
    private string rampRateUp;
    private string rampRateDown;

    public RemapControllerUIOptions()
    {
      sourceController = "";
      mappingCurve = new();
      mappingCurve.Add(0f, 0f);
      mappingCurve.Add(1f, 1f);
      GenerateCurveThumbs();
    }

    public override void DrawOptions()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Source controller", UIResources.GetStyle("data_header"));
      sourceController = GUILayout.TextArea(sourceController);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Ramp Rate Up", UIResources.GetStyle("data_header"), GUILayout.MaxWidth(160f));
      rampRateUp = GUILayout.TextArea(rampRateUp, GUILayout.MaxWidth(60f));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Ramp Rate Down", UIResources.GetStyle("data_header"), GUILayout.MaxWidth(160f));
      rampRateDown = GUILayout.TextArea(rampRateDown, GUILayout.MaxWidth(60f));
      GUILayout.EndHorizontal();

      var buttonRect = GUILayoutUtility.GetRect(curveButtonDims.x, curveButtonDims.y);
      var imageRect = new Rect(buttonRect.xMin + 10f, buttonRect.yMin + 10, buttonRect.width - 20, buttonRect.height - 20);
      if (GUI.Button(buttonRect, ""))
      {
        EditCurve(mappingCurve, UpdateEventCurve);
      }

      GUI.DrawTexture(imageRect, miniCurve);
    }

    protected override void LoadOptions(RemapController controller)
    {
      mappingCurve = controller.mappingCurve;
      sourceController = controller.sourceController;
      rampRateUp = controller.responseRateUp.ToString();
      rampRateDown = controller.responseRateDown.ToString();

      GenerateCurveThumbs();
    }

    protected override RemapController CreateControllerInternal() =>
      new()
      {
        sourceController = sourceController,
        mappingCurve = mappingCurve,
        responseRateUp = ParseOrZero(rampRateUp),
        responseRateDown = ParseOrZero(rampRateDown),
      };

    private void EditCurve(FastFloatCurve toEdit, CurveUpdateFunction function)
    {
      Utils.Log($"Started editing curve {toEdit}");
      curveEditor = WaterfallUI.Instance.OpenCurveEditor(toEdit, function);
    }

    private void GenerateCurveThumbs()
    {
      miniCurve = GraphUtils.GenerateCurveTexture(texWidth, texHeight, mappingCurve, Color.green);
    }

    private void UpdateEventCurve(FastFloatCurve curve)
    {
      mappingCurve = curve;
      GenerateCurveThumbs();
    }
  }
}
