using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using Waterfall.EffectControllers;

namespace Waterfall
{
  public enum Version
  {
    Initial,
    FixedRampRates,
  }

  public class ModuleWaterfallFX : PartModule
  {
    public static readonly Version CurrentVersion = Enum.GetValues(typeof(Version)).Cast<Version>().Max();

    // This identifies this FX module for reference elsewhere
    [KSPField(isPersistant = false)] public string moduleID = "";

    // This links to an EngineID from a ModuleEnginesFX
    [KSPField(isPersistant = false)] public string engineID = "";

    [KSPField(isPersistant = false)] public bool useRelativeScaling;

    [KSPField(isPersistant = false)] public Version version = CurrentVersion;

    protected readonly List<WaterfallController> allControllers = new(8);
    protected readonly List<WaterfallEffect> allFX = new(16);
    protected readonly List<WaterfallEffect> activeFX = new(16);
    protected readonly List<WaterfallEffectTemplate> allTemplates = new(16);
    protected Renderer[] allRenderers;
    protected bool refreshRenderers = true;
    private bool dynamicSortRenderers = false;

    private bool hasAdditiveShaders;
    private bool _hasAlphaBlendedShaders;
    private bool hasAlphaBlendedShaders
    {
      get => _hasAlphaBlendedShaders;
      set
      {
        if (value != _hasAlphaBlendedShaders)
        {
          x_modulesWithAlphaBlendedShaders += value ? 1 : -1;
        }
        _hasAlphaBlendedShaders = value;
      }
    }

    private static int x_modulesWithAlphaBlendedShaders = 0;

    protected bool started;
    private bool isHDR;
    protected ConfigNode config;
    protected ConfigNode[] effectsNodes = { };
    protected ConfigNode[] templatesNodes = { };

    public List<WaterfallEffect> FX => allFX;

    public List<WaterfallEffectTemplate> Templates => allTemplates;

    public List<WaterfallController> Controllers => allControllers;
    bool isAwake;
    UInt64 awakeControllerMask;

    /// <summary>
    /// Sets the value of a specific controller
    /// Interface method. Used by other mods to push values to controllers
    /// </summary>
    /// <param name="controllerID"></param>
    /// <param name="value"></param>
    public void SetControllerValue(string controllerID, float value)
    {
      var controller = FindController(controllerID);
      if (controller != null)
      {
        controller.Set(value);
        awakeControllerMask |= controller.mask;
      }
      else
      {
        Utils.LogWarning($"[ModuleWaterfallFX]: Could not find controller {controllerID} to set on module {moduleID}");
      }
    }
    public WaterfallController FindController(string controllerName)
    {
      return allControllers.FirstOrDefault(c => c.name == controllerName);
    }

    public void ResetEffect(string effectName, bool playImmediately)
    {
      var effect = FindEffect(effectName);
      if (effect != null)
      {
        effect.Reset(playImmediately);
      }
      else 
      {
        Utils.LogWarning($"[ModuleWaterfallFX]: Could not find effect {effectName} on module {moduleID}");
      }
    }
    public WaterfallEffect FindEffect(string effectName)
    {
      return allFX.FirstOrDefault(x => x.name == effectName);
    }

    private void OnDestroy()
    {
      hasAdditiveShaders = false;
      hasAlphaBlendedShaders = false;
    }

