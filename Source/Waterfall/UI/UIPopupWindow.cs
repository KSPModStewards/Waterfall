using UnityEngine;
using Random = System.Random;

namespace Waterfall.UI
{
  public class UIPopupWindow
  {
    public Rect windowPos = new(200f, 200f, 500f, 400f);

    // Control Vars
    protected bool showWindow;
    protected int windowID = new Random(123123).Next();
    protected bool initUI;

    // Assets
    private Vector2 scrollPosition = Vector2.zero;
    private float scrollHeight = 0f;


    public UIPopupWindow(bool show)
    {
      windowID = new Random().Next();

      Utils.Log("[UI]: Start fired");
      showWindow = show;
    }

    public Rect WindowPosition
    {
      get => windowPos;
      set => windowPos = value;
    }

    /// <summary>
    ///   Draw the UI
    /// </summary>
    public virtual void Draw()
    {
      if (!initUI)
        InitUI();

      if (showWindow)
      {
        //windowPos.height = Mathf.Min(scrollHeight + 50f, 96f * 3f + 50f);
        windowPos = GUILayout.Window(windowID, windowPos, DrawWindow, new GUIContent(), UIResources.GetStyle("window_main"), GUILayout.ExpandHeight(true));
      }
    }

    /// <summary>
    ///   Turn the window on or off
    /// </summary>
    public void ToggleWindow()
    {
      Utils.Log("[UI]: Toggle Window");
      showWindow = !showWindow;
    }

    public void SetWindowState(bool on)
    {
      showWindow = on;
    }

    /// <summary>
    ///   Initialize the UI comoponents, do localization, set up styles
    /// </summary>
    protected virtual void InitUI()
    {
      Utils.Log("[UI]: Initializing");
      initUI = true;
    }

    /// <summary>
    ///   Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected virtual void DrawWindow(int windowId) { }
  }
}