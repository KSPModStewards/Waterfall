using UnityEngine;
using Random = System.Random;

namespace Waterfall.UI
{
  public class UIBaseWindow : MonoBehaviour
  {
    // Control Vars
    public static bool showWindow;
    public Rect windowPos = new(200f, 200f, 1000f, 400f);
    protected bool initUI;

    // Assets
    private float scrollHeight = 0f;
    private Vector2 scrollPosition = Vector2.zero;
    protected int windowID = new Random(13123).Next();

    UnityEngine.UI.Image image;

    public Rect WindowPosition
    {
      get => windowPos;
      set => windowPos = value;
    }

    protected virtual void Awake()
    {
      Utils.Log("[UI]: Awake fired");

      gameObject.layer = 5;
      gameObject.AddComponent<RectTransform>();

      var canvasRenderer = gameObject.AddComponent<CanvasRenderer>();
      canvasRenderer.cullTransparentMesh = true;
      image = gameObject.AddComponent<UnityEngine.UI.Image>();
      image.color = new Color(0, 0, 0, 0);
      image.raycastTarget = true;

      gameObject.transform.SetParent(MainCanvasUtil.MainCanvas.transform, false);

      // Ensure the RectTransform is anchored to the top-left of the screen
      var rectTransform = transform as RectTransform;
      rectTransform.anchorMin = new Vector2(0, 0);
      rectTransform.anchorMax = new Vector2(0, 0);
      rectTransform.pivot = new Vector2(0, 0);
    }

    protected virtual void Start()
    {
      Utils.Log("[UI]: Start fired");
    }

    protected virtual void OnGUI()
    {
      if (Event.current.type == EventType.Repaint || Event.current.isMouse) { }

      Draw();

      var rectTransform = transform as RectTransform;
      rectTransform.anchoredPosition = new Vector2(windowPos.x, Screen.height - windowPos.y - windowPos.height);
      rectTransform.sizeDelta = new Vector2(windowPos.width, windowPos.height);

      image.enabled = showWindow;
    }

    /// <summary>
    ///   Turn the window on or off
    /// </summary>
    public static void ToggleWindow()
    {
      Utils.Log("[UI]: Toggle Window");
      showWindow = !showWindow;
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
    ///   Draw the UI
    /// </summary>
    protected virtual void Draw()
    {
      if (!initUI)
        InitUI();

      GUI.skin = HighLogic.Skin;
      if (showWindow)
      {
        //windowPos.height = Mathf.Min(scrollHeight + 50f, 96f * 3f + 50f);
        windowPos = GUILayout.Window(windowID, windowPos, DrawWindow, new GUIContent(),
          UIResources.GetStyle("window_main"), GUILayout.ExpandHeight(true));
      }
    }

    /// <summary>
    ///   Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected virtual void DrawWindow(int windowId) { }
  }
}