    public void Start()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        Initialize();
        started = true;
      }
    }

    private void GatherRenderers()
    {
      if (refreshRenderers)
      {
        List<Renderer> renderers = new List<Renderer>();
        hasAdditiveShaders = false;
        hasAlphaBlendedShaders = false;

        for (int i = activeFX.Count; i-- > 0;)
        {
          var fx = activeFX[i];
          foreach (var renderer in fx.effectRenderers)
          {
            Material mat = renderer.material;

            // distortion effects get a constant renderqueue value, so they don't need to be sorted
            if (mat.HasProperty(ShaderPropertyID._Strength))
            {
              mat.renderQueue = Settings.DistortQueue;
              if (!Settings.EnableDistortion)
              {
                fx.CleanupEffect();
                activeFX.RemoveAt(i);
                break;
              }
            }
            else
            {
              if (mat.HasProperty(ShaderPropertyID._Intensity))
              {
                hasAlphaBlendedShaders = true;
              }
              else
              {
                hasAdditiveShaders = true;
              }

              renderers.AddUnique(renderer);
            }
          }
        }

        allRenderers = renderers.ToArray();
        refreshRenderers = false;
      }
    }

    private static readonly ProfilerMarker camerasProf = new("Waterfall.Effect.Update.Cameras");
    private void SetupRenderersForCamera(Camera camera)
    {
      // if there are no alpha blended effects in the world, we don't need dynamic sorting
      if (x_modulesWithAlphaBlendedShaders == 0)
      {
        // if we were previously dynamically sorted, reset everything to default.
        if (dynamicSortRenderers)
        {
          foreach (var renderer in allRenderers)
          {
            renderer.material.renderQueue = -1;
          }
          dynamicSortRenderers = false;
        }

        return;
      }

      camerasProf.Begin();

      dynamicSortRenderers = true;

      var c = camera.transform;
      Vector3 cameraForward = c.forward;
      Vector3 cameraPosition = c.position;
      float queueScalar = Settings.QueueDepth / Settings.SortedDepth;
      int firstQueueDelta = -1;
      bool allShadersAreSameType = !(hasAdditiveShaders && _hasAlphaBlendedShaders);

      for (int i = allRenderers.Length; i-- > 0;)
      {
        var renderer = allRenderers[i];
        if (!renderer.enabled) continue; // TODO: not sure how much time this takes but we could sort disabled renderers to the end (but would need a way to re-sort on changes)
        Material mat = renderer.material;

        int qDelta;

        // if all of the shaders are the same type, we can calculate a single queue value for the whole module
        if (allShadersAreSameType && firstQueueDelta >= 0)
        {
          qDelta = firstQueueDelta;
        }
        else
        {
          float camDistBounds = Vector3.Dot(renderer.bounds.center - cameraPosition, cameraForward);
          float camDistTransform = Vector3.Dot(renderer.transform.position - cameraPosition, cameraForward);
          qDelta = Settings.QueueDepth - (int)Mathf.Clamp(Mathf.Min(camDistBounds, camDistTransform) * queueScalar, 0, Settings.QueueDepth);

          firstQueueDelta = qDelta;

          // TODO: not sure how much time this takes but we could cache it (or store these materials separately)
          if (!hasAdditiveShaders || mat.HasProperty(ShaderPropertyID._Intensity)) // alpha blended shaders go later
            qDelta += 1;
        }

        mat.renderQueue = Settings.TransparentQueueBase + qDelta;
      }
      camerasProf.End();
    }

    private static readonly ProfilerMarker luSetup = new ProfilerMarker("Waterfall.LateUpdate.Setup");
    private static readonly ProfilerMarker luControllers = new ProfilerMarker("Waterfall.LateUpdate.Controllers");
    private static readonly ProfilerMarker luEffects = new ProfilerMarker("Waterfall.LateUpdate.Effects");

    protected void LateUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        luSetup.Begin();
        GatherRenderers();
        var camera = FlightCamera.fetch.cameras[0];
        bool changeHDR = camera.allowHDR != isHDR;
        if (changeHDR)
          isHDR = !isHDR;
        luSetup.End();
        luControllers.Begin();

        for (int i = 0; i < allControllers.Count; ++i)
        {
          var controller = allControllers[i];
          if (controller.Update())
          {
            awakeControllerMask |= controller.mask;
          }
          else if ((awakeControllerMask & controller.mask) != 0)
          {
            // when we call Initialize(), we force awakeControllerMask to ~0 in order to make sure that everything updates for one frame
            // but the modifiers check controller.awake instead of the mask, so they won't update anyway
            // TODO: remove controller.awake and use the mask everywhere (this will remove some code smells)
            controller.awake = true;
          }
        }
        luControllers.End();

        isAwake = isAwake || awakeControllerMask != 0;

        if (isAwake)
        {
          bool effectsAwake = false;
          luEffects.Begin();
          for (int fxIndex = 0; fxIndex < activeFX.Count; ++fxIndex)
          {
            var fx = activeFX[fxIndex];
            effectsAwake = fx.Update(ref awakeControllerMask) || effectsAwake;
            if (changeHDR)
              fx.SetHDR(isHDR);
          }

          if (awakeControllerMask == 0 && !effectsAwake)
          {
            isAwake = false;
          }

          awakeControllerMask = 0;

          luEffects.End();
          SetupRenderersForCamera(camera);
        }
      }
    }

    private void FetchConfigFromPrefab()
    {
      config = part.partInfo.partPrefab.FindModulesImplementing<ModuleWaterfallFX>().FirstOrDefault(x => x.moduleID == moduleID).config;
      effectsNodes = config.GetNodes(WaterfallConstants.EffectNodeName);
      templatesNodes = config.GetNodes(WaterfallConstants.TemplateNodeName);
    }

    /// <summary>
    ///   Load all CONTROLLERS, TEMPLATES and EFFECTS
    /// </summary>
    /// <param name="node"></param>
    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      Utils.Log($"[ModuleWaterfallFX]: OnLoad called with contents \n{node}", LogType.Modules);

      if (HighLogic.LoadedScene == GameScenes.LOADING)  // Store the node for later, nothing else to do now
      {
        config = node.CreateCopy();
        return;
      }
      else if (HighLogic.LoadedSceneIsEditor) return; // Nothing to do in the Editor

      // KSP behaviour is to only provide the Persistent data, everything else is in the prefab.
      if (!node.HasNode(WaterfallConstants.EffectNodeName) && !node.HasNode(WaterfallConstants.TemplateNodeName))
      {
        FetchConfigFromPrefab();
      }
      else
      {
        config = node.CreateCopy();
        if (node.HasNode(WaterfallConstants.EffectNodeName))
          effectsNodes = node.GetNodes(WaterfallConstants.EffectNodeName);
        if (node.HasNode(WaterfallConstants.TemplateNodeName))
          templatesNodes = node.GetNodes(WaterfallConstants.TemplateNodeName);
        config.RemoveNodes(WaterfallConstants.EffectNodeName);
        config.RemoveNodes(WaterfallConstants.TemplateNodeName);
        foreach (var n in effectsNodes)
          config.AddNode(n);
        foreach (var n in templatesNodes)
          config.AddNode(n);
      }

      // Wait for OnStart to initialize, if it hasn't already run. (B9PS can call after OnStart())
      if (started)
        Initialize();
    }

    /// <summary>
    ///   Dumps the entire set of EFFECTS to a confignode, then to a string
    /// </summary>
    /// <returns></returns>
    public string ExportModule()
    {
      var newNode = new ConfigNode("MODULE");
      newNode.AddValue("name", "ModuleWaterfallFX");
      newNode.AddValue("moduleID", moduleID);
      newNode.AddValue("version", version);
      //newNode.AddValue("engineID", engineID);

      foreach (var ctrl in Controllers)
      {
        newNode.AddNode(ctrl.Save());
      }

      if (Templates.Count > 0)
      {
        foreach (var templ in Templates)
        {
          newNode.AddNode(templ.Save());
        }
      }
      else
      {
        foreach (var fx in allFX)
        {
          newNode.AddNode(fx.Save());
        }
      }

      return newNode.ToString();
    }

    public string ExportControllers()
    {
      var toRet = StringBuilderCache.Acquire();
      foreach (var ctrl in Controllers)
      {
        toRet.AppendLine($"{ctrl.Save()}");
      }

      return toRet.ToStringAndRelease();
    }

    /// <summary>
    ///   Dumps the entire set of EFFECTS to a confignode, then to a string
    /// </summary>
    /// <returns></returns>
    public string ExportEffects()
    {
      var toRet = StringBuilderCache.Acquire();
      foreach (var fx in allFX)
      {
        toRet.AppendLine($"{fx.Save()}");
      }

      return toRet.ToStringAndRelease();
    }

    private void LoadEffects(ConfigNode node)
    {
      Utils.Log($"[ModuleWaterfallFX]: Loading Effects on moduleID {moduleID}", LogType.Modules);
      var effectNodes = node.GetNodes(WaterfallConstants.EffectNodeName);
      var templateNodes = node.GetNodes(WaterfallConstants.TemplateNodeName);

      foreach (var fxDataNode in effectNodes)
      {
        allFX.Add(new(fxDataNode));
      }

      Utils.Log($"[ModuleWaterfallFX]: Loading Template effects on moduleID {moduleID}", LogType.Modules);
      foreach (var templateNode in templateNodes)
      {
        var template = new WaterfallEffectTemplate(templateNode);
        allTemplates.Add(template);
        foreach (var fx in template.allFX)
        {
          allFX.Add(fx);
        }

        Utils.Log($"[ModuleWaterfallFX]: Loaded effect template {template.templateName}", LogType.Modules);
      }

      Utils.Log($"[ModuleWaterfallFX]: Finished loading {allTemplates.Count} templates", LogType.Modules);
      Utils.Log($"[ModuleWaterfallFX]: Finished loading {allFX.Count} effects", LogType.Modules);
    }

    /// <summary>
    ///   Determine controller type from config node in the old format (config node named <see cref="WaterfallConstants.LegacyControllerNodeName" /> with <see cref="WaterfallController.LegacyControllerTypeNodeName" /> property).
    ///   If value of config node entry named <see cref="WaterfallController.LegacyControllerTypeNodeName" /> is missing or incorrect, default to <see cref="ThrottleController" /> type.
    /// </summary>
    /// <remarks>
    ///   <see href="https://github.com/post-kerbin-mining-corporation/Waterfall/pull/95" /> for info regarding change to new format.
    /// </remarks>
    private EffectControllerInfo DetermineControllerTypeFromLegacyConfigNode(ConfigNode configNode)
    {
      var controllerType = EffectControllersMetadata.ControllersByType[typeof(ThrottleController)];

      string linkedToValue = null;
      if (configNode.TryGetValue(WaterfallController.LegacyControllerTypeNodeName, ref linkedToValue))
      {
        if (EffectControllersMetadata.ControllersByLegacyControllerTypeIds.TryGetValue(linkedToValue, out var legacyControllerType))
        {
          controllerType = legacyControllerType;
        }
        else
        {
          Utils.LogWarning($"[ModuleWaterfallFX]: Unknown {WaterfallController.LegacyControllerTypeNodeName} value {linkedToValue} on moduleID {moduleID}, setting throttle as default");
        }
      }
      else
      {
        Utils.LogWarning($"[ModuleWaterfallFX]: Controller on moduleID {moduleID} does not define {WaterfallController.LegacyControllerTypeNodeName} field, setting throttle as default");
      }

      return controllerType;
    }

    private void LoadControllers(ConfigNode node)
    {
      Utils.Log($"[ModuleWaterfallFX]: Loading effect controllers on moduleID {moduleID}", LogType.Modules);

      allControllers.Clear();
      foreach (var childNode in node.GetNodes())
      {
        EffectControllerInfo controllerType;

        // Check if node is either called CONTROLLER (old format) or something like THROTTLECONTROLLER (new format)
        if (childNode.name == WaterfallConstants.LegacyControllerNodeName)
        {
          controllerType = DetermineControllerTypeFromLegacyConfigNode(childNode);
        }
        else if (EffectControllersMetadata.ControllersByConfigNodeName.TryGetValue(childNode.name, out var info))
        {
          controllerType = info;
        }
        else
        {
          continue;
        }

        var controller = controllerType.CreateFromConfig(childNode);
        Utils.Log($"[ModuleWaterfallFX]: Loaded effect controller of type {controller} named {controller.name} on moduleID {moduleID}, adding to loaded controllers dictionary", LogType.Modules);

        if (FindController(controller.name) == null)
        {
          controller.mask = (1ul << allControllers.Count);
          allControllers.Add(controller);
          // NOTE: initialize gets called later
        }
        else
        {
          Utils.LogError($"[ModuleWaterfallFX]: unable to add controller {controller} named {controller.name} to controllers dictionary on moduleID {moduleID}: already exists");
        }
      }

      Utils.Log($"[ModuleWaterfallFX]: Finished loading effect controllers on moduleID {moduleID}", LogType.Modules);
    }

    // VAB Inforstrings are blank
    public string GetModuleTitle() => "";

    public override string GetInfo() => "";
    /// <summary>
    ///   Gets the list of controllers
    /// </summary>
    /// <param name="controllerName"></param>
    /// <returns></returns>
    public string[] GetControllerNames() => allControllers.Select(controller => controller.name).ToArray();

    public void AddController(WaterfallController newController)
    {
      Utils.Log("[ModuleWaterfallFX]: Added new controller", LogType.Modules);
      newController.mask = 1ul << allControllers.Count;
      allControllers.Add(newController);
      newController.Initialize(this);
      InitializeEffects();
    }

    public void RemoveController(WaterfallController toRemove)
    {
      Utils.Log("[ModuleWaterfallFX]: Deleting controller", LogType.Modules);
      int removedIndex = allControllers.IndexOf(toRemove);
      allControllers.RemoveAt(removedIndex);
      for (int i = removedIndex; i < allControllers.Count; ++i)
      {
        allControllers[i].mask >>= 1;
      }
      InitializeEffects();
    }

    public void AddEffect(WaterfallEffect newEffect)
    {
      Utils.Log("[ModuleWaterfallFX]: Added new effect", LogType.Modules);
      AddWithInitialize(newEffect, newEffect.parentTemplate, true);
    }

    public void CopyEffect(WaterfallEffect toCopy, WaterfallEffectTemplate template)
    {
      Utils.Log($"[ModuleWaterfallFX]: Copying effect {toCopy}", LogType.Modules);
      var newEffect = new WaterfallEffect(toCopy);
      AddWithInitialize(newEffect, template, false);
    }

    private void AddWithInitialize(WaterfallEffect effect, WaterfallEffectTemplate target, bool fromNothing)
    {
      if (target != null)
      {
        foreach (var t in Templates.Where(x => x == target))
        {
          t.allFX.Add(effect);
        }
      }

      allFX.Add(effect);
      if (effect.InitializeEffect(this, fromNothing, useRelativeScaling))
      {
        activeFX.Add(effect);
      }
      refreshRenderers = true;
    }

    public void RemoveEffect(WaterfallEffect toRemove)
    {
      Utils.Log("[ModuleWaterfallFX]: Deleting effect", LogType.Modules);

      toRemove.CleanupEffect();
      if (toRemove.parentTemplate != null)
      {
        foreach (var t in Templates)
        {
          if (t == toRemove.parentTemplate)
          {
            t.allFX.Remove(toRemove);
          }
        }
      }

      allFX.Remove(toRemove);
      refreshRenderers = true;
    }

    /// <summary>
    ///   Does initialization of everything woo
    /// </summary>
    protected void Initialize()
    {
      Utils.Log("[ModuleWaterfallFX]: Initializing", LogType.Modules);

      // Some shaders require the depth texture; force-enable that.
      FlightCamera.fetch.mainCamera.depthTextureMode |= DepthTextureMode.Depth;

      // we load controllers and effects here instead of OnLoad because OnLoad will not be called for modules that exist in a prefab but not craft file
      // e.g. if a craft file was saved without waterfall installed
      if (config == null)
      {
        FetchConfigFromPrefab();
      }
      
      if (config != null)
      {
        CleanupEffects();
        allFX.Clear();
        allTemplates.Clear();
        allControllers.Clear();

        if (!config.HasValue(nameof(version)))
        {
          version = Version.Initial;
        }

        LoadControllers(config);
        LoadEffects(config);
      }

      InitializeControllers();
      awakeControllerMask = ~0ul;
      InitializeEffects();

      UpgradeToCurrentVersion();

      isAwake = true;
    }

    private void UpgradeToCurrentVersion()
    {
      if (version < CurrentVersion)
      {
        foreach (var c in Controllers)
        {
          c.UpgradeToCurrentVersion(version);
        }
        version = CurrentVersion;
      }
    }

    /// <summary>
    ///   Initialize the CONTROLLERS
    /// </summary>
    protected void InitializeControllers()
    {
      Utils.Log("[ModuleWaterfallFX]: Initializing Controllers", LogType.Modules);
      for (int i = 0; i < allControllers.Count; ++i)
      {
        var controller = allControllers[i];
        Utils.Log($"[ModuleWaterfallFX]: Initializing controller {controller.name}", LogType.Modules);
        controller.mask = 1ul << i;
        controller.Initialize(this);
      }
    }

    /// <summary>
    ///   Initialize the EFFECTS
    /// </summary>
    protected void InitializeEffects()
    {
      Utils.Log("[ModuleWaterfallFX]: Initializing Effects", LogType.Modules);
      activeFX.Clear();
      foreach (var fx in allFX)
      {
        Utils.Log($"[ModuleWaterfallFX]: Initializing effect {fx.name}", LogType.Modules);
        if (fx.InitializeEffect(this, false, useRelativeScaling))
        {
          activeFX.Add(fx);
        }
      }
      refreshRenderers = true;
    }

    protected void CleanupEffects()
    {
      Utils.Log("[ModuleWaterfallFX]: Cleanup Effects", LogType.Modules);
      foreach (var fx in allFX)
      {
        fx.CleanupEffect();
      }
    }
  }
}
