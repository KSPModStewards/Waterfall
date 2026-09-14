using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Waterfall
{
  /// <summary>
  ///   A controller that pulls from the current engine's thrust. Returns a fractional thrust value
  ///   normalized to [0, 1] where 1 corresponds to the max thrust possible under current conditions.
  /// </summary>
  [Serializable]
  [DisplayName("Thrust")]
  public class ThrustController : WaterfallController
  {
    [Persistent] public string engineID = String.Empty;
    public  float         currentThrustFraction;
    private ModuleEngines engineController;

    public ThrustController() : base() { }
    public ThrustController(ConfigNode node) : base(node) { }

    public override void Initialize(ModuleWaterfallFX host)
    {
      base.Initialize(host);

      values = new float[1];

      engineController = host.GetComponents<ModuleEngines>().FirstOrDefault(x => x.engineID == engineID);
      if (engineController == null)
      {
        Utils.Log($"[ThrustController] Could not find engine ID {engineID}, using first module");
        engineController = host.part.FindModuleImplementing<ModuleEngines>();
      }

      if (engineController == null)
        Utils.LogError("[ThrustController] Could not find engine controller on Initialize");
    }

    protected override float UpdateSingleValue()
    {
      if (engineController == null)
      {
        Utils.LogWarning("[ThrustController] Engine controller not assigned");
        currentThrustFraction = 0;
      }
      else if (!engineController.isOperational)
        currentThrustFraction = 0f;
      else
      {
        // Thanks to NathanKell for the formula.
        currentThrustFraction = engineController.fuelFlowGui
                              / engineController.maxFuelFlow
                              / (float)engineController.ratioSum
                              * engineController.mixtureDensity
                              * engineController.multIsp;
      }

      return currentThrustFraction;
    }
  }
}
