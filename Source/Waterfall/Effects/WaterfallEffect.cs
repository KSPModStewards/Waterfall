using System;
using System.Collections.Generic;
using UniLinq;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Waterfall
{
  /// <summary>
  ///   This class represents a single effect. An effect contains a MODEL with various PROPERTIES, modified by EFFECTS
  /// </summary>
  public class WaterfallEffect
  {
    public          string                           name       = "";
    public          string                           parentName = "";
    public readonly List<Vector3>                    baseScales = new();
    public          ModuleWaterfallFX                parentModule;
    public          WaterfallEffectTemplate          parentTemplate;

    public readonly List<EffectIntegrator> intensityTestIntegrators = new();
    public readonly List<EffectIntegrator> otherIntegrators = new();

    private HashSet<string> disabledTransformNames = new();
    private UInt64 usedControllerMask = 0;

    protected           WaterfallModel       model;
    protected readonly  List<EffectModifier> fxModifiers = new ();
    protected readonly  List<DirectModifier> directModifiers = new();
    protected           ConfigNode           savedNode;
    protected           bool                 effectVisible = true;
    protected           Vector3              savedScale;
    protected readonly  List<Transform>      effectTransforms = new();
    private   readonly  List<Material>       effectRendererMaterials = new();
    private   readonly  List<Transform>      effectRendererTransforms = new();
    internal  readonly  List<Renderer>       effectRenderers = new();

    public WaterfallEffect(string parent, WaterfallModel mdl, WaterfallEffectTemplate templateOwner = null)
    {
      if (templateOwner != null)
        parentTemplate = templateOwner;
      parentName             = parent;
      model                  = mdl;
      TemplatePositionOffset = templateOwner != null ? parentTemplate.position : Vector3.zero;
      TemplateRotationOffset = templateOwner != null ? parentTemplate.rotation : Vector3.zero;
      TemplateScaleOffset    = templateOwner != null ? parentTemplate.scale : Vector3.one;
    }

    public WaterfallEffect(ConfigNode node, WaterfallEffectTemplate templateOwner = null)
    {
      if (templateOwner != null)
        parentTemplate = templateOwner;
      TemplatePositionOffset = Vector3.zero;
      TemplateRotationOffset = Vector3.zero;
      TemplateScaleOffset    = Vector3.one;
      Load(node);
    }

    public WaterfallEffect(WaterfallEffect fx, WaterfallEffectTemplate templateOwner)
    {
      parentTemplate         = templateOwner;
      TemplatePositionOffset = parentTemplate.position;
      TemplateRotationOffset = parentTemplate.rotation;
      TemplateScaleOffset    = parentTemplate.scale;

      if (parentTemplate.overrideParentTransform != "" && parentTemplate.overrideParentTransform != null)
        fx.savedNode.SetValue("parentName", parentTemplate.overrideParentTransform, true);

      Load(fx.savedNode);
    }

    public WaterfallEffect(WaterfallEffect fx)
    {
      parentTemplate         = fx.parentTemplate;
      TemplatePositionOffset = fx.TemplatePositionOffset;
      TemplateRotationOffset = fx.TemplateRotationOffset;
      TemplateScaleOffset    = fx.TemplateScaleOffset;
      Load(fx.Save());
    }

    public override string ToString()
    {
      return name + ":" + parentName;
    }

    public Vector3 TemplatePositionOffset { get; set; }
    public Vector3 TemplateRotationOffset { get; set; }
    public Vector3 TemplateScaleOffset    { get; set; }

    public List<EffectModifier> FXModifiers => fxModifiers;

    public WaterfallModel FXModel => model;

    public ConfigNode SavedNode => savedNode;

    /// <summary>
    ///   Load the node from config
    /// </summary>
    /// <param name="node"></param>
    public void Load(ConfigNode node)
    {
      savedNode = node;
      node.TryGetValue(nameof(name), ref name);

      if (!node.TryGetValue(nameof(parentName), ref parentName))
      {
        Utils.LogError(String.Format("[WaterfallEffect]: EFFECT with name {0} does not define parentName, which is required", name));
        return;
      }

      model       = new(node.GetNode(WaterfallConstants.ModelNodeName));
      fxModifiers.Clear();

      // types
      var positionNodes = node.GetNodes(WaterfallConstants.PositionModifierNodeName);
      var rotationNodes = node.GetNodes(WaterfallConstants.RotationModifierNodeName);
      var scalingNodes  = node.GetNodes(WaterfallConstants.ScaleModifierNodeName);
      var colorNodes    = node.GetNodes(WaterfallConstants.ColorModifierNodeName);
      var uvOffsetNodes = node.GetNodes(WaterfallConstants.UVScrollModifierNodeName);
      var floatNodes    = node.GetNodes(WaterfallConstants.FloatModifierNodeName);

      var colorLightNodes = node.GetNodes(WaterfallConstants.ColorFromLightNodeName);

      var lightFloatNodes = node.GetNodes(WaterfallConstants.LightFloatModifierNodeName);
      var lightColorNodes = node.GetNodes(WaterfallConstants.LightColorModifierNodeName);

      var particleNumericNodes = node.GetNodes(WaterfallConstants.ParticleNumericModifierNodeName);
      var particleColorNodes = node.GetNodes(WaterfallConstants.ParticleColorModifierNodeName);

      foreach (var subNode in positionNodes)

      {
        fxModifiers.Add(new EffectPositionModifier(subNode));
      }

      foreach (var subNode in rotationNodes)
      {
        fxModifiers.Add(new EffectRotationModifier(subNode));
      }

      foreach (var subNode in scalingNodes)
      {
        fxModifiers.Add(new EffectScaleModifier(subNode));
      }

      foreach (var subNode in colorNodes)
      {
        fxModifiers.Add(new EffectColorModifier(subNode));
      }

      foreach (var subNode in uvOffsetNodes)
      {
        fxModifiers.Add(new EffectUVScrollModifier(subNode));
      }

      foreach (var subNode in floatNodes)
      {
        fxModifiers.Add(new EffectFloatModifier(subNode));
      }

      foreach (var subNode in colorLightNodes)
      {
        fxModifiers.Add(new EffectColorFromLightModifier(subNode));
      }

      foreach (var subNode in lightFloatNodes)
      {
        fxModifiers.Add(new EffectLightFloatModifier(subNode));
      }

      foreach (var subNode in lightColorNodes)
      {
        fxModifiers.Add(new EffectLightColorModifier(subNode));
      }
      foreach (var subNode in particleNumericNodes)
      {
        fxModifiers.Add(new EffectParticleMultiNumericModifier(subNode));
      }
      foreach (var subNode in particleColorNodes)
      {
        fxModifiers.Add(new EffectParticleMultiColorModifier(subNode));
      }
    }

    public ConfigNode Save()
    {
      var node = new ConfigNode();
      node.name = WaterfallConstants.EffectNodeName;
      node.AddValue("name",       name);
      node.AddValue("parentName", parentName);
      node.AddNode(model.Save());
      foreach (var fx in fxModifiers)
      {
        node.AddNode(fx.Save());
      }

      return node;
    }

    public void CleanupEffect()
    {
      Utils.Log($"[WaterfallEffect]: Deleting effect {name}");
      model.Cleanup();
    }
    public void Reset(bool playImmediately)
    {
      model.ResetParticleSystem(playImmediately);
    }
    public bool InitializeEffect(ModuleWaterfallFX host, bool fromNothing, bool useRelativeScaling)
    {
      parentModule = host;
      var parents = parentModule.part.FindModelTransforms(parentName);
      Utils.Log($"[WaterfallEffect]: Initializing effect {name} at {parentName} [{parents.Length} instances]; relative scaling: {useRelativeScaling}");

      effectTransforms.Clear();
      baseScales.Clear();
      usedControllerMask = 0;

      foreach (var parent in parents)
      {
        if (parents == null)
        {
          Utils.LogError($"[WaterfallEffect]: Trying to attach effect to null parent transform {parentName} on model");
          continue;
        }
        else if (!parent.gameObject.activeInHierarchy)
        {
          // The stock ModulePartVariants will deactivate transforms that shouldn't exist on the current variant
          // This assumes that we can respond to any changes in activation state by reinitializing the effects
          continue;
        }

        int i = effectTransforms.Count;

        var effect          = new GameObject($"Waterfall_FX_{name}_{i}");
        var effectTransform = effect.transform;

        effectTransform.SetParent(parent, true);
        effectTransform.localPosition    = Vector3.zero;
        effectTransform.localEulerAngles = Vector3.zero;
        if (useRelativeScaling)
          effectTransform.localScale = Vector3.one;

        model.Initialize(effectTransform, fromNothing);

        baseScales.Add(effectTransform.localScale);
        Utils.Log($"[WaterfallEffect] Scale: {effectTransform.localScale}");

        effectTransform.localPosition    = TemplatePositionOffset;
        effectTransform.localEulerAngles = TemplateRotationOffset;
        effectTransform.localScale       = Vector3.Scale(baseScales[i], TemplateScaleOffset);

        Utils.Log($"[WaterfallEffect] local Scale {effectTransform.localScale}, baseScale, {baseScales[i]}, {Vector3.Scale(baseScales[i], TemplateScaleOffset)}");

        Utils.Log($"[WaterfallEffect] Applied template offsets {TemplatePositionOffset}, {TemplateRotationOffset}, {TemplateScaleOffset}");

        effectTransforms.Add(effectTransform);
      }

      foreach (var fx in fxModifiers)
      {
        fx.Init(this);
      }

      if (effectTransforms.Count == 0)
      {
        return false;
      }

      effectRenderers.Clear();
      effectRendererMaterials.Clear();
      effectRendererTransforms.Clear();
      foreach (var t in model.modelTransforms)
      {
        foreach (var r in t.GetComponentsInChildren<Renderer>())
        {
          effectRenderers.Add(r);
          effectRendererMaterials.Add(r.material);
          effectRendererTransforms.Add(r.transform);
        }
      }

      intensityTestIntegrators.Clear();
      otherIntegrators.Clear();

      foreach (var mod in fxModifiers)
      {
        AddModifierToIntegratorList(mod);
      }

      SortIntegrators();
      
      return true;
    }

    void AddModifierToIntegratorList(EffectModifier mod)
    {
      if (mod is DirectModifier directMod) directModifiers.Add(directMod);
      else if (mod.TestIntensity) mod.CreateOrAttachToIntegrator(intensityTestIntegrators);
      else mod.CreateOrAttachToIntegrator(otherIntegrators);
      usedControllerMask |= mod.GetControllerMask();
    }

    void SortIntegrators()
    {
      // Integrators need to be sorted such that:
      // 1. integrators are grouped by transform (so once we hit one on a disabled transform, we skip all of the ones on that transform)
      //    TODO: maybe we should sort by whether the transform is disabled?  Might have to re-sort as transforms enable/disable

      Comparison<EffectIntegrator> OrderByTransform = (EffectIntegrator a, EffectIntegrator b) => string.Compare(a.transformName, b.transformName);

      intensityTestIntegrators.RemoveAll(integrator => !integrator.Valid);
      otherIntegrators.RemoveAll(integrator => !integrator.Valid);

      intensityTestIntegrators.Sort(OrderByTransform);
      otherIntegrators.Sort(OrderByTransform);
    }

    public void ApplyTemplateOffsets(Vector3 position, Vector3 rotation, Vector3 scale)
    {
      TemplatePositionOffset = position;
      TemplateRotationOffset = rotation;
      TemplateScaleOffset    = scale;

      Utils.Log($"[WaterfallEffect] Applying template offsets from FN2 {position}, {rotation}, {scale}");


      for (int i = 0; i < effectTransforms.Count; i++)
      {
        effectTransforms[i].localPosition = TemplatePositionOffset;
        effectTransforms[i].localScale    = Vector3.Scale(baseScales[i], TemplateScaleOffset);

        if (TemplateRotationOffset == Vector3.zero)
        {
          effectTransforms[i].localRotation = Quaternion.identity;
        }
        else
        {
          effectTransforms[i].localEulerAngles = TemplateRotationOffset;
        }
      }
    }

    public List<Transform> GetModelTransforms() => model.modelTransforms;
    public List<Transform> GetEffectTransforms() => effectTransforms;

    private static readonly ProfilerMarker s_Update = new ProfilerMarker("Waterfall.Effect.Update");
    private static readonly ProfilerMarker s_fxApply = new ProfilerMarker("Waterfall.Effect.Update.FxApply");
    private static readonly ProfilerMarker s_Integrators = new ProfilerMarker("Waterfall.Effect.Update.Integrators");

    private static readonly float[] EmptyControllerValues = new float[1];

    private void UpdateIntegratorArray<T>(List<T> integrators, UInt64 awakeControllerMask) where T : EffectIntegrator
    {
      for (int i = integrators.Count-1; i >= 0;)
      {
        var integrator = integrators[i];

        // skip the block of integrators with the same transform name
        if (disabledTransformNames.Contains(integrator.transformName))
        {
          // TODO: we could build a table that would let us jump to the next one immediately instead of looping
          string transformName = integrator.transformName;
          while (i-- > 0 && integrators[i].transformName == transformName) ;
          continue;
        }
        else if (integrator.NeedsUpdate(awakeControllerMask))
        {
          integrator.Update();
        }
        --i;
      }
    }

    private bool UpdateIntegratorArray_TestIntensity(List<EffectIntegrator> integrators, ref UInt64 awakeControllerMask)
    {
      bool anyActive = false;

      for (int i = integrators.Count; i-- > 0;)
      {
        var integrator = integrators[i];

        bool wasActive = integrator.active;
        if (integrator.NeedsUpdate(awakeControllerMask))
        {
          integrator.Update();
        }
        
        if (integrator.active)
        {
          anyActive = true;
          if (!wasActive)
          {
            // when an integrator becomes active, we need to force all modifiers for that transform to update because they may have cached an old controller value that has gone to sleep
            // We could do this by storing extra state on the modifiers, but just marking all controllers as awake for a frame works too and is simpler
            // This shouldn't happen very often, only during engine ignition etc.
            // this whole thing could use a refactor, because there's two sources of truth about which controllers are awake..
            foreach (var controller in parentModule.Controllers)
            {
              controller.awake = true;
            }
            awakeControllerMask = ~0ul;
          }
        }
        // if this integrator controls the visibility for a specific transform and it's turned off, we can skip the remaining integrators on this transform
        else
        {
          string transformName = integrator.transformName;
          disabledTransformNames.Add(transformName);
        }
      }

      return anyActive;
    }

    public bool Update(ref UInt64 awakeControllerMask)
    {
      s_Update.Begin();

      bool anyActive = false;

      if (effectVisible)
      {
        s_fxApply.Begin();
        foreach (var fx in directModifiers)
        {
          float[] controllerData = fx.Controller == null ? EmptyControllerValues : fx.Controller.Get();
          fx.Apply(controllerData);
        }
        s_fxApply.End();

        if ((awakeControllerMask & usedControllerMask) != 0)
        {
          s_Integrators.Begin();

          disabledTransformNames.Clear();
          anyActive = UpdateIntegratorArray_TestIntensity(intensityTestIntegrators, ref awakeControllerMask);
          UpdateIntegratorArray(otherIntegrators, awakeControllerMask);

          s_Integrators.End();
        }
      }
      s_Update.End();

      return anyActive;
    }

    public void SetHDR(bool isHDR)
    {
      float destMode = Settings.EnableLegacyBlendModes ? 6 : 1;

      foreach (var mat in effectRendererMaterials)
      {
        if (mat.HasProperty(ShaderPropertyID._DestMode))
        {
          mat.SetFloat(ShaderPropertyID._DestMode, isHDR ? 1 : destMode);
          mat.SetFloat(ShaderPropertyID._ClipBrightness, isHDR ? 50: 1);
        }
      }
    }

    public void RemoveModifier(EffectModifier mod)
    {
      fxModifiers.Remove(mod);

      // it may be more reasonable to reinitialize the entire effect rather than trying to keep everything in sync...

      if (mod is DirectModifier directMod) directModifiers.Remove(directMod);
      else if (mod.TestIntensity) mod.RemoveFromIntegrator(intensityTestIntegrators);
      else mod.RemoveFromIntegrator(otherIntegrators);

      usedControllerMask = 0;
      foreach (var modifier in fxModifiers)
      {
        usedControllerMask |= modifier.GetControllerMask();
      }
    }

    public void ModifierParameterChange(EffectModifier mod)
    {
      RemoveModifier(mod);
      AddModifier(mod);
    }

    public void AddModifier(EffectModifier mod)
    {
      mod.Init(this);
      fxModifiers.Add(mod);
      AddModifierToIntegratorList(mod);
      SortIntegrators();
    }

    public void MoveModifierFromTo(int oldIndex, int newIndex)
    {
      oldIndex = Mathf.Clamp(oldIndex, 0, fxModifiers.Count - 1);
      newIndex = Mathf.Clamp(newIndex, 0, fxModifiers.Count - 1);

      var item = fxModifiers[oldIndex];
      fxModifiers.RemoveAt(oldIndex);
      fxModifiers.Insert(newIndex, item);

      // make sure the integrator's modifier ordering matches
      // this is overkill, we only really need to check its neighbors in the list - but this is editor-only code
      item.integrator.handledModifiers.OrderBy(mod => fxModifiers.IndexOf(mod));
      directModifiers.OrderBy(mod => fxModifiers.IndexOf(mod));
    }

    public void MoveModifierUp(int index) => MoveModifierFromTo(index, index - 1);
    public void MoveModifierDown(int index) => MoveModifierFromTo(index, index + 1);

    public void SetEnabled(bool state)
    {
      for (int i = 0; i < effectTransforms.Count; i++)
        effectTransforms[i].localScale = state ? Vector3.Scale(baseScales[i], TemplateScaleOffset) : Vector3.one * 0.00001f;
      effectVisible = state;

      foreach (var r in effectRenderers)
      {
        r.enabled = state;
      }

      foreach (var waterfallLight in model.lights)
      {
        foreach (var light in waterfallLight.lights)
        {
          light.enabled = state;
        }
      }
    }
  }
}
