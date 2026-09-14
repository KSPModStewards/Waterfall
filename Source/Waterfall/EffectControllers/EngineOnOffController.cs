using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Waterfall
{
  [DisplayName("Engine On State")]
  public class EngineOnOffController : WaterfallController
  {
    public float currentState = 0f;
    [Persistent] public string engineID = String.Empty;
    [Persistent] public float responseRateUp = 100f;
    [Persistent] public float responseRateDown = 100f;
    private ModuleEngines engineController;

    public bool zeroOnFlameout = true;

    public EngineOnOffController() : base() { }
    public EngineOnOffController(ConfigNode node) : base(node) { }

    public override void Initialize(ModuleWaterfallFX host)
    {
      base.Initialize(host);

      values = new float[1];

      engineController = host.GetComponents<ModuleEngines>().FirstOrDefault(x => x.engineID == engineID);
      if (engineController == null)
      {
        Utils.Log($"[EngineOnOffController] Could not find engine ID {engineID}, using first module");
        engineController = host.part.FindModuleImplementing<ModuleEngines>();
      }
      else
      {
        currentState = engineController.isOperational ? 1f : 0f;
      }

      if (engineController == null)
        Utils.LogError("[EngineOnOffController] Could not find engine controller on Initialize");
    }

    protected override float UpdateSingleValue()
    {
      if (engineController == null)
      {
        Utils.LogWarning("[EngineOnOffController] Engine controller not assigned");
        return 0f;
      }
      else if (zeroOnFlameout)
      {
        float targetThrottle = engineController.isOperational && !engineController.flameout ? 1f : 0f;

        if (currentState != targetThrottle)
        {
          float rampRate = targetThrottle > currentState ? responseRateUp : responseRateDown;
          currentState = Mathf.MoveTowards(currentState, targetThrottle, rampRate * TimeWarp.deltaTime);
        }

        return currentState;
      }
      return currentState;
    }
  }
}
   
