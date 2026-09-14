using KSP.UI.Screens;
using UnityEngine;

namespace Waterfall.UI
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public class UIAppToolbarWindow : MonoBehaviour
  {
    // Stock toolbar button
    protected ApplicationLauncherButton stockToolbarButton;
    private Texture2D activeTexture;
    private Texture2D inactiveTexture;

    private WaterfallUI waterfallUI;

    void Awake()
    {
      if (Settings.ShowEffectEditor)
      {
        GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
        GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

        activeTexture = GameDatabase.Instance.GetTexture(UIConstants.UIApplicationButton_On, false);
        inactiveTexture = GameDatabase.Instance.GetTexture(UIConstants.UIApplicationButton_Off, false);

        waterfallUI = new GameObject(nameof(WaterfallUI)).AddComponent<WaterfallUI>();
      }
      else
      {
        GameObject.Destroy(gameObject);
      }
    }

    // Stock toolbar handling methods
    void OnDestroy()
    {
      Utils.Log("[UI]: OnDestroy Fired");
      // Remove the stock toolbar button
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
      GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
      }

      if (waterfallUI != null)
      {
        GameObject.Destroy(waterfallUI.gameObject.gameObject);
      }
    }
    protected void OnToolbarButtonToggle()
    {
      Utils.Log("[UI]: Toolbar Button Toggled");
      UIBaseWindow.ToggleWindow();
      stockToolbarButton.SetTexture(UIBaseWindow.showWindow ? activeTexture : inactiveTexture);
    }


    protected void OnGUIAppLauncherReady()
    {
      Utils.Log("[UI]: App Launcher Ready");

      if (ApplicationLauncher.Ready && stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(OnToolbarButtonToggle,
                                                                            OnToolbarButtonToggle,
                                                                            DummyVoid,
                                                                            DummyVoid,
                                                                            DummyVoid,
                                                                            DummyVoid,
                                                                            ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
                                                                            inactiveTexture);
      }
    }

    protected void OnGUIAppLauncherDestroyed()
    {
      Utils.Log("[UI]: App Launcher Destroyed");
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
        stockToolbarButton = null;
      }
    }

    protected void onAppLaunchToggleOff()
    {
      Utils.Log("[UI]: App Launcher Toggle Off");
      stockToolbarButton.SetTexture(inactiveTexture);
    }

    protected void DummyVoid() { }
  }
}