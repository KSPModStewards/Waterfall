using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Waterfall.UI.EffectControllersUI;

namespace Waterfall.UI
{
  public class WaterfallUI : UIBaseWindow
  {
    protected List<UIModifierWindow> editWindows = new();
    protected bool exportsOpen;

    private string[] modulesString;

    protected bool templatesOpen = false;
    private string[] templatesString;
    public static WaterfallUI Instance { get; private set; }

    protected override void Awake()
    {
      base.Awake();
      Instance = this;
    }

    protected override void Start()
    {
      base.Start();
      windowPos = new(200f, 200f, 800f, 600f);
      modelOffsetString = new[] { modelOffset.x.ToString(), modelOffset.y.ToString(), modelOffset.z.ToString() };
      modelRotationString = new[] { modelRotation.x.ToString(), modelRotation.y.ToString(), modelRotation.z.ToString() };
      modelScaleString = new[] { modelScale.x.ToString(), modelScale.y.ToString(), modelScale.z.ToString() };
      StartCoroutine(DelayedStart());
    }

    public void Update()
    {
      //}
      for (int i = 0; i < effectUIWidgets.Count; i++)
      {
        effectUIWidgets[i].Update();
      }

      if (colorPickWindow != null)
      {
        colorPickWindow.Update();
      }

      if (texturePickWindow != null)
      {
        texturePickWindow.Update();
      }

      if (selectedTemplate != null)
      {
        if (modelOffset != selectedTemplate.position || modelRotation != selectedTemplate.rotation || modelScale != selectedTemplate.scale)
        {
          selectedTemplate.position = modelOffset;
          selectedTemplate.rotation = modelRotation;
          selectedTemplate.scale = modelScale;
        }
      }
    }


    /// <summary>
    ///   Initialize the UI widgets, do localization, set up styles
    /// </summary>
    protected override void InitUI()
    {
      windowTitle = "WaterfallFX Editor";
      base.InitUI();
    }

    private IEnumerator DelayedStart()
    {
      yield return 5;
      GetVesselData();
    }

    /// <summary>
    ///   Draw the UI
    /// </summary>
    protected override void Draw()
    {
      base.Draw();
      foreach (var modWin in editWindows)
      {
        modWin.Draw();
      }
      if (curveEditWindow != null)
      {
        curveEditWindow.Draw();
      }
      if (gradientEditWindow != null)
      {
        gradientEditWindow.Draw();
      }
      if (materialEditWindow != null)
      {
        materialEditWindow.Draw();
      }

      if (particleEditWindow != null)
      {
        particleEditWindow.Draw();
      }

      if (lightEditWindow != null)
      {
        lightEditWindow.Draw();
      }

      if (modifierPopupWindow != null)
      {
        modifierPopupWindow.Draw();
      }

      if (effectAddWindow != null)
      {
        effectAddWindow.Draw();
      }

      if (controlAddWindow != null)
      {
        controlAddWindow.Draw();
      }

      if (colorPickWindow != null)
      {
        colorPickWindow.Draw();
      }

      if (texturePickWindow != null)
      {
        texturePickWindow.Draw();
      }
    }


    /// <summary>
    ///   Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected override void DrawWindow(int windowId)
    {
      // Draw the header/tab controls

      if (selectedModule != null)
      {
        DrawHeader();
        GUILayout.BeginHorizontal();
        DrawPartsList();
        DrawExporters();
        GUILayout.EndHorizontal();
        //
        GUILayout.BeginHorizontal();
        DrawControllers();
        DrawTemplateControl();

        GUILayout.EndHorizontal();
        // Draw the parts list


        // Draw the effects list


        DrawEffectsList();
      }
      else
      {
        GUILayout.Label("Couldn't find any effects modules on this craft, add modules via config before using this editor..");
      }

      GUI.DragWindow();
    }

