using System.Collections.Generic;
using UnityEngine;

namespace Waterfall
{
  /// <summary>
  /// Manages a ParticleSystem by providing interfaces to modify it, serialize and deserialize it
  /// </summary>
  public class WaterfallParticle
  {
    public string transformName = "";
    public string baseTransformName = "";
    public bool worldSpaceAlternateSimulation = false;
    public List<ParticleSystem> systems;
    public List<WaterfallParticleProperty> pProperties;

    public WaterfallParticle()
    {
      pProperties = new();
    }

    public WaterfallParticle(ConfigNode node)
    {
      Load(node);
    }

    /// <summary>
    /// Deserialize from ConfigNode
    /// </summary>
    /// <param name="node"></param>
    public void Load(ConfigNode node)
    {
      node.TryGetValue("transform", ref transformName);
      node.TryGetValue("baseTransform", ref baseTransformName);
      node.TryGetValue("worldSpaceAlternateSimulation", ref worldSpaceAlternateSimulation);
      pProperties = new();
      foreach (var subnode in node.GetNodes(WaterfallConstants.FloatNodeName))
      {
        pProperties.Add(new WaterfallParticleNumericProperty(subnode));
      }
    }
    /// <summary>
    /// Serialize to ConfigNode
    /// </summary>
    /// <returns></returns>
    public ConfigNode Save()
    {
      ConfigNode node = new();
      node.name = WaterfallConstants.ParticleNodeName;

      if (baseTransformName != "")
      {
        node.AddValue("baseTransform", baseTransformName);
      }
      if (transformName != "")
      {
        node.AddValue("transform", transformName);
      }
      node.AddValue("worldSpaceAlternateSimulation", worldSpaceAlternateSimulation);
      foreach (var p in pProperties)
      {
        node.AddNode(p.Save());
      }
      return node;
    }

    public void Initialize(Transform parentTransform)
    {
      systems = new();

      var particleTarget = parentTransform.FindDeepChild(transformName);
      if (particleTarget != null)
      {
        ParticleSystem targetParticleSystem = particleTarget.GetComponent<ParticleSystem>();
        if (targetParticleSystem != null)
        {
          systems.Add(targetParticleSystem);
          
        }
      }
      foreach (var p in pProperties)
      {
        foreach (var sys in systems)
        {
          p.Initialize(sys);
        }
      }
      Utils.Log($"[WaterfallParticle]: Initialized Waterfall Particle at {parentTransform}, tracking {transformName}");
    }

