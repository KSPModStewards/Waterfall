using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Waterfall.UI;
using Waterfall.UI.EffectControllersUI;

namespace Waterfall.EffectControllers
{
  /// <summary>
  ///   Provides list of all effect controllers currently implemented in Waterfall as well as method to create them.
  /// </summary>
  /// <remarks>
  ///   Unfortunately had to use a little reflection magic for controller types discovery and binding to UI options type and config node name.
  /// </remarks>
  public static class EffectControllersMetadata
  {
    public static readonly IReadOnlyCollection<EffectControllerInfo> Controllers;

    /// <summary>
    ///   Maps controller type to metadata.
    /// </summary>
    public static readonly IReadOnlyDictionary<Type, EffectControllerInfo> ControllersByType;

    /// <summary>
    ///   Maps controller config node name (which is uppercase controller type name) to metadata.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, EffectControllerInfo> ControllersByConfigNodeName;

    /// <summary>
    ///   Maps old "linkedTo" values to corresponding controllers metadata. These were used before field "linkedTo" was replaced by using controller type names for serialization.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, EffectControllerInfo> ControllersByLegacyControllerTypeIds;

    static EffectControllersMetadata()
    {
      var waterfallAssembly = typeof(EffectControllersMetadata).Assembly;
      var baseType          = typeof(WaterfallController);
      var controllerTypes = waterfallAssembly
        .GetTypes()
        .Where(t => t != baseType && baseType.IsAssignableFrom(t))
        .ToArray();

      Controllers = controllerTypes
        .Select(type => new EffectControllerInfo(type))
        .ToArray();

      ControllersByType           = Controllers.ToDictionary(c => c.ControllerType);
      ControllersByConfigNodeName = Controllers.ToDictionary(c => c.ConfigNodeName);

      ControllersByLegacyControllerTypeIds = new Dictionary<string, EffectControllerInfo>
      {
        ["atmosphere_density"] = ControllersByType[typeof(AtmosphereDensityController)],
        ["custom"]             = ControllersByType[typeof(CustomPushController)],
        ["gimbal"]             = ControllersByType[typeof(GimbalController)],
        ["light"]              = ControllersByType[typeof(LightController)],
        ["mach"]               = ControllersByType[typeof(MachController)],
        ["random"]             = ControllersByType[typeof(RandomnessController)],
        ["rcs"]                = ControllersByType[typeof(RCSController)],
        ["throttle"]           = ControllersByType[typeof(ThrottleController)],
        ["engineEvent"]        = ControllersByType[typeof(EngineEventController)],
        ["thrust"]             = ControllersByType[typeof(ThrustController)],
        ["velocity"]           = ControllersByType[typeof(VelocityController)]
      };
    }

    public static string GetConfigNodeName(Type controllerType) => controllerType.Name.ToUpperInvariant();
  }

  public class EffectControllerInfo
  {
    public readonly  Type            ControllerType;
    public readonly  string          DisplayName;
    public readonly  string          ConfigNodeName;
    private readonly ConstructorInfo deserializeConstructor;

    public EffectControllerInfo(Type controllerType)
    {
      ControllerType = controllerType;
      DisplayName    = controllerType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? ControllerType.Name;

      deserializeConstructor = controllerType.GetConstructor(new[] { typeof(ConfigNode) });

      if (deserializeConstructor == null)
        throw new InvalidOperationException($"Unable to get ConfigNode constructor for controller of type {controllerType}");

      ConfigNodeName = EffectControllersMetadata.GetConfigNodeName(controllerType);

      Utils.Log($"[{nameof(EffectControllersMetadata)}]: Registered controller type {ControllerType}");
    }

    public WaterfallController CreateFromConfig(ConfigNode node)
    {
      var controller = (WaterfallController)deserializeConstructor.Invoke(new object[] { node });
      return controller;
    }

    /// <summary>
    ///   Find corresponding type implementing <see cref="IEffectControllerUIOptions" /> and create new instance.
    ///   Inject UIResources dependency if necessary.
    /// </summary>
    public IEffectControllerUIOptions CreateUIOptions()
    {
      var waterfallAssembly = typeof(EffectControllersMetadata).Assembly;
      var baseType          = typeof(DefaultEffectControllerUIOptions<>);

      var optionsType = waterfallAssembly
        .GetTypes()
        .First(t => t.BaseType is { IsConstructedGenericType: true }
                 && t.BaseType.GetGenericTypeDefinition()            == baseType
                 && t.BaseType.GenericTypeArguments.FirstOrDefault() == ControllerType);

      object options = optionsType.GetConstructor(Type.EmptyTypes)?.Invoke(new object[0]);

      if (options == null)
        throw new InvalidOperationException($"Unable to construct UI options for type {ControllerType}");

      return (IEffectControllerUIOptions)options;
    }
  }
}