using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall.UI
{
  public delegate void CurveUpdateFunction(FastFloatCurve curve);

  public class UICurveEditWindow : UIPopupWindow
  {
    private const int                GraphLabels = 4;
    private const float              labelSpace  = 20f * (GraphLabels + 1) / GraphLabels;
    private       float              minY        = -5f;
    private       float              maxY        = 5f;
    private       string             textVersion;
    private       List<FloatString4> points = new();


    public UICurveEditWindow(FastFloatCurve curveToEdit, bool show) : base(show)
    {
      Utils.Log($"[UICurveEditWindow] Started editing curve {curveToEdit}");


      if (!showWindow)
        WindowPosition = new(Screen.width / 2, Screen.height / 2, 678, 600);
      curve = curveToEdit;

      points = GraphUtils.FastFloatCurveToPoints(curveToEdit);
      UpdateCurve(out curve);
    }

    public UICurveEditWindow(FastFloatCurve curveToEdit, CurveUpdateFunction curveFun, bool show) : base(show)
    {
      curveUpdateFun = curveFun;
      Utils.Log($"[UICurveEditWindow]: Started editing curve {curveToEdit}");

      if (!showWindow)
        WindowPosition = new(Screen.width / 2, Screen.height / 2, 678, 600);
      curve = curveToEdit;

      points = GraphUtils.FastFloatCurveToPoints(curveToEdit);
      UpdateCurve(out curve);
    }

    public UICurveEditWindow(FastFloatCurve curveToEdit, UIModifierWindow modWin, string tag, bool show) : base(show)
    {
      modifier    = modWin;
      modifierTag = tag;
      Utils.Log($"[UICurveEditWindow]: Started editing curve {curveToEdit}");

      if (!showWindow)
        WindowPosition = new(Screen.width / 2, Screen.height / 2, 678, 600);
      curve = curveToEdit;

      points = GraphUtils.FastFloatCurveToPoints(curveToEdit);
      UpdateCurve(out curve);
    }

    public void ChangeCurve(FastFloatCurve curveToEdit)
    {
      Utils.Log($"[UICurveEditWindow]: Started editing curve {curveToEdit}");
      curve  = curveToEdit;
      points = GraphUtils.FastFloatCurveToPoints(curveToEdit);
      UpdateCurve(out curve);
      showWindow = true;
      GUI.BringWindowToFront(windowID);
    }

    public void ChangeCurve(FastFloatCurve curveToEdit, CurveUpdateFunction curveFun)
    {
      curveUpdateFun = curveFun;
      Utils.Log($"[UICurveEditWindow]: Started editing curve {curveToEdit}");
      curve  = curveToEdit;
      points = GraphUtils.FastFloatCurveToPoints(curveToEdit);
      UpdateCurve(out curve);
      showWindow = true;
      GUI.BringWindowToFront(windowID);
    }

    public void ChangeCurve(FastFloatCurve curveToEdit, UIModifierWindow modWin, string tag)
    {
      modifier    = modWin;
      modifierTag = tag;
      Utils.Log($"[UICurveEditWindow]: Started editing curve {curveToEdit}");
      curve  = curveToEdit;
      points = GraphUtils.FastFloatCurveToPoints(curveToEdit);
      UpdateCurve(out curve);
      showWindow = true;
      GUI.BringWindowToFront(windowID);
    }

    public void UpdateCurve(out FastFloatCurve theCurve)
    {
      theCurve = new();
      foreach (var v in points)
      {
        theCurve.Add(v.floats.x, v.floats.y, v.floats.z, v.floats.w);
      }

      theCurve.FindMinMax(out float minTime, out minY, out float maxTime, out maxY);
      curve = theCurve;
      curveUpdateFun(curve);
      textVersion  = GraphUtils.PointsToString(points);
      graphTexture = GraphUtils.GenerateCurveTexture(texWidth, texHeight, curve, Color.green);
    }

    public bool Compare(FastFloatCurve toCompare)
    {
      if (GraphUtils.FastFloatCurveToPoints(toCompare) != points)
      {
        return false;
      }

      return true;
    }

    public FastFloatCurve GetCurve() => curve;

    protected virtual void DrawHeader()
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


    /// <summary>
    ///   Initialize the UI widgets, do localization, set up styles
    /// </summary>
    protected override void InitUI()
    {
      windowTitle = "Curve Editor";
      base.InitUI();
    }


    /// <summary>
    ///   Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected override void DrawWindow(int windowId)
    {
      // Draw the header/tab controls
      DrawHeader();
      DrawCurveEditor();
      GUI.DragWindow();
    }


    protected void DrawCurveEditor()
    {
      GUILayout.BeginHorizontal(GUILayout.Height(texHeight));

      var sizeMax = GUI.skin.label.CalcSize(new(maxY.ToString("F3")));
      var sizeMin = GUI.skin.label.CalcSize(new(minY.ToString("F3")));

      GUILayout.BeginVertical(GUILayout.MinWidth(Mathf.Max(sizeMin.x, sizeMax.x)));
      const int   GraphLabels = 4;
      const float labelSpace  = 20f * (GraphLabels + 1) / GraphLabels;
      for (int i = 0; i <= GraphLabels; i++)
      {
        GUILayout.Label((maxY - (maxY - minY) * i / GraphLabels).ToString("F3"), new GUIStyle(GUI.skin.label) { wordWrap = false });
        if (i != GraphLabels) //only do it if it's not the last one
          GUILayout.Space(texHeight / GraphLabels - labelSpace);
      }

      GUILayout.EndVertical();

      GUILayout.Box(graphTexture);

      GUILayout.EndHorizontal();

      FloatString4 excludePoint = null;

      scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.Height(200));
      GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
      GUILayout.BeginVertical();
      GUILayout.Label("X");

      foreach (var p in points)
      {
        string ns = GUILayout.TextField(p.strings[0]);
        if (ns != p.strings[0])
        {
          p.strings[0] = ns;
          p.UpdateFloats();
          UpdateCurve(out curve);
        }
      }

      GUILayout.EndVertical();
      GUILayout.BeginVertical();
      GUILayout.Label("Y");
      foreach (var p in points)
      {
        string ns = GUILayout.TextField(p.strings[1]);
        if (ns != p.strings[1])
        {
          p.strings[1] = ns;
          p.UpdateFloats();
          UpdateCurve(out curve);
        }
      }

      GUILayout.EndVertical();
      GUILayout.BeginVertical();
      GUILayout.Label("In Tangent");
      foreach (var p in points)
      {
        string ns = GUILayout.TextField(p.strings[2]);
        if (ns != p.strings[2])
        {
          p.strings[2] = ns;
          p.UpdateFloats();
          UpdateCurve(out curve);
        }
      }

      GUILayout.EndVertical();
      GUILayout.BeginVertical();
      GUILayout.Label("Out Tangent");
      foreach (var p in points)
      {
        string ns = GUILayout.TextField(p.strings[3]);
        if (ns != p.strings[3])
        {
          p.strings[3] = ns;
          p.UpdateFloats();
          UpdateCurve(out curve);
        }
      }

      GUILayout.EndVertical();
      GUILayout.BeginVertical();
      GUILayout.Label("Remove");
      foreach (var p in points)
      {
        if (GUILayout.Button("X"))
        {
          excludePoint = p;
        }
      }

      GUILayout.EndVertical();
      GUILayout.EndHorizontal();
      GUILayout.EndScrollView();

      if (excludePoint != null)
      {
        points.Remove(excludePoint);
        UpdateCurve(out curve);
      }

      GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
      // sort = GUILayout.Toggle(sort, "Sort");
      if (GUILayout.Button("New Curve"))
      {
        points.Clear();
        textVersion  = "";
        graphTexture = GraphUtils.GenerateCurveTexture(texWidth, texHeight, curve, Color.green);
      }

      if (GUILayout.Button("Smooth Tangents"))
      {
        //SmoothTangents();
      }

      if (GUILayout.Button("Copy out"))
      {
        GUIUtility.systemCopyBuffer = textVersion;
      }

      if (GUILayout.Button("Paste in"))
      {
        textVersion = GUIUtility.systemCopyBuffer;
        points      = GraphUtils.StringToPoints(textVersion);
        UpdateCurve(out curve);
      }

      if (GUILayout.Button("Add Node"))
      {
        if (points.Count > 0)
        {
          points.Add(new(points.Last().floats.x + 1, points.Last().floats.y, points.Last().floats.z, points.Last().floats.w));
        }
        else
        {
          points.Add(new(0, 0, 0, 0));
        }

        UpdateCurve(out curve);
      }

      GUILayout.EndHorizontal();
    }

    #region GUI Variables

    protected        string    windowTitle = "";
    private readonly int       texWidth    = 512;
    private readonly int       texHeight   = 128;
    private          Texture2D graphTexture;
    private          Vector2   scrollPos;

    #endregion

    #region GUI Widgets

    #endregion

    #region Data

    private   FastFloatCurve          curve;
    protected UIModifierWindow    modifier;
    protected string              modifierTag;
    protected CurveUpdateFunction curveUpdateFun;

    #endregion
  }
}