using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall.UI
{
  public class UIParticleEditWindow : UIPopupWindow
  {
    protected string windowTitle = "";
    private WaterfallParticle particle;
    private WaterfallModel model;
    private string[] particleList;
    private int particleID;
    private int savedID = -1;
    private Vector2 scrollListPosition = Vector2.zero;


    private Dictionary<string, UIParticleModule> particleModules;

    public UIParticleEditWindow(WaterfallModel modelToEdit, bool show) : base(show)
    {
      particleID = 0;
      model = modelToEdit;
      Utils.Log($"[UIParticleEditWindow]: Started editing particles on {modelToEdit}");

      particle = modelToEdit.particles[particleID];

      particleList = new string[model.particles.Count];
      for (int i = 0; i < model.particles.Count; i++)
      {
        particleList[i] = $"{model.particles[i].transformName}";
      }
      particle = modelToEdit.particles[particleID];

      InitializeParticleProperties(particle);
      WindowPosition = new(Screen.width / 2 - 200, Screen.height / 2f, 600, 900);
    }


    public void ChangeParticle(WaterfallModel modelToEdit)
    {
      model = modelToEdit;
      Utils.Log($"[UIParticleEditWindow]: Started editing particles on {modelToEdit}");
      particleID = 0;
      particleList = new string[model.particles.Count];
      for (int i = 0; i < model.particles.Count; i++)
      {
        particleList[i] = $"{model.particles[i].transformName}";
      }
      particle = modelToEdit.particles[particleID];

      showWindow = true;
      GUI.BringWindowToFront(windowID);

      InitializeParticleProperties(particle);
    }

    protected override void InitUI()
    {
      windowTitle = "Particle System Editor";
      base.InitUI();
    }

    protected override void DrawWindow(int windowId)
    {
      // Draw the header/tab controls
      DrawTitle();
      DrawParticles();
      DrawParticleEdit();
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



    protected void DrawParticles()
    {
      GUILayout.Label("<b>SELECT SYSTEM</b>");
      GUILayout.BeginVertical(GUI.skin.textArea);
      particleID = GUILayout.SelectionGrid(particleID, particleList, 1, UIResources.GetStyle("radio_text_button"));
      if (particleID != savedID)
      {
        savedID = particleID;
        particle = model.particles[savedID];
        InitializeParticleProperties(model.particles[savedID]);
      }
      GUILayout.EndVertical();
    }

    protected void DrawParticleEdit()
    {
      GUILayout.Label("<b>SELECT SYSTEM</b>");
      GUILayout.BeginVertical(GUI.skin.textArea);
      particle.worldSpaceAlternateSimulation = GUILayout.Toggle(particle.worldSpaceAlternateSimulation, "World/Floating Origin Simulation");
      GUILayout.Label("This toggles the worldspace/floating origin compensation system. Notes\n- Many particle system position functions will no longer work correctly\n- It is computationally expensive \n- It is buggy sometimes");
      GUILayout.EndVertical();
      GUILayout.Label("<b>PARTICLE PARAMETERS</b>");
      scrollListPosition = GUILayout.BeginScrollView(scrollListPosition, GUILayout.ExpandHeight(true), GUILayout.MinHeight(700f));

      foreach (var section in particleModules)
      {
        section.Value.Draw();
      }
      GUILayout.EndScrollView();
    }

    protected void InitializeParticleProperties(WaterfallParticle p)
    {
      Utils.Log($"[ParticleEditor] Generating particle property map for {p}");

      particleModules = new();

      UIFloatParticleData floatData;
      UIColorParticleData colorData;
      foreach (var pProp in WaterfallParticleLoader.GetParticlePropertyMap())
      {
        if (pProp.Value.type == WaterfallParticlePropertyType.Numeric)
        {
          floatData = new UIFloatParticleData(pProp.Value, p);

          string thisCat = pProp.Value.category;

          if (particleModules.ContainsKey(thisCat))
          {
            particleModules[thisCat].particleData.Add(floatData);
          }
          else
          {
            UIParticleModule module = new(thisCat);
            particleModules.Add(thisCat, module);
            particleModules[thisCat].particleData.Add(floatData);
          }

        }
        if (pProp.Value.type == WaterfallParticlePropertyType.Color)
        {
          colorData = new UIColorParticleData(pProp.Value, p);

          string thisCat = pProp.Value.category;
          if (particleModules.ContainsKey(thisCat))
          {
            particleModules[thisCat].particleData.Add(colorData);
          }
          else
          {
            UIParticleModule module = new(thisCat);
            particleModules.Add(thisCat, module);
            particleModules[thisCat].particleData.Add(colorData);
          }
        }
      }
      foreach (var section in particleModules)
      {
        section.Value.GetModuleStates();
      }
    }
  }
  public class UIParticleModule
  {
    public string name;
    public bool shown = true;
    public bool on = false;
    public List<UIParticleData> particleData;
    protected bool savedOn = false;

    public UIParticleModule(string categoryName)
    {
      name = categoryName;
      particleData = new();
    }
    public void GetModuleStates()
    {
      if (particleData.Count > 0)
      {
        savedOn = ParticleUtils.GetParticleModuleState(particleData[0].name, particleData[0].particle.systems[0]);
        on = savedOn;
      }
    }
    public void Draw()
    {
      GUILayout.BeginVertical(GUI.skin.textArea);
      if (shown)
      { 
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("[-]", GUILayout.ExpandHeight(false), GUILayout.Width(20f)))
        {
          shown = !shown;
        }
        
        GUILayout.Label($"<b>{name} Parameters</b>");
        GUILayout.FlexibleSpace();
        on = GUILayout.Toggle(on, "Enabled", GUILayout.Width(100f));
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        foreach (var control in particleData)
        {
          control.Draw();

        }
        GUILayout.EndVertical();
      }
      else
      {
        GUILayout.BeginHorizontal(GUI.skin.textArea);
        if (GUILayout.Button("[+]", GUILayout.ExpandHeight(false), GUILayout.Width(20f)))
        {
          shown = !shown;
        }
        GUILayout.Label($"<b>{name} Parameters</b>");
        GUILayout.FlexibleSpace();
        on = GUILayout.Toggle(on, "Enabled", GUILayout.Width(100f));
        GUILayout.EndHorizontal();
      }
      GUILayout.EndVertical();
      if (on != savedOn)
      {
        savedOn = on;
        if (particleData.Count > 0)
        {
          particleData[0].particle.SetParticleModuleState(particleData[0].name, savedOn);
        }
      }
    }
  }
}