    public void Reset(bool playImmediately)
    {
      foreach (var sys in systems)
      {
        sys.Clear();
        if (playImmediately)
        {
          sys.Simulate(0.0f, false, true);
          sys.Play();
        }
      }
    }
    /// <summary>
    /// Sets the Particle's module state and updates the module state of the attached systems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="state"></param>
    public void SetParticleModuleState(string propertyName, bool state)
    {
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleProperty t && prop != null)
      {
        t.moduleEnabled = state;
      }
      if (prop == null)
      {
        if (WaterfallParticleLoader.GetParticlePropertyMap()[propertyName].type == WaterfallParticlePropertyType.Numeric)
        {
          var newProp = new WaterfallParticleNumericProperty
          {
            propertyName = propertyName,
            moduleEnabled = state
          };
          pProperties.Add(newProp);
        }
        if (WaterfallParticleLoader.GetParticlePropertyMap()[propertyName].type == WaterfallParticlePropertyType.Color)
        {
          var newProp = new WaterfallParticleColorProperty
          {
            propertyName = propertyName,
            moduleEnabled = state
          };
          pProperties.Add(newProp);
        }
      }

      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleModuleState(propertyName, particle, state);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetParticleValue(string propertyName, float value)
    {
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleNumericProperty t && prop != null)
      {
        t.curveMode = ParticleSystemCurveMode.Constant;
        t.constant1Value = value;
      }
      else
      {
        var newProp = new WaterfallParticleNumericProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemCurveMode.Constant,
          constant1Value = value,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    public void SetParticleValue(string propertyName, float value1, float value2)
    {

      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleNumericProperty t && prop != null)
      {
        t.curveMode = ParticleSystemCurveMode.TwoConstants;
        t.constant1Value = value1;
        t.constant2Value = value2;
      }
      else
      {
        var newProp = new WaterfallParticleNumericProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemCurveMode.TwoConstants,
          constant1Value = value1,
          constant2Value = value2,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value1, value2);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetParticleValue(string propertyName, FastFloatCurve value)
    {
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleNumericProperty t && prop != null)
      {
        t.curveMode = ParticleSystemCurveMode.Curve;
        t.curve1Value = value;
      }
      else
      {
        var newProp = new WaterfallParticleNumericProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemCurveMode.Curve,
          curve1Value = value,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    public void SetParticleValue(string propertyName, FastFloatCurve value1, FastFloatCurve value2)
    {
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleNumericProperty t && prop != null)
      {
        t.curveMode = ParticleSystemCurveMode.TwoCurves;
        t.curve1Value = value1;
        t.curve2Value = value2;
      }
      else
      {
        var newProp = new WaterfallParticleNumericProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemCurveMode.TwoCurves,
          curve1Value = value1,
          curve2Value = value2,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value1, value2);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetParticleValue(string propertyName, Color value)
    {
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleColorProperty t && prop != null)
      {
        t.curveMode = ParticleSystemGradientMode.Color;
        t.constant1Value = value;
      }
      else
      {
        var newProp = new WaterfallParticleColorProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemGradientMode.Color,
          constant1Value = value,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    public void SetParticleValue(string propertyName, Color value1, Color value2)
    {

      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleColorProperty t && prop != null)
      {
        t.curveMode = ParticleSystemGradientMode.TwoColors;
        t.constant1Value = value1;
        t.constant2Value = value2;
      }
      else
      {
        var newProp = new WaterfallParticleColorProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemGradientMode.TwoColors,
          constant1Value = value1,
          constant2Value = value2,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value1, value2);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetParticleValue(string propertyName, Gradient value)
    {
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleColorProperty t && prop != null)
      {
        t.curveMode = ParticleSystemGradientMode.Gradient;
        t.gradient1Value = value;
      }
      else
      {
        var newProp = new WaterfallParticleColorProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemGradientMode.Gradient,
          gradient1Value = value,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value);
      }
    }
    /// <summary>
    /// Sets the Particle's property for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    public void SetParticleValue(string propertyName, Gradient value1, Gradient value2)
    {

      var prop = pProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallParticleColorProperty t && prop != null)
      {
        t.curveMode = ParticleSystemGradientMode.TwoGradients;
        t.gradient1Value = value1;
        t.gradient2Value = value2;
      }
      else
      {
        var newProp = new WaterfallParticleColorProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = ParticleSystemGradientMode.TwoGradients,
          gradient1Value = value1,
          gradient2Value = value2,
        };
        pProperties.Add(newProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemValue(propertyName, particle, value1, value2);
      }
    }
    /// <summary>
    /// Sets the Particle's curve mode for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    public void SetParticleCurveMode(string propertyName, ParticleSystemCurveMode mode)
    {
      float value1;
      float value2;
      FastFloatCurve curve1 = new();
      FastFloatCurve curve2 = new();
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      WaterfallParticleNumericProperty numProp;
      if (prop != null)
      {
        numProp = prop as WaterfallParticleNumericProperty;
        if (numProp != null)
        {
          numProp.curveMode = mode;
        }
      }
      else
      {
        numProp = new WaterfallParticleNumericProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = mode
        };
        pProperties.Add(numProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemMode(propertyName, particle, mode);
      }

      // If the mode changed to a different thing, get the data out of the system to populate the parameter
      if (mode == ParticleSystemCurveMode.Constant)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out value1);
        numProp.constant1Value = value1;
      }
      if (mode == ParticleSystemCurveMode.TwoConstants)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out value1);
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out value2);
        numProp.constant1Value = value1;
        numProp.constant2Value = value2;
      }
      if (mode == ParticleSystemCurveMode.Curve)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], ref curve1);
        numProp.curve1Value = curve1;
      }
      if (mode == ParticleSystemCurveMode.TwoCurves)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], ref curve1);
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], ref curve2);
        numProp.curve1Value = curve1;
        numProp.curve2Value = curve1;
      }
    }
    /// <summary>
    /// Sets the Particle's color mode for serialization and updates attached ParticleSystems
    /// </summary>
    /// <param name="propertyName"></param>
    public void SetParticleColorMode(string propertyName, ParticleSystemGradientMode mode)
    {
      Color color1;
      Color color2;
      Gradient gradient1 = new();
      Gradient gradient2 = new();
      var prop = pProperties.Find(x => x.propertyName == propertyName);
      WaterfallParticleColorProperty colorProp;
      if (prop != null)
      {
        colorProp = prop as WaterfallParticleColorProperty;
        if (colorProp != null)
        {
          colorProp.curveMode = mode;
        }
      }
      else
      {
        colorProp = new WaterfallParticleColorProperty
        {
          propertyName = propertyName,
          moduleEnabled = ParticleUtils.GetParticleModuleState(propertyName, systems[0]),
          curveMode = mode
        };
        pProperties.Add(colorProp);
      }
      foreach (var particle in systems)
      {
        ParticleUtils.SetParticleSystemColorMode(propertyName, particle, mode);
      }

      // If the mode changed to a different thing, get the color out of the system to populate the parameter
      if (mode == ParticleSystemGradientMode.Color)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out color1);
        colorProp.constant1Value = color1;
      }
      if (mode == ParticleSystemGradientMode.TwoColors)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out color1);
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out color2);
        colorProp.constant1Value = color1;
        colorProp.constant2Value = color2;
      }
      if (mode == ParticleSystemGradientMode.Gradient)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out gradient1);
        colorProp.gradient1Value = gradient1;
      }
      if (mode == ParticleSystemGradientMode.TwoGradients)
      {
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out gradient1);
        ParticleUtils.GetParticleSystemValue(propertyName, systems[0], out gradient2);
        colorProp.gradient1Value = gradient1;
        colorProp.gradient2Value = gradient2;
      }
    }
  }

}