    protected void DrawHeader()
    {
      GUILayout.BeginHorizontal();

      GUILayout.FlexibleSpace();
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

    private void DrawController(WaterfallController ctrl)
    {
      GUILayout.BeginHorizontal();


      ctrl.overridden = GUILayout.Toggle(ctrl.overridden, "", GUILayout.Width(60));

      GUILayout.Label(ctrl.name, GUILayout.MaxWidth(120f));
      // GUILayout.Label(ctrl.TypeId, GUILayout.MaxWidth(130f));


      float sliderMax = 1f;
      if (ctrl is MachController)
        sliderMax = 15f;

      float sliderMin = 0f;
      if (ctrl is GimbalController)
        sliderMin = -1f;

      if (ctrl is VelocityController)
        sliderMax = 3000f;

      if (ctrl.overridden)
      {
        ctrl.overrideValue = GUILayout.HorizontalSlider(ctrl.overrideValue, sliderMin, sliderMax, GUILayout.MaxWidth(100f));
      }
      else
      {
        float[] output = ctrl.Get();
        ctrl.overrideValue = GUILayout.HorizontalSlider(output[0], 0f, sliderMax, GUILayout.MaxWidth(100f));
      }

      GUILayout.Label(ctrl.overrideValue.ToString("F2"), GUILayout.MinWidth(40f));

      if (GUILayout.Button("Edit", GUILayout.Width(30)))
      {
        OpenControllerEditWindow(ctrl);
      }

      if (GUILayout.Button("x", GUILayout.Width(20)))
      {
        OpenControllerDeleteWindow(ctrl);
      }

      GUILayout.EndHorizontal();
    }

    protected void DrawControllers()
    {
      GUILayout.BeginVertical();
      GUILayout.Label("<b>CONTROLLERS</b>");
      GUILayout.BeginVertical(GUI.skin.textArea);
      // Title bar
      if (selectedModule != null)
      {
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b>Override</b>", GUILayout.Width(60));
        GUILayout.Label("  <b>Name</b>", GUILayout.Width(120));
        GUILayout.FlexibleSpace();
        GUILayout.Space(140);
        if (GUILayout.Button("Add New"))
        {
          OpenControllerAddWindow();
        }

        GUILayout.EndHorizontal();

        foreach (var ctrl in selectedModule.Controllers)
        {
          DrawController(ctrl);
        }

        GUILayout.EndVertical();
        GUILayout.EndVertical();
      }
    }

    protected void DrawTemplateControl()
    {
      GUILayout.BeginVertical();
      if (selectedModule.Templates != null && selectedModule.Templates.Count > 0)
      {
        GUILayout.Label("<b>TEMPLATES</b>");
        GUILayout.BeginVertical(GUI.skin.textArea);

        GUILayout.BeginHorizontal();

        int selectedTemplateChanges = GUILayout.SelectionGrid(selectedTemplateIndex, templatesString, 4, UIResources.GetStyle("radio_text_button"));

        if (selectedTemplateChanges != selectedTemplateIndex)
        {
          selectedTemplateIndex = selectedTemplateChanges;
          SelectTemplate(selectedModule.Templates[selectedTemplateIndex]);
        }

        if (GUILayout.Button("Copy offsets", GUILayout.Width(160f), GUILayout.Height(40)))
        {
          string copiedString = "";
          copiedString += $"position = {selectedTemplate.position.x},{selectedTemplate.position.y},{selectedTemplate.position.z}\n";
          copiedString += $"rotation = {selectedTemplate.rotation.x}, {selectedTemplate.rotation.y}, {selectedTemplate.rotation.z}\n";
          copiedString += $"scale = {selectedTemplate.scale.x}, {selectedTemplate.scale.y}, {selectedTemplate.scale.z}";

          GUIUtility.systemCopyBuffer = copiedString;
        }

        GUILayout.EndHorizontal();

        GUILayout.Label(selectedModule.Templates[selectedTemplateIndex].templateName);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Offset");
        modelOffset = UIUtils.Vector3InputField(GUILayoutUtility.GetRect(200f, 30f), modelOffset, modelOffsetString, GUI.skin.label, GUI.skin.textArea);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Rotation ");
        modelRotation = UIUtils.Vector3InputField(GUILayoutUtility.GetRect(200f, 30f), modelRotation, modelRotationString, GUI.skin.label, GUI.skin.textArea);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale");
        modelScale = UIUtils.Vector3InputField(GUILayoutUtility.GetRect(200f, 30f), modelScale, modelScaleString, GUI.skin.label, GUI.skin.textArea);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
      }

      GUILayout.EndVertical();
    }

    protected void DrawExporters()
    {
      GUILayout.BeginVertical();
      GUILayout.Label("<b>EXPORT</b>");
      GUILayout.BeginVertical(GUI.skin.textArea);
      //if (exportsOpen)
      //{
      //  if (GUILayout.Button("-", GUILayout.Width(30)))
      //  {
      //    exportsOpen = false;
      //  }
      //}
      //else
      //{
      //  if (GUILayout.Button("+", GUILayout.Width(30)))
      //  {
      //    exportsOpen = true;
      //  }
      //}
      exportsOpen = true;


      if (exportsOpen)
      {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Module and \ncopy to clipboard", GUILayout.Width(170f), GUILayout.Height(40)))
        {
          GUIUtility.systemCopyBuffer = selectedModule.ExportModule();
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();


        if (GUILayout.Button("Generate template from\n and copy to clipboard", GUILayout.Width(170f), GUILayout.Height(60)))
        {
          var node = new ConfigNode(WaterfallConstants.TemplateLibraryNodeName);
          if (templatesString != null && templatesString.Length > 0)
          {
            node.AddValue("templateName", templateName);
            foreach (var fx in selectedTemplate.allFX)
            {
              node.AddNode(fx.Save());
            }
          }
          else
          {
            node.AddValue("templateName", templateName);
            foreach (var fx in selectedModule.FX)
            {
              node.AddNode(fx.Save());
            }
          }

          GUIUtility.systemCopyBuffer = node.ToString();
        }

        GUILayout.BeginVertical();
        GUILayout.Label("Exported template name");
        templateName = GUILayout.TextArea(templateName);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
      }

      GUILayout.EndVertical();
      GUILayout.EndVertical();
    }

    protected void DrawPartsList()
    {
      GUILayout.BeginVertical();
      GUILayout.Label("<b>FX MODULES ON VESSEL</b>");

      partsScrollListPosition = GUILayout.BeginScrollView(partsScrollListPosition, GUILayout.Width(340f));


      int selectedModuleChanges = GUILayout.SelectionGrid(selectedModuleIndex, modulesString, Mathf.Min(modulesString.Length, 2), UIResources.GetStyle("radio_text_button"));

      if (selectedModuleChanges != selectedModuleIndex)
      {
        selectedModuleIndex = selectedModuleChanges;
        SelectFXModule(effectsModules[selectedModuleIndex]);
      }

      GUILayout.EndScrollView();
      GUILayout.EndVertical();
    }


    protected void DrawEffectsList()
    {
      GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
      GUILayout.BeginHorizontal();
      GUILayout.Label("<b>EFFECTS</b>");
      GUILayout.FlexibleSpace();
      if (selectedModule != null)
      {
        if (GUILayout.Button("Add"))
        {
          OpenEffectAddWindow();
        }
      }

      GUILayout.EndHorizontal();


      effectsScrollListPosition = GUILayout.BeginScrollView(effectsScrollListPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(350f));

      for (int i = 0; i < effectUIWidgets.Count; i++)
      {
        effectUIWidgets[i].Draw();
      }

      GUILayout.EndScrollView();
      GUILayout.EndVertical();
    }

    public void SelectFXModule(ModuleWaterfallFX fxMod)
    {
      selectedModule = fxMod;
      RefreshEffectList();
    }

    public void SelectTemplate(WaterfallEffectTemplate template)
    {
      selectedTemplate = template;
      effectUIWidgets.Clear();
      foreach (var fx in selectedTemplate.allFX)
      {
        effectUIWidgets.Add(new(this, fx));

        modelRotation = fx.TemplateRotationOffset;
        modelScale = fx.TemplateScaleOffset;
        modelOffset = fx.TemplatePositionOffset;
        modelOffsetString = new[] { modelOffset.x.ToString(), modelOffset.y.ToString(), modelOffset.z.ToString() };
        modelRotationString = new[] { modelRotation.x.ToString(), modelRotation.y.ToString(), modelRotation.z.ToString() };
        modelScaleString = new[] { modelScale.x.ToString(), modelScale.y.ToString(), modelScale.z.ToString() };
      }
    }

    public void RefreshEffectList()
    {
      effectUIWidgets.Clear();
      if (selectedModule.Templates.Count > 0)
      {
        selectedTemplateIndex = 0;

        templatesString = new string[selectedModule.Templates.Count];
        if (selectedModule.Templates.Count > 0)
        {
          for (int i = 0; i < selectedModule.Templates.Count; i++)
          {
            templatesString[i] = $"{i}";
          }

          SelectTemplate(selectedModule.Templates[0]);
        }
      }
      else
      {
        foreach (var fx in selectedModule.FX)
        {
          effectUIWidgets.Add(new(this, fx));

          modelRotation = fx.TemplateRotationOffset;
          modelScale = fx.TemplateScaleOffset;
          modelOffset = fx.TemplatePositionOffset;
          modelOffsetString = new[] { modelOffset.x.ToString(), modelOffset.y.ToString(), modelOffset.z.ToString() };
          modelRotationString = new[] { modelRotation.x.ToString(), modelRotation.y.ToString(), modelRotation.z.ToString() };
          modelScaleString = new[] { modelScale.x.ToString(), modelScale.y.ToString(), modelScale.z.ToString() };
        }
      }
    }

    public void GetVesselData()
    {
      vessel = FlightGlobals.ActiveVessel;
      effectsModules = new();
      if (vessel != null)
      {
        foreach (var p in vessel.Parts)
        {
          var fxModules = p.GetComponents<ModuleWaterfallFX>();
          foreach (var fxModule in fxModules)
          {
            effectsModules.Add(fxModule);
          }
        }

        modulesString = new string[effectsModules.Count];
        if (effectsModules.Count > 0)
        {
          for (int i = 0; i < effectsModules.Count; i++)
          {
            modulesString[i] = $"{effectsModules[i].moduleID} ({effectsModules[i].FX.Count} Effects)";
          }

          SelectFXModule(effectsModules[0]);
        }
      }
    }


    public void OpenModifierEditWindow(EffectModifier fxMod)
    {
      foreach (var editWin in editWindows.ToList())
      {
        editWindows.Remove(editWin);
      }

      try
      {
        var colMod = (EffectColorModifier)fxMod;
        if (colMod != null)
        {
          editWindows.Add(new UIColorModifierWindow(colMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var scaleMod = (EffectScaleModifier)fxMod;
        if (scaleMod != null)
        {
          editWindows.Add(new UIScaleModifierWindow(scaleMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var scrollMod = (EffectUVScrollModifier)fxMod;
        if (scrollMod != null)
        {
          editWindows.Add(new UIUVScrollModifierWindow(scrollMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var floatMod = (EffectFloatModifier)fxMod;
        if (floatMod != null)
        {
          editWindows.Add(new UIFloatModifierWindow(floatMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var posMod = (EffectPositionModifier)fxMod;
        if (posMod != null)
        {
          editWindows.Add(new UIPositionModifierWindow(posMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var rotMod = (EffectRotationModifier)fxMod;
        if (rotMod != null)
        {
          editWindows.Add(new UIRotationModifierWindow(rotMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var colMod = (EffectColorFromLightModifier)fxMod;
        if (colMod != null)
        {
          editWindows.Add(new UIColorFromLightModifierWindow(colMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var colMod = (EffectLightFloatModifier)fxMod;
        if (colMod != null)
        {
          editWindows.Add(new UILightFloatModifierWindow(colMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var colMod = (EffectLightColorModifier)fxMod;
        if (colMod != null)
        {
          editWindows.Add(new UILightColorModifierWindow(colMod, true));
        }
      }
      catch (InvalidCastException) { }

      try
      {
        var pMod = (EffectParticleMultiNumericModifier)fxMod;
        if (pMod != null)
        {
          editWindows.Add(new UIParticleMultiNumericModifierWindow(pMod, true));
        }
      }
      catch (InvalidCastException) { }
      try
      {
        var pMod = (EffectParticleMultiColorModifier)fxMod;
        if (pMod != null)
        {
          editWindows.Add(new UIParticleMultiColorModifierWindow(pMod, true));
        }
      }
      catch (InvalidCastException) { }


    }

    public UICurveEditWindow OpenCurveEditor(FastFloatCurve toEdit)
    {
      if (curveEditWindow != null)
      {
        curveEditWindow.ChangeCurve(toEdit);
      }
      else
      {
        curveEditWindow = new(toEdit, true);
      }

      return curveEditWindow;
    }

    public UICurveEditWindow OpenCurveEditor(FastFloatCurve toEdit, CurveUpdateFunction curveFun)
    {
      if (curveEditWindow != null)
      {
        curveEditWindow.ChangeCurve(toEdit, curveFun);
      }
      else
      {
        curveEditWindow = new(toEdit, curveFun, true);
      }

      return curveEditWindow;
    }
    public UIGradientEditWindow OpenGradientEditor(Gradient toEdit, GradientUpdateFunction gradFun, float lower = 1f, float upper = 0f)
    {
      if (gradientEditWindow != null)
      {
        gradientEditWindow.ChangeGradient(toEdit, gradFun, lower, upper);
      }
      else
      {
        gradientEditWindow = new(toEdit, gradFun, lower, upper, true);
      }

      return gradientEditWindow;
    }

    public UICurveEditWindow OpenCurveEditor(FastFloatCurve toEdit, UIModifierWindow modWin, string tag)
    {
      currentModWinForCurve = modWin;
      currentCurveTag = tag;
      if (curveEditWindow != null)
      {
        curveEditWindow.ChangeCurve(toEdit, modWin, tag);
      }
      else
      {
        curveEditWindow = new(toEdit, modWin, tag, true);
      }

      return curveEditWindow;
    }

    public UIMaterialEditWindow OpenMaterialEditWindow(WaterfallModel mdl)
    {
      if (materialEditWindow != null)
      {
        materialEditWindow.ChangeMaterial(mdl);
      }
      else
      {
        materialEditWindow = new(mdl, true);
      }

      return materialEditWindow;
    }

    public UILightEditWindow OpenLightEditWindow(WaterfallModel mdl)
    {
      if (lightEditWindow != null)
      {
        lightEditWindow.ChangeLight(mdl);
      }
      else
      {
        lightEditWindow = new(mdl, true);
      }

      return lightEditWindow;
    }
    public UIParticleEditWindow OpenParticleEditWindow(WaterfallModel mdl)
    {
      if (particleEditWindow != null)
      {
        particleEditWindow.ChangeParticle(mdl);
      }
      else
      {
        particleEditWindow = new(mdl, true);
      }

      return particleEditWindow;
    }
    public UIColorPickerWindow OpenColorEditWindow(Color c, ColorUpdateFunction fun)
    {
      if (colorPickWindow != null)
      {
        Utils.Log("[WaterfallUI] Changing Color Picker target");
        colorPickWindow.ChangeColor(c, fun, true);
      }
      else
      {
        Utils.Log("[WaterfallUI] Opening Color Picker");
        colorPickWindow = new(c, true, fun);
      }

      return colorPickWindow;
    }

    public UITexturePickerWindow OpenTextureEditWindow(string current, TextureUpdateFunction fun)
    {
      if (texturePickWindow != null)
      {
        Utils.Log("[WaterfallUI] Changing Texture Picker target");
        texturePickWindow.ChangeTexture(current, fun);
      }
      else
      {
        Utils.Log("[WaterfallUI] Opening Texture Picker");
        texturePickWindow = new(current, true, fun);
      }

      return texturePickWindow;
    }

    public string GetTextureFromPicker()
    {
      if (texturePickWindow != null)
      {
        return texturePickWindow.GetTexturePath();
      }

      return "";
    }

    public void CopyEffect(WaterfallEffect toCopy)
    {
      selectedModule.CopyEffect(toCopy, selectedTemplate);
      RefreshEffectList();
    }

    public void OpenControllerAddWindow()
    {
      if (controlAddWindow == null)
      {
        controlAddWindow = new(true);
      }

      controlAddWindow.SetAddMode(selectedModule);
    }

    public void OpenControllerDeleteWindow(WaterfallController toDelete)
    {
      if (controlAddWindow == null)
      {
        controlAddWindow = new(true);
      }

      controlAddWindow.SetDeleteMode(toDelete, selectedModule);
    }

    public void OpenControllerEditWindow(WaterfallController toEdit)
    {
      if (controlAddWindow == null)
      {
        controlAddWindow = new(true);
      }

      controlAddWindow.SetEditMode(toEdit, selectedModule);
    }

    public void OpenEffectAddWindow()
    {
      if (effectAddWindow == null)
      {
        effectAddWindow = new(true);
      }

      effectAddWindow.SetAddMode(selectedModule);
    }

    public void OpenEffectDeleteWindow(WaterfallEffect toDelete)
    {
      if (effectAddWindow == null)
      {
        effectAddWindow = new(true);
      }

      effectAddWindow.SetDeleteMode(selectedModule, toDelete);
    }

    public void OpenEffectModifierAddWindow(WaterfallEffect fx)
    {
      if (modifierPopupWindow == null)
      {
        modifierPopupWindow = new(true);
      }

      modifierPopupWindow.SetAddMode(fx);
    }

    public void OpenEffectModifierDeleteWindow(WaterfallEffect fx, EffectModifier toDelete)
    {
      if (modifierPopupWindow == null)
      {
        modifierPopupWindow = new(true);
      }

      modifierPopupWindow.SetDeleteMode(fx, toDelete);
    }


    public void UpdateCurve(FastFloatCurve curve)
    {
      // update the curve
      currentModWinForCurve.UpdateCurves(curve, currentCurveTag);
    }

    #region GUI Variables

    private string windowTitle = "";
    private Vector2 effectsScrollListPosition = Vector2.zero;
    private Vector2 partsScrollListPosition = Vector2.zero;


    public Vector3 modelRotation;
    public Vector3 modelOffset;
    public Vector3 modelScale = Vector3.one;
    private string templateName = "";
    private string[] modelOffsetString;
    private string[] modelRotationString;
    private string[] modelScaleString;


    private int selectedModuleIndex;
    private int selectedTemplateIndex;

    public WaterfallEffectTemplate selectedTemplate;

    #endregion

    #region GUI Widgets

    private UICurveEditWindow curveEditWindow;
    private UIGradientEditWindow gradientEditWindow;
    private UIModifierPopupWindow modifierPopupWindow;
    private UIAddEffectWindow effectAddWindow;
    private UIControllerPopupWindow controlAddWindow;
    private UIModifierWindow currentModWinForCurve;
    private UIMaterialEditWindow materialEditWindow;
    private UIParticleEditWindow particleEditWindow;
    private UILightEditWindow lightEditWindow;
    private UIColorPickerWindow colorPickWindow;
    private UITexturePickerWindow texturePickWindow;
    private string currentCurveTag;

    #endregion

    #region Vessel Data

    private Vessel vessel;
    private List<ModuleWaterfallFX> effectsModules = new();
    private ModuleWaterfallFX selectedModule;
    private readonly List<UIEffectWidget> effectUIWidgets = new();

    #endregion
  }
}