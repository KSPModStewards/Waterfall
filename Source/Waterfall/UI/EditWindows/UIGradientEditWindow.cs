using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall.UI
{
  public delegate void GradientUpdateFunction(Gradient gradient, float lower, float upper);

  public class UIGradientEditWindow : UIPopupWindow
  {
    public enum KeyEditMode
    {
      Color,
      Alpha,
      None
    }

    public Gradient GetGradient() => gradient;

    private Gradient gradient;

    bool editingColor = false;
    protected UIModifierWindow modifier;
    protected string modifierTag;

    internal UIGradientAlphaKey selectedKey;
    internal UIGradientColorKey selectedColorKey;

    List<UIGradientAlphaKey> deleteAlphaList = new();
    List<UIGradientColorKey> deleteColorList = new();

    internal List<UIGradientAlphaKey> uiAlphaKeys;
    internal List<UIGradientColorKey> uiColorKeys;


    protected GradientUpdateFunction gradientUpdateFun;


    internal string timeText;
    internal float timeVal;
    internal float alphaVal;
    internal Color colorVal;


    Texture2D gradientTexture;

    GUIStyle boxStyle;

    Texture swatch;

    float upperBound = 1f;
    float lowerBound = 0f;
    string upperBoundText = "1";
    string lowerBoundText = "0";
    protected KeyEditMode editMode = KeyEditMode.None;
    protected Vector2 keySize = new(10f, 20f);


    public UIGradientEditWindow(Gradient gradToEdit, float lower, float upper, bool show) : base(show)
    {
      Utils.Log($"Started editing gradient {gradToEdit} with low={lower}, upper={upper}");

      lowerBound = lower;
      upperBound = upper;
      WindowPosition = new(Screen.width / 2, Screen.height / 2, 500, 200);
      gradient = gradToEdit;
      CreateUIKeysFromGradient(gradient);
      GenerateGradientTextures();
      UpdateGradient(out gradient);
    }

    public UIGradientEditWindow(Gradient gradToEdit, GradientUpdateFunction gradientFun, float lower, float upper, bool show) : base(show)
    {
      gradientUpdateFun = gradientFun;
      Utils.Log($"Started editing gradient {gradToEdit} with low={lower}, upper={upper}");


      lowerBound = lower;
      upperBound = upper;
      WindowPosition = new(Screen.width / 2, Screen.height / 2, 200, 200);
      gradient = gradToEdit;

      CreateUIKeysFromGradient(gradient);
      UpdateGradient(out gradient);
    }


    protected void CreateUIKeysFromGradient(Gradient g)
    {
      uiAlphaKeys = new List<UIGradientAlphaKey>();
      for (int i = 0; i < gradient.alphaKeys.Length; i++)
      {
        uiAlphaKeys.Add(new UIGradientAlphaKey(gradient.alphaKeys[i], keySize, this));
      }
      uiColorKeys = new List<UIGradientColorKey>();
      for (int i = 0; i < gradient.colorKeys.Length; i++)
      {
        uiColorKeys.Add(new UIGradientColorKey(gradient.colorKeys[i], keySize, this));
      }
    }
    public void ChangeGradient(Gradient gradientToEdit, GradientUpdateFunction gradFun, float lower, float upper)
    {
      gradientUpdateFun = gradFun;
      lowerBound = lower;
      upperBound = upper;
      Utils.Log($"Started editing gradient {gradientToEdit} with low={lower}, upper={upper}");
      gradient = gradientToEdit;
      CreateUIKeysFromGradient(gradient);
      UpdateGradient(out gradient);

      showWindow = true;
      GUI.BringWindowToFront(windowID);
    }

    public void UpdateGradient(out Gradient theGradient)
    {
      theGradient = new();
      //theCurve.FindMinMaxValue(out minY, out maxY);
      gradient = theGradient;
      
      gradientUpdateFun(gradient, lowerBound, upperBound);
      GenerateGradientTextures();
    }



    protected virtual void DrawHeader()
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
    ///   Initialize the UI widgets, do localization, set up styles
    /// </summary>
    protected override void InitUI()
    {
      windowTitle = "Gradient Editor";
      base.InitUI();
    }


    /// <summary>
    ///   Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected override void DrawWindow(int windowId)
    {

      boxStyle = new GUIStyle(GUI.skin.box);

      // Draw the header/tab controls
      DrawHeader();
      DrawGradientEditor();

      GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Width(400));
      GUILayout.FlexibleSpace();
      if (editMode == KeyEditMode.Alpha)
        DrawAlphaEditor();
      if (editMode == KeyEditMode.Color)
        DrawColorEditor();
      if (editMode == KeyEditMode.None)
        DrawHelp();
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      GUI.DragWindow();
    }
    protected void DrawHelp()
    {
      GUILayout.BeginHorizontal(GUI.skin.textArea);
      GUILayout.Label("Click on a key to edit it. Drag a key to move it. Right click to delete it");
      GUILayout.EndHorizontal();
    }
    protected void DrawGradientEditor()
    {
      // 
      GUILayout.Space(10f);
      GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Height(100f));
      GUILayout.BeginVertical(GUILayout.Width(100));
      GUILayout.FlexibleSpace();
      GUILayout.Label("Minimum");
      string fieldText = GUILayout.TextArea(lowerBoundText, GUILayout.MaxWidth(70f));
      if (fieldText != lowerBoundText)
      {
        if (float.TryParse(fieldText, out lowerBound))
        {
          lowerBoundText = fieldText;
          if (upperBound < lowerBound)
          {
            upperBound = lowerBound + 0.01f;
            upperBoundText = $"{upperBound:F2}";
          }
        }
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndVertical();

      GUILayout.BeginVertical();
      Rect alphaRect = GUILayoutUtility.GetRect(400, keySize.y);
      DrawAlphaKeys(alphaRect);

      Rect gradRect = GUILayoutUtility.GetRect(400, 64);

      GUI.DrawTexture(new Rect(gradRect.x + keySize.x / 2f, gradRect.y, gradRect.width - keySize.x, gradRect.height), gradientTexture, ScaleMode.StretchToFill);
      Rect colorRect = GUILayoutUtility.GetRect(400, keySize.y);
      DrawColorKeys(colorRect);
      GUILayout.EndVertical();
      GUILayout.BeginVertical(GUILayout.Width(100));
      GUILayout.FlexibleSpace();
      GUILayout.Label("Maximum");
      fieldText = GUILayout.TextArea(upperBoundText, GUILayout.MaxWidth(70f));
      if (fieldText != upperBoundText)
      {
        if (float.TryParse(fieldText, out upperBound))
        {
          upperBoundText = $"{upperBound}";
          if (upperBound < lowerBound)
          {
            lowerBound = upperBound - 0.01f;
            lowerBoundText = $"{lowerBound:F2}";
          }
        }
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndVertical();
      GUILayout.EndHorizontal();

      CleanupKeys();
    }




    protected void CleanupKeys()
    {
      for (int i = uiAlphaKeys.Count - 1; i >= 0; i--)
      {
        for (int j = deleteAlphaList.Count - 1; j >= 0; j--)
        {
          if (uiAlphaKeys[i] == deleteAlphaList[j])
          {
            uiAlphaKeys.RemoveAt(i);
            deleteAlphaList.RemoveAt(j);
          }
        }
      }
      for (int i = uiColorKeys.Count - 1; i >= 0; i--)
      {
        for (int j = deleteColorList.Count - 1; j >= 0; j--)
        {
          if (uiColorKeys[i] == deleteColorList[j])
          {
            uiColorKeys.RemoveAt(i);
            deleteColorList.RemoveAt(j);
          }
        }
      }
      RegenerateGradient();
    }
    protected void DrawAlphaKeys(Rect rect)
    {
      GUI.BeginGroup(rect);

      foreach (UIGradientAlphaKey key in uiAlphaKeys)
      {
        bool allowInteraction = false;

        if (keyDrag && key == selectedKey)
          allowInteraction = true;

        if (!keyDrag)
          allowInteraction = true;

        key.Draw(new Rect(keySize.x / 2f, 0f, rect.width - keySize.x, rect.height), allowInteraction);

      }
      GUI.color = new Color(1, 1, 1, 0.2f);
      if (GUI.Button(new Rect(0f, 0f, rect.width, rect.height), "", boxStyle))
      {
        if (!keyDrag)
          AddAlphaKey(Event.current.mousePosition.x / rect.width);
      }
      GUI.color = new Color(1, 1, 1, 1f);
      GUI.EndGroup();
    }
    protected void AddAlphaKey(float selTime)
    {
      if (gradient.alphaKeys.Length <= 10)
      {
        uiAlphaKeys.Add(new UIGradientAlphaKey(selTime, gradient.Evaluate(selTime).a, keySize, this));
        RegenerateGradient();
      }
    }
    public void DeleteAlphaKey(UIGradientAlphaKey selKey)
    {
      deleteAlphaList.Add(selKey);
      if (selectedKey == selKey)
      {
        editMode = KeyEditMode.None;
        selectedKey = null;
      }
    }

    public void SelectAlphaKey(UIGradientAlphaKey selKey)
    {

      editMode = KeyEditMode.Alpha;
      selectedKey = selKey;
      timeVal = selectedKey.time;
      alphaVal = selectedKey.alpha;
      timeText = $"{timeVal.ToString("F2")}";

      foreach (UIGradientColorKey key in uiColorKeys)
      {
        key.selected = false;
      }
      foreach (UIGradientAlphaKey key in uiAlphaKeys)
      {
        if (key != selKey) key.selected = false;
      }
    }
    public void StopAlphaKeyDrag()
    {
      keyDrag = false;
      SelectAlphaKey(selectedKey);
      RegenerateGradient();
    }
    bool keyDrag = false;
    public void StartKeyDrag()
    {
      keyDrag = true;
    }

    public void DoKeyDrag()
    {
      RegenerateGradient();
    }
    protected void DrawAlphaEditor()
    {
      bool inputChanged = false;
      GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.MaxHeight(80));
      GUILayout.Label("Alpha");

      alphaVal = GUILayout.HorizontalSlider(alphaVal, 0, 1.0f, GUILayout.MaxWidth(150f));
      if (alphaVal != selectedKey.alpha)
        inputChanged = true;

      GUILayout.Label("Time");
      string fieldText = GUILayout.TextArea(timeText, GUILayout.MaxWidth(150f));
      if (fieldText != timeText)
      {
        if (float.TryParse(fieldText, out timeVal))
        {
          timeVal = Mathf.Clamp(timeVal, 0f, 1f);
          timeText = $"{timeVal}";
          inputChanged = true;
        }
      }


      GUILayout.EndHorizontal();


      if (inputChanged)
      {
        selectedKey.ChangedColor();
        selectedKey.alpha = alphaVal;
        selectedKey.time = timeVal;
        RegenerateGradient();

      }
    }
    protected void DrawColorKeys(Rect rect)
    {
      GUI.BeginGroup(rect);

      foreach (UIGradientColorKey key in uiColorKeys)
      {
        bool allowInteraction = false;
        if (keyDrag && key == selectedColorKey)
          allowInteraction = true;
        if (!keyDrag)
          allowInteraction = true;
        key.Draw(new Rect(keySize.x / 2f, 0f, rect.width - keySize.x, rect.height), allowInteraction);

      }
      GUI.color = new Color(1, 1, 1, 0.2f);
      if (GUI.Button(new Rect(0f, 0f, rect.width, rect.height), "", boxStyle))
      {
        if (!keyDrag && !Input.GetMouseButton(1) && !Input.GetMouseButtonUp(1) && !Input.GetMouseButtonDown(1))
        {
          AddColorKey(Event.current.mousePosition.x / rect.width);
        }
      }
      GUI.color = new Color(1, 1, 1, 1);
      GUI.EndGroup();
    }

    protected void AddColorKey(float selTime)
    {
      UIGradientColorKey newKey = new UIGradientColorKey(selTime,
        new Color(
          gradient.Evaluate(selTime).r,
          gradient.Evaluate(selTime).g,
          gradient.Evaluate(selTime).b), keySize, this);
      uiColorKeys.Add(newKey);

      SelectColorKey(newKey);
      RegenerateGradient();
    }
    public void DeleteColorKey(UIGradientColorKey selKey)
    {
      deleteColorList.Add(selKey);
      if (selectedColorKey == selKey)
      {
        editingColor = false;
        editMode = KeyEditMode.None;
        selectedColorKey = null;
      }
    }

    public void SelectColorKey(UIGradientColorKey selKey)
    {
      editMode = KeyEditMode.Color;
      selectedColorKey = selKey;
      timeVal = selectedColorKey.time;
      colorVal = selectedColorKey.color;
      timeText = $"{timeVal.ToString("F2")}";
      swatch = TextureUtils.GenerateColorTexture(64, 32, colorVal);
      foreach (UIGradientAlphaKey key in uiAlphaKeys)
      {
        key.selected = false;
      }
      foreach (UIGradientColorKey key in uiColorKeys)
      {
        if (key != selKey) key.selected = false;
      }

      if (editingColor)
      {
        WaterfallUI.Instance.OpenColorEditWindow(selKey.color, UpdateColor);
      }

    }
    public void StopColorKeyDrag()
    {
      keyDrag = false;
      SelectColorKey(selectedColorKey);
      RegenerateGradient();
    }

    protected void RegenerateGradient()
    {
      GradientColorKey[] colorKeys = new GradientColorKey[uiColorKeys.Count];
      GradientAlphaKey[] alphaKeys = new GradientAlphaKey[uiAlphaKeys.Count];

      for (int i = 0; i < uiColorKeys.Count; i++)
      {
        colorKeys[i] = new GradientColorKey(uiColorKeys[i].color, uiColorKeys[i].time);

      }
      for (int i = 0; i < uiAlphaKeys.Count; i++)
      {
        alphaKeys[i] = new GradientAlphaKey(uiAlphaKeys[i].alpha, uiAlphaKeys[i].time);
      }
      gradient.SetKeys(colorKeys, alphaKeys);
      gradientUpdateFun(gradient, lowerBound, upperBound);
      GenerateGradientTextures();
    }
    protected void DrawColorEditor()
    {

      GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.MaxHeight(80));
      GUILayout.Label("Color");

      Rect sRect = GUILayoutUtility.GetRect(150, 30);
      if (GUI.Button(sRect, ""))
      {
        editingColor = true;
        WaterfallUI.Instance.OpenColorEditWindow(selectedColorKey.color, UpdateColor);
      }
      GUI.DrawTexture(sRect, swatch);

      bool timeChanged = false;
      GUILayout.Label("Time");
      string fieldText = GUILayout.TextArea(timeText, GUILayout.MaxWidth(150f));
      if (fieldText != timeText)
      {
        if (float.TryParse(fieldText, out timeVal))
        {
          timeVal = Mathf.Clamp(timeVal, 0f, 1f);
          timeText = $"{timeVal}";
          timeChanged = true;
        }
      }
      GUILayout.EndHorizontal();
      if (timeChanged)
      {
        selectedColorKey.time = timeVal;
        swatch = TextureUtils.GenerateColorTexture(64, 32, colorVal);
        RegenerateGradient();

      }
    }

    public void UpdateColor(Color col)
    {
      Utils.Log("[GradientEditWindow] Updated Color");
      colorVal = col;
      selectedColorKey.color = colorVal;
      selectedColorKey.ChangedColor();
      swatch = TextureUtils.GenerateColorTexture(64, 32, colorVal);
    }
    public void GenerateGradientTextures()
    {
      gradientTexture = TextureUtils.GenerateGradientTexture(256, 32, gradient);
    }

    #region GUI Variables

    protected string windowTitle = "";

    #endregion


    public class UIGradientAlphaKey
    {
      public Vector2 keySize = Vector2.one;
      public bool selected = false;
      public bool dragStart = false;
      public float alpha = 0f;
      public float time = 0f;
      public Vector2 mPosLast;
      public Texture2D keyCarat;

      protected UIGradientEditWindow editor;
      protected GUIStyle activeStyle;

      public UIGradientAlphaKey(GradientAlphaKey k, Vector2 uiSize, UIGradientEditWindow parent)
      {
        alpha = k.alpha;
        time = k.time;
        keySize = uiSize;
        editor = parent;
        keyCarat = TextureUtils.GenerateFlatColorTexture(16, 16, Color.white * alpha);
      }
      public UIGradientAlphaKey(float t, float a, Vector2 uiSize, UIGradientEditWindow parent)
      {
        alpha = a;
        time = t;
        keySize = uiSize;
        editor = parent;
        keyCarat = TextureUtils.GenerateFlatColorTexture(16, 16, Color.white * alpha);
      }

      public void ChangedColor()
      {
        keyCarat = TextureUtils.GenerateFlatColorTexture(16, 16, Color.white * alpha);
      }

      public void Draw(Rect rect, bool allowAction)
      {
        if (activeStyle == null)
        {
          activeStyle = new GUIStyle(new GUIStyle(GUI.skin.button)
          {
            normal = GUI.skin.button.active
          });
        }
        float xPos = (rect.width * time) - keySize.x / 2f;
        Rect buttonRect = new(rect.x + xPos, 0f, keySize.x, rect.height);

        GUI.Button(buttonRect, "", selected ? activeStyle : GUI.skin.button);
        GUI.DrawTexture(new(buttonRect.x + 2, buttonRect.y + 2, keySize.x - 4, rect.height - 2), keyCarat);
        if (allowAction)
        {
          if (Input.GetMouseButton(0) && buttonRect.Contains(Event.current.mousePosition))
          {
            dragStart = true;
            selected = true;

            editor.SelectAlphaKey(this);
            if (!dragStart)
              editor.StartKeyDrag();
          }
          if (Input.GetMouseButton(1) && buttonRect.Contains(Event.current.mousePosition))
          {
            editor.DeleteAlphaKey(this);
          }
          if (Input.GetMouseButton(0) && dragStart)
          {
            if (Event.current.type == EventType.Repaint)
            {
              time = Mathf.Clamp01((Event.current.mousePosition.x - keySize.x / 2f) / rect.width);
            }
            editor.DoKeyDrag();
          }
          if (!Input.GetMouseButton(0) && dragStart)
          {

            dragStart = false;

            editor.StopAlphaKeyDrag();

          }
        }

      }
    }

    public class UIGradientColorKey
    {
      public Vector2 keySize = Vector2.one;
      public bool selected = false;
      public bool dragStart = false;
      public Color color = Color.white;
      public float time = 0f;
      public Vector2 mPosLast;
      public Texture2D keyCarat;

      protected UIGradientEditWindow editor;
      protected GUIStyle activeStyle;

      public UIGradientColorKey(GradientColorKey k, Vector2 uiSize, UIGradientEditWindow parent)
      {
        color = k.color;
        time = k.time;
        keySize = uiSize;
        editor = parent;
        keyCarat = TextureUtils.GenerateFlatColorTexture(16, 16, color);
      }
      public UIGradientColorKey(float t, Color c, Vector2 uiSize, UIGradientEditWindow parent)
      {
        color = c;
        time = t;
        keySize = uiSize;
        editor = parent;
        keyCarat = TextureUtils.GenerateFlatColorTexture(16, 16, color);

      }

      public void ChangedColor()
      {
        keyCarat = TextureUtils.GenerateFlatColorTexture(16, 16, color);
      }
      public void Draw(Rect rect, bool allowAction)
      {
        if (activeStyle == null)
        {
          activeStyle = new GUIStyle(new GUIStyle(GUI.skin.button)
          {
            normal = GUI.skin.button.active
          });
        }
        float xPos = (rect.width * time) - keySize.x / 2f;
        Rect buttonRect = new Rect(rect.x + xPos, 0f, keySize.x, rect.height);

        GUI.Button(buttonRect, "", selected ? activeStyle : GUI.skin.button);
        GUI.DrawTexture(new Rect(buttonRect.x + 2, buttonRect.y + 2, keySize.x - 4, rect.height - 2), keyCarat);
        if (allowAction)
        {

          if (Input.GetMouseButton(1) && buttonRect.Contains(Event.current.mousePosition))
          {
            editor.DeleteColorKey(this);
          }
          else
          {

            if (Input.GetMouseButton(0) && buttonRect.Contains(Event.current.mousePosition))
            {
              dragStart = true;
              selected = true;

              editor.SelectColorKey(this);
              if (!dragStart)
                editor.StartKeyDrag();
            }
            if (Input.GetMouseButton(0) && dragStart)
            {
              if (Event.current.type == EventType.Repaint)
              {
                time = Mathf.Clamp01((Event.current.mousePosition.x - keySize.x / 2f) / rect.width);
              }
              editor.DoKeyDrag();
            }
            if (!Input.GetMouseButton(0) && dragStart)
            {

              dragStart = false;
              editor.StopColorKeyDrag();
            }
          }
        }
      }
    }
  }
}