using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Waterfall
{
  /// <summary>
  ///   A controller that pulls specified value from engines module.
  ///   Output is normalized to 0..1 range and smoothing is applied when <see cref="responseRateUp" /> or <see cref="responseRateDown" /> not zero.
  ///   This is pull based alternative to <see cref="CustomPushController" /> which is push based.
  /// </summary>
  /// <example>
  ///   For example, effects from air-breathing engines in AJE mod will be able to take nozzleArea introduced by this mod into account and scale appropriately.
  /// </example>
  [Serializable]
  [DisplayName("Custom (Pull)")]
  public class CustomPullController : WaterfallController
  {
    [Persistent] public string moduleTypeName = "ModuleEngines";
    [Persistent] public string engineIDFieldName = String.Empty; // a field on the module used for disambiguation if more than one exists
    [Persistent] public string engineID   = String.Empty;
    [Persistent] public string memberName = "currentThrottle"; // There is ThrottleController for that, but works as an example
    [Persistent] public string predicateFieldName = String.Empty; // the name of a boolean field on the module which must be true for the value to be pulled

    [Persistent] public float minInputValue;
    [Persistent] public float maxInputValue = 1;

    [Persistent] public float responseRateUp;
    [Persistent] public float responseRateDown;
    private float currentValue;

    private PartModule sourceModule;
    private Func<float>   pullValueMethod = () => 0;
    private Func<PartModule, bool> TestPredicate = (module) => true;
    private Func<PartModule, string> GetEngineID = (module) => String.Empty;

    public CustomPullController() : base() { }

    public CustomPullController(ConfigNode node) : base(node) { }

    public override void Initialize(ModuleWaterfallFX host)
    {
      base.Initialize(host);

      values = new float[1];

      Type moduleType = AssemblyLoader.GetClassByName(typeof(PartModule), moduleTypeName);

      if (moduleType == null)
      {
        Utils.LogError($"[{nameof(CustomPullController)}]: Could not find part module type {moduleTypeName}, effect controller will not do anything");
        return;
      }

      // set default field names for engines, for backwards compatibility
      if (moduleTypeName == "ModuleEngines")
      {
        if (engineIDFieldName == String.Empty)
          engineIDFieldName = "engineID";
        if (predicateFieldName == string.Empty)
          engineIDFieldName = "isOperational";
      }

      if (!String.IsNullOrEmpty(engineIDFieldName))
      {
        FieldInfo engineIDField = moduleType.GetField(engineIDFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (engineIDField == null && !String.IsNullOrEmpty(engineID))
           Utils.LogError($"[{nameof(CustomPullController)}]: Could not find field {engineIDFieldName} on module type {moduleTypeName}");
        else if (engineIDField.FieldType != typeof(string))
        {
          Utils.LogError($"[{nameof(CustomPullController)}]: Field {engineIDFieldName} on module type {moduleTypeName} is not of type string");
          engineIDField = null;
        }

        if (engineIDField != null)
          GetEngineID = (module) => (string)engineIDField.GetValue(module);
      }
      if (!String.IsNullOrEmpty(predicateFieldName))
      {
        FieldInfo predicateField = moduleType.GetField(predicateFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (predicateField == null)
          Utils.LogError($"[{nameof(CustomPullController)}]: Could not find field predicateFieldName {predicateFieldName} on module type {moduleTypeName}.  You may need to set this to an empty string if you're not using it.");
        else if (predicateField.FieldType != typeof(bool))
        {
          Utils.LogError($"[{nameof(CustomPullController)}]: Field {predicateFieldName} on module type {moduleTypeName} is not of type bool");
          predicateField = null;
        }

        if (predicateField != null)
          TestPredicate = (module) => (bool)predicateField.GetValue(module);
      }

      UnityEngine.Component[] possibleModules = host.GetComponents(moduleType);
      sourceModule = possibleModules.FirstOrDefault(c => GetEngineID(c as PartModule) == engineID) as PartModule;

      if (sourceModule == null && possibleModules.Length > 0)
      {
        if (possibleModules.Length > 0)
        {
          Utils.Log($"[{nameof(CustomPullController)}]: Could not find engine ID {engineID}, using first module", LogType.Effects);
          sourceModule = possibleModules[0] as PartModule;
        }
        else
        {
          Utils.LogError($"[{nameof(CustomPullController)}]: Could not find any {moduleTypeName} to use with {nameof(CustomPullController)} named {name}, effect controller will not do anything");
          return;
        }

      }

      pullValueMethod = FindSuitableMemberOnEnginesModule();
    }

    private Func<float> FindSuitableMemberOnEnginesModule()
    {
      const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

      var methodInfo = sourceModule.GetType()
        .GetMethods(bindingFlags)
        .FirstOrDefault(m => m.Name == memberName && m.GetParameters().Length == 0);

      if (methodInfo != null)
        return () => Convert.ToSingle(methodInfo.Invoke(sourceModule, new object[0]));

      var propertyInfo = sourceModule.GetType()
        .GetProperty(memberName, bindingFlags);

      if (propertyInfo != null)
        return () => (float)propertyInfo.GetValue(sourceModule);

      var fieldInfo = sourceModule.GetType()
        .GetField(memberName, bindingFlags);

      if (fieldInfo != null)
        return () => Convert.ToSingle(fieldInfo.GetValue(sourceModule));

      Utils.LogError($"[{nameof(CustomPullController)}]: Could not find any public instance method, property or field named {memberName} to use with {nameof(CustomPullController)} named {name}, effect controller will not do anything");
      return () => 0;
    }

    protected override float UpdateSingleValue()
    {
      float newValue = Mathf.InverseLerp(minInputValue, maxInputValue, GetValue());
      float responseRate = newValue > currentValue
        ? responseRateUp
        : responseRateDown;

      currentValue = responseRate > 0
        ? Mathf.MoveTowards(currentValue, newValue, responseRate * TimeWarp.deltaTime)
        : newValue;

      return currentValue;
    }

    private float GetValue()
    {
      if (sourceModule == null)
        return 0;

      if (!TestPredicate(sourceModule))
        return 0;

      engineID = GetEngineID(sourceModule); // Make sure that engineID is in-sync with actually used module

      try
      {
        return pullValueMethod.Invoke();
      }
      catch (Exception ex)
      {
        Utils.LogError($"[{nameof(CustomPullController)}]: Error while getting value of specified member {memberName}: {ex.Message}");
        return 0;
      }
    }

    public override void UpgradeToCurrentVersion(Version loadedVersion)
    {
      base.UpgradeToCurrentVersion(loadedVersion);

      if (loadedVersion < Version.FixedRampRates)
      {
        responseRateDown *= Math.Max(1, referencingModifierCount);
        responseRateUp *= Math.Max(1, referencingModifierCount);
      }
    }
  }
}
