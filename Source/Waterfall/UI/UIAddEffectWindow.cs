using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall.UI
{
  public class UIAddEffectWindow : UIPopupWindow
  {
    protected string            windowTitle = "";
    private   ModifierPopupMode windowMode;
    private   WaterfallEffect   effect;
    private   ModuleWaterfallFX module;

    private string parentName    = "";
    private string newEffectName = "newEffect";
    private string parentErrorString = "";

    private int      workflowFlag;
    private string[] workflowOptions;

    private int      shaderFlag;
    private string[] shaderOptions;

    private int                  modelFlag;
    private string[]             modelOptions;

    private int particleFlag;
    private string[] particleOptions;

    private List<WaterfallAsset> models;
    private List<WaterfallAsset> shaders;
    private List<WaterfallAsset> particles;

    private bool randomizeSeed = true;

    public UIAddEffectWindow(bool show) : base(show)
    {
      WindowPosition = new(Screen.width / 2, Screen.height / 2f, 750, 400);
    }

    public void SetDeleteMode(ModuleWaterfallFX fxModule, WaterfallEffect fx)
    {
      showWindow     = true;
      module         = fxModule;
      effect         = fx;
      windowMode     = ModifierPopupMode.Delete;
      WindowPosition = new(Screen.width / 2 - 100, Screen.height / 2f - 50, 200, 100);
    }

    public void SetAddMode(ModuleWaterfallFX fxModule)
    {
      showWindow          = true;
      module              = fxModule;
      windowMode          = ModifierPopupMode.Add;
      parentErrorString   = "";

      workflowOptions = Enum.GetNames(typeof(AssetWorkflow)).ToArray();
      workflowFlag    = 0;
      var modelOpts = new List<string>();
      models = WaterfallAssets.GetModels((AssetWorkflow)Enum.Parse(typeof(AssetWorkflow), workflowOptions[workflowFlag]));
      foreach (var w in models)
      {
        modelOpts.Add($"<b>{w.Name}</b>\n{w.Description}\n{w.Path}");
      }

      modelOptions = modelOpts.ToArray();

      var shaderOpts = new List<string>();
      shaders = WaterfallAssets.GetShaders((AssetWorkflow)Enum.Parse(typeof(AssetWorkflow), workflowOptions[workflowFlag]));
      foreach (var w in shaders)
      {
        shaderOpts.Add($"<b>{w.Name}</b>\n{w.Description}");
      }

      shaderOptions  = shaderOpts.ToArray();

      var particleOpts = new List<string>();
      particles = WaterfallAssets.GetParticles((AssetWorkflow)Enum.Parse(typeof(AssetWorkflow), workflowOptions[workflowFlag]));
      foreach (var w in particles)
      {
        particleOpts.Add($"<b>{w.Name}</b>\n{w.Description}");
      }
      particleOptions = particleOpts.ToArray();

      WindowPosition = new(Screen.width / 2, Screen.height / 2f, 750, 400);
    }

    protected override void InitUI()
    {
      if (windowMode == ModifierPopupMode.Add)
        windowTitle = "Add new Effect";
      if (windowMode == ModifierPopupMode.Delete)
        windowTitle = "Confirm Delete";
      base.InitUI();
    }


    protected override void DrawWindow(int windowId)
    {
      // Draw the header/tab controls
      DrawTitle();
      if (windowMode == ModifierPopupMode.Add)
        DrawAdd();
      if (windowMode == ModifierPopupMode.Delete)
        DrawDelete();
      GUI.DragWindow();
    }

    protected void DrawTitle()
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

    protected void DrawDelete()
    {
      GUILayout.Label($"Are you sure you want to delete {effect.name}?");
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Yes"))
      {
        module.RemoveEffect(effect);
        WaterfallUI.Instance.RefreshEffectList();
        showWindow = false;
      }

      if (GUILayout.Button("No"))
      {
        showWindow = false;
      }

      GUILayout.EndHorizontal();
    }

    protected void DrawAdd()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Effect Name", GUILayout.Width(160f));
      newEffectName = GUILayout.TextArea(newEffectName, GUILayout.Width(200f));
      GUILayout.Space(250f);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Effect Parent [required]", GUILayout.Width(160f));
      parentName = GUILayout.TextArea(parentName, GUILayout.Width(200f));
      if (parentErrorString != "")
      {
        GUILayout.Label($"<color=#FF0000>{parentErrorString}</color>", GUILayout.Width(250f));
      }
      else
      {
        GUILayout.Space(250f);
      }
      GUILayout.EndHorizontal();
      


      GUILayout.Label("<b>SELECT WORKFLOW</b>", GUILayout.Width(120f));
      int newFlag = GUILayout.SelectionGrid(workflowFlag,
                                            workflowOptions,
                                            Mathf.Min(workflowOptions.Length, 4),
                                            UIResources.GetStyle("radio_text_button"));


      if (newFlag != workflowFlag)
      {
        workflowFlag = newFlag;

        var modelOpts = new List<string>();
        models = WaterfallAssets.GetModels((AssetWorkflow)Enum.Parse(typeof(AssetWorkflow), workflowOptions[workflowFlag]));
        foreach (var w in models)
        {
          modelOpts.Add($"<b>{w.Name}</b>\n{w.Description}\n{w.Path}");
        }

        modelOptions = modelOpts.ToArray();

        var shaderOpts = new List<string>();
        shaders = WaterfallAssets.GetShaders((AssetWorkflow)Enum.Parse(typeof(AssetWorkflow), workflowOptions[workflowFlag]));
        foreach (var w in shaders)
        {
          shaderOpts.Add($"<b>{w.Name}</b>\n{w.Description}");
        }

        shaderOptions = shaderOpts.ToArray();
        var particleOpts = new List<string>();
        particles = WaterfallAssets.GetParticles((AssetWorkflow)Enum.Parse(typeof(AssetWorkflow), workflowOptions[workflowFlag]));
        foreach (var w in particles)
        {
          particleOpts.Add($"<b>{w.Name}</b>\n{w.Description}");
        }
        particleOptions = particleOpts.ToArray();

        modelFlag  = 0;
        shaderFlag = 0;
        particleFlag = 0;
      }

      GUILayout.BeginVertical(GUI.skin.textArea);

      GUILayout.BeginHorizontal();
      GUILayout.Label("<b>Model</b>", GUILayout.Width(120f));
      modelFlag = GUILayout.SelectionGrid(modelFlag,
                                          modelOptions,
                                          Mathf.Min(modelOptions.Length, 2),
                                          UIResources.GetStyle("radio_text_button"));
      GUILayout.EndHorizontal();
      if (shaderOptions.Length > 0)
      {
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b>Shader</b>", GUILayout.Width(120f));
        shaderFlag = GUILayout.SelectionGrid(shaderFlag,
                                             shaderOptions,
                                             Mathf.Min(shaderOptions.Length, 2),
                                             UIResources.GetStyle("radio_text_button"));
        GUILayout.EndHorizontal();
      }
      if (particleOptions.Length > 0)
      {
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b>Particle</b>", GUILayout.Width(120f));
        particleFlag = GUILayout.SelectionGrid(particleFlag,
                                             particleOptions,
                                             Mathf.Min(particleOptions.Length, 2),
                                             UIResources.GetStyle("radio_text_button"));
        GUILayout.EndHorizontal();
      }
      GUILayout.EndVertical();
      GUILayout.BeginHorizontal();

      randomizeSeed = GUILayout.Toggle(randomizeSeed, "Randomize Effect Seed");

      GUILayout.EndHorizontal();

      if (GUILayout.Button("Add"))
      {
        var modelXforms = module.GetComponentsInChildren<Transform>();
        if (parentName =="")
        {
          parentErrorString = "Please specify a valid Transform name";
        }
        else if (modelXforms.ToList().FindAll(x => x.name == parentName).Any())
        {
          module.AddEffect(CreateNewEffect());
          WaterfallUI.Instance.RefreshEffectList();
          showWindow = false;
        }
        else
        {
          parentErrorString = $"{parentName} is not a valid Transform on this part";
        }

      }

      if (GUILayout.Button("Cancel"))
      {
        showWindow = false;
      }
    }

    private WaterfallEffect CreateNewEffect()
    {
      WaterfallModel model;      

      if (shaders.Count == 0 && particles.Count == 0)
      {
        model = new(models[modelFlag],
                    null,
                    null,
                    randomizeSeed);
      }
      else if (particles.Count > 0)
      {
        model = new(models[modelFlag],
                    null,
                    particles[particleFlag],
                    randomizeSeed);
      }
      else 
      {
        model = new(models[modelFlag],
                    shaders[shaderFlag],
                    null,
                    randomizeSeed);
      }

      WaterfallEffect newFX;
      if (WaterfallUI.Instance.selectedTemplate == null)
      {
        Utils.Log("[UIAddEffectWindow]: Creating effect");
        newFX = new(parentName, model);
      }
      else
      {
        Utils.Log($"[UIAddEffectWindow]: Creating effect as part of template {WaterfallUI.Instance.selectedTemplate.templateName}");
        newFX = new(parentName, model, WaterfallUI.Instance.selectedTemplate);
      }

      newFX.name = newEffectName;

      return newFX;
    }
  }
}