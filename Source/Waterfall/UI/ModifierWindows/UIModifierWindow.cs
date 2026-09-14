using System;
using UnityEngine;

namespace Waterfall.UI
{
  public class UIModifierWindow : UIPopupWindow
  {
    public UIModifierWindow(EffectModifier mod, bool show) : base(show)
    {
      controllerNames = mod.parentEffect.parentModule.GetControllerNames();
      for (int i = 0; i < controllerNames.Length; i++)
      {
        if (controllerNames[i] == mod.controllerName)
          controllerFlag = i;
      }

      combineModes    = Enum.GetNames(typeof(EffectModifierMode));
      combineModeFlag = (int)mod.effectMode;
      modifier        = mod;
      randomText      = modifier.randomnessScale.ToString();
      windowPos       = new(WaterfallUI.Instance.WindowPosition.xMax + 5f, WaterfallUI.Instance.WindowPosition.yMin, 500f, 100f);
    }

    public virtual void UpdateCurves(FastFloatCurve newCurve, string tag) { }

    /// <summary>
    ///   Draw the UI
    /// </summary>
    public override void Draw()
    {
      base.Draw();
    }

    /// <summary>
    ///   Draws the title panel (title, close button)
    /// </summary>
    protected virtual void DrawTitle()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(windowTitle, UIResources.GetStyle("window_header"), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f), GUILayout.MinWidth(350f));
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

    /// <summary>
    ///   Draws the common header area
    /// </summary>
    protected virtual void DrawHeader()
    {
      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();
      GUILayout.Label($"Transform Target {modifier.transformName}");
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Modifier Name");
      modifier.fxName = GUILayout.TextArea(modifier.fxName);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Controller");
      controllerFlag          = GUILayout.SelectionGrid(controllerFlag, controllerNames, controllerNames.Length, UIResources.GetStyle("radio_text_button"));
      modifier.controllerName = controllerNames[controllerFlag];
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Combine Mode");
      combineModeFlag     = GUILayout.SelectionGrid(combineModeFlag, combineModes, combineModes.Length, UIResources.GetStyle("radio_text_button"));
      modifier.effectMode = (EffectModifierMode)combineModeFlag;
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      modifier.useRandomness = GUILayout.Toggle(modifier.useRandomness, "Use Randomness");
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      if (modifier.useRandomness)
      {
        GUILayout.Label("Controller");
        modifier.randomnessController = GUILayout.TextArea(modifier.randomnessController);
        GUILayout.Label("Scale");
        randomText = GUILayout.TextArea(randomText);
        if (Single.TryParse(randomText, out modifier.randomnessScale)) { }
      }

      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
    }

    /// <summary>
    ///   Draws the child-specific items
    /// </summary>
    protected virtual void DrawModifierPanel() { }

    /// <summary>
    ///   Draws the child-specific items
    /// </summary>
    protected virtual void UpdateModifierPanel() { }

    /// <summary>
    ///   Initialize the UI widgets, do localization, set up styles
    /// </summary>
    protected override void InitUI()
    {
      base.InitUI();
      windowTitle = "Modifier Editor";
    }


    /// <summary>
    ///   Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected override void DrawWindow(int windowId)
    {
      DrawTitle();
      // Draw the header/tab controls
      DrawHeader();
      DrawModifierPanel();
      GUI.DragWindow();
    }

    /// <summary>
    ///   Spawns the float editor panel to edit a float
    /// </summary>
    /// <param name="toEdit"></param>
    protected void EditCurve(FastFloatCurve toEdit, CurveUpdateFunction updateFunction)
    {
      Utils.Log($"Started editing curve {toEdit}");
      curveEditor = WaterfallUI.Instance.OpenCurveEditor(toEdit, updateFunction);
    }

    /// <summary>
    ///   Spawns the float editor panel to edit a float
    /// </summary>
    /// <param name="toEdit"></param>
    protected void EditCurve(FastFloatCurve toEdit, string tag)
    {
      Utils.Log($"Started editing curve {toEdit}");
      curveEditor = WaterfallUI.Instance.OpenCurveEditor(toEdit, this, tag);
    }

    protected void CopyCurve(FastFloatCurve toCopy)
    {
      UICopy.CopyFastFloatCurve(toCopy);
    }

    protected void PasteCurve(FastFloatCurve value, out FastFloatCurve target)
    {
      if (UICopy.CurveCopyBuffer != null)
        target = UICopy.CurveCopyBuffer;
      else
        target = value;
      UpdateModifierPanel();
    }

    #region GUI Variables

    protected string     windowTitle = "";
    protected int        texWidth    = 80;
    protected int        texHeight   = 30;
    protected float      hdrWidth    = 75f;
    protected float      copyWidth   = 40f;
    protected FastFloatCurve copyBuffer;

    #endregion

    #region GUI Widgets

    protected UICurveEditWindow curveEditor;

    #endregion

    #region Modifier Data

    public           EffectModifier modifier;
    private readonly string[]       combineModes;
    private readonly string[]       controllerNames;
    private          int            combineModeFlag;
    private          int            controllerFlag;
    protected        int            selectionFlag = 0;
    protected        string         randomText;

    #endregion
  }
}