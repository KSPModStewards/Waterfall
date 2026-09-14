using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall.UI
{
  public class UIMaterialEditWindow : UIPopupWindow
  {
    protected string windowTitle = "";
    private WaterfallMaterial matl;
    private WaterfallModel model;
    private string[] materialList;
    private int materialID;
    private int savedID = -1;

    private List<UIMaterialData> materialControls = new();


    public UIMaterialEditWindow(WaterfallModel modelToEdit, bool show) : base(show)
    {
      materialID = 0;
      model = modelToEdit;
      Utils.Log($"[UIMaterialEditWindow]: Started editing materials on {modelToEdit}");

      materialList = new string[model.materials.Count];
      for (int i = 0; i < model.materials.Count; i++)
      {
        materialList[i] = $"{model.materials[i].transformName}"; // ({model.materials[i].materials[0].name.Split('(').First()})";
      }

      matl = modelToEdit.materials[materialID];


      InitializeShaderProperties(matl.materials[0]);
      WindowPosition = new(Screen.width / 2 - 200, Screen.height / 2f, 400, 100);
    }


    public void ChangeMaterial(WaterfallModel modelToEdit)
    {
      model = modelToEdit;
      Utils.Log($"[UIMaterialEditWindow]: Started editing materials on {modelToEdit}");
      materialID = 0;
      materialList = new string[model.materials.Count];
      for (int i = 0; i < model.materials.Count; i++)
      {
        materialList[i] = $"{model.materials[i].transformName}"; // ({model.materials[i].materials[0].name.Split('(').First()})";
      }

      matl = modelToEdit.materials[materialID];
      showWindow = true;
      GUI.BringWindowToFront(windowID);

      InitializeShaderProperties(matl.materials[0]);
    }

    protected override void InitUI()
    {
      windowTitle = "Material Editor";
      base.InitUI();
    }

    protected override void DrawWindow(int windowId)
    {
      // Draw the header/tab controls
      DrawTitle();
      DrawMaterials();
      DrawMaterialEdit();
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

    protected void DrawMaterials()
    {
      GUILayout.Label("<b>SELECT MATERIAL</b>");
      GUILayout.BeginHorizontal(GUI.skin.textArea);
      materialID = GUILayout.SelectionGrid(materialID, materialList, 1, UIResources.GetStyle("radio_text_button"));
      if (materialID != savedID)
      {
        savedID = materialID;
        matl = model.materials[savedID];
        InitializeShaderProperties(model.materials[savedID].materials[0]);
      }

      GUILayout.EndHorizontal();
    }

    protected void DrawMaterialEdit()
    {
      float headerWidth = 120f;
      GUILayout.Label("<b>TEXTURE PARAMETERS</b>");
      GUILayout.BeginVertical(GUI.skin.textArea);
      foreach (UIMaterialData control in materialControls)
      {
        if (control.PropertyType == WaterfallMaterialPropertyType.Texture)
        {
          control.Draw();
        }
      }
      GUILayout.EndVertical();
      GUILayout.Label("<b>MATERIAL PARAMETERS</b>");
      GUILayout.BeginVertical(GUI.skin.textArea);
      foreach (UIMaterialData control in materialControls)
      {
        if (!(control.PropertyType == WaterfallMaterialPropertyType.Texture))
        {
          control.Draw();
        }
      }

      GUILayout.BeginHorizontal();
      GUILayout.Space(headerWidth);
      GUILayout.Label($"Queue: {matl.materials[0].renderQueue}", GUILayout.Width(headerWidth));
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
    }

    public void UpdateColor(Color c)
    {

    }

    protected void InitializeShaderProperties(Material m)
    {
      Utils.Log($"[MaterialEditor] Generating shader property map for {m}");

      materialControls = new();

      foreach (var mProp in ShaderLoader.GetShaderPropertyMap())
      {
        if (m.HasProperty(mProp.Key))
        {
          if (mProp.Value.type == WaterfallMaterialPropertyType.Color)
          {
            UIMaterialColorData control = new(mProp.Value, model, matl);
            materialControls.Add(control);
          }

          if (mProp.Value.type == WaterfallMaterialPropertyType.Float)
          {
            UIMaterialFloatData control = new(mProp.Value, model, matl);
            materialControls.Add(control);
          }

          if (mProp.Value.type == WaterfallMaterialPropertyType.Vector4)
          {
            UIMaterialVector4Data control = new(mProp.Value, model, matl);
            materialControls.Add(control);
          }

          if (mProp.Value.type == WaterfallMaterialPropertyType.Texture)
          {
            UIMaterialTextureData control = new(mProp.Value, model, matl);
            materialControls.Add(control);
          }
        }
      }
    }
  }
}