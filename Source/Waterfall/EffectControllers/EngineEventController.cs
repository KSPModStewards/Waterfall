using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Waterfall
{
  [DisplayName("Engine Event")]
  public class EngineEventController : WaterfallController
  {
    [Persistent] public string eventName;
    [Persistent] public string engineID;

    public FastFloatCurve eventCurve = new();
    [Persistent] public float eventDuration = 1f;
    private ModuleEngines engineModule;
    private MultiModeEngine multiEngine;

    private Func<ModuleEngines, bool> getEngineStateFunc; // when the result of this function transitions from false -> true, the event should fire
    private bool  eventPlaying;
    private bool  eventReady;     // the event can fire on the next transition
    private float eventTime;

    private static readonly Dictionary<string, Func<ModuleEngines, bool>> EngineStateFuncs = new()
    {
      { "flameout", (engineModule) => engineModule.flameout || !engineModule.EngineIgnited},
      { "ignition", (engineModule) => engineModule.EngineIgnited}
    };

    public EngineEventController() : base() { }
    public EngineEventController(ConfigNode node) : base(node)
    {
      eventCurve.Load(node.GetNode(nameof(eventCurve)));
    }

    public override ConfigNode Save()
    {
      var c = base.Save();
      c.AddNode(Utils.SerializeFloatCurve(nameof(eventCurve), eventCurve));
      return c;
    }

    public override void Initialize(ModuleWaterfallFX host)
    {
      base.Initialize(host);

      values = new float[1];

      engineModule = host.GetComponents<ModuleEngines>().FirstOrDefault(x => x.engineID == engineID);
      if (engineModule == null)
      {
        Utils.Log($"[EngineEventController] Could not find engine ID {engineID}, using first module");
        engineModule = host.part.FindModuleImplementing<ModuleEngines>();
      }
      multiEngine = host.GetComponent<MultiModeEngine>();
      if (multiEngine == null)
      {
      }

      EngineStateFuncs.TryGetValue(eventName, out getEngineStateFunc);

      if (getEngineStateFunc != null)
      {
        eventReady = !getEngineStateFunc(engineModule);
      }
      else
      {
        Utils.LogError($"[EngineEventController] Invalid engine eventName '{eventName}' in waterfall moduleID '{host.moduleID}' engine '{host.engineID}' in part '{host.part.name}'");
      }
    }

    protected override float UpdateSingleValue()
    {
      if (engineModule != null && getEngineStateFunc != null)
        return eventCurve.Evaluate(CheckStateChange());

      return 0;
    }

    public float CheckStateChange()
    {
      if (eventReady)
      {
        /// Check if engine state flipped
        if (getEngineStateFunc(engineModule))
        {
          Utils.Log($"[EngineEventController] {eventName} fired on {engineID}");
          eventReady   = false;
          eventPlaying = true;
          eventTime    = TimeWarp.deltaTime;
          return eventTime;
        }
      }
      else if (eventPlaying)
      {
        eventTime += TimeWarp.deltaTime;
        eventPlaying = eventTime <= eventDuration;
        return eventTime;
      }
      else if (!eventPlaying && !eventReady)
      {
        // Check to see if event can be reset
        if (!getEngineStateFunc(engineModule))
        {
          Utils.Log($"[EngineEventController] {eventName} ready on {engineID}");
          eventReady = true;
        }
      }

      return 0f;
    }

    public override void UpgradeToCurrentVersion(Version loadedVersion)
    {
      base.UpgradeToCurrentVersion(loadedVersion);

      if (loadedVersion < Version.FixedRampRates)
      {
        float scaleFactor = 1.0f / Math.Max(1, referencingModifierCount);

        eventDuration *= scaleFactor;
        
        for (int i = 0; i < eventCurve.KeyCount; ++i)
        {
          var key = eventCurve[i];

          key.time *= scaleFactor;
          key.inTangent /= scaleFactor;
          key.outTangent /= scaleFactor;

          eventCurve[i] = key;
        }

        eventCurve.Compile();
      }
    }
  }
}
