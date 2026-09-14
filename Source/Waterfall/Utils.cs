using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Waterfall
{
  public static class Utils
  {
    /// <summary>
    ///   Log a message with the mod name tag prefixed
    /// </summary>
    /// <param name="str">message string </param>
    [Conditional("DEBUG")]
    public static void Log(string str)
    {
      KSPCommunityLib.Logging.Log.Debug(str);
    }

    /// <summary>
    ///   Log an error with the mod name tag prefixed
    /// </summary>
    /// <param name="str">Error string </param>
    public static void LogError(string str)
    {
      KSPCommunityLib.Logging.Log.Error(str);
    }

    /// <summary>
    ///   Log a warning with the mod name tag prefixed
    /// </summary>
    /// <param name="str">warning string </param>
    public static void LogWarning(string str)
    {
      KSPCommunityLib.Logging.Log.Warning(str);
    }


    public static Gradient CreateGradientFromCurves(FastFloatCurve r, FastFloatCurve g, FastFloatCurve b, FastFloatCurve a, out float lower, out float upper, float timeMergeTolerance = 0.01f)
    {

      // Need to convert real space keys into gradient space keys (0-1)

      float rCurveMax = 1f;
      float rCurveMin = 0f;
      float gCurveMax = 1f;
      float gCurveMin = 0f;
      float bCurveMax = 1f;
      float bCurveMin = 0f;
      float aCurveMax = 1f;
      float aCurveMin = 0f;

      if (r.KeyCount > 0)
      {
        rCurveMax = r[r.KeyCount - 1].time;
        rCurveMin = r[0].time;
      }
      if (g.KeyCount > 0)
      {
        gCurveMax = g[g.KeyCount - 1].time;
        gCurveMin = g[0].time;
      }
      if (b.KeyCount > 0)
      {
        bCurveMax = b[b.KeyCount - 1].time;
        bCurveMin = b[0].time;
      }
      if (a.KeyCount > 0)
      {
        aCurveMax = a[a.KeyCount - 1].time;
        aCurveMin = a[0].time;
      }

      lower = Mathf.Min(rCurveMin, gCurveMin, bCurveMin, aCurveMin);
      upper = Mathf.Max(rCurveMax, gCurveMax, bCurveMax, aCurveMax);

      float offset = 0 - lower;
      float gain = 1f / (upper - offset);

      Gradient grad = new();
      List<GradientColorKey> gradientColorKeys = new List<GradientColorKey>();
      GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[a.KeyCount];

      for (int i = 0; i < r.KeyCount; i++)
      {
        float adjTime = (r[i].time - offset) * gain;
        GradientColorKey newKey = new(
          new Color(r[i].value, g.Evaluate(r[i].time), b.Evaluate(r[i].time)),
          adjTime
          );
        gradientColorKeys.Add(newKey);
      }

      for (int i = 0; i < g.KeyCount; i++)
      {
        float adjTime = (g[i].time - offset) * gain;

        bool skip = false;
        foreach (GradientColorKey checkKey in gradientColorKeys)
        {
          if (Mathf.Abs(adjTime - checkKey.time) < timeMergeTolerance)
            skip = true;
        }

        if (!skip)
        {
          GradientColorKey newKey = new(
            new Color(r.Evaluate(g[i].time), g[i].value, b.Evaluate(g[i].time)),
            adjTime
            );
          gradientColorKeys.Add(newKey);
        }
      }
      for (int i = 0; i < b.KeyCount; i++)
      {
        float adjTime = (b[i].time - offset) * gain;

        bool skip = false;
        foreach (GradientColorKey checkKey in gradientColorKeys)
        {
          if (Mathf.Abs(adjTime - checkKey.time) < timeMergeTolerance)
            skip = true;
        }

        if (!skip)
        {
          GradientColorKey newKey = new(
            new Color(r.Evaluate(b[i].time), g.Evaluate(b[i].time), b[i].value),
            adjTime
            );
          gradientColorKeys.Add(newKey);
        }
      }
      for (int i = 0; i < a.KeyCount; i++)
      {
        gradientAlphaKeys[i] = new(a[i].value, (a[i].time - offset) * gain);
      }
      grad.SetKeys(gradientColorKeys.ToArray(), gradientAlphaKeys);
      return grad;
    }
    public static void CreateCurvesFromGradient(Gradient gradient, out FastFloatCurve r, out FastFloatCurve g, out FastFloatCurve b, out FastFloatCurve a, float lower = 0f, float upper = 1f)
    {
      /// Put 0-1 gradient keys into real space
      float offset = 0 - lower;
      float gain = 1f / (upper - offset);
      r = new();
      g = new();
      b = new();
      a = new();
      foreach (GradientColorKey key in gradient.colorKeys)
      {
        r.Add(key.time * gain + offset, key.color.r);
        g.Add(key.time * gain + offset, key.color.g);
        b.Add(key.time * gain + offset, key.color.b);
      }
      foreach (GradientAlphaKey key in gradient.alphaKeys)
      {
        a.Add(key.time * gain + offset, key.alpha);
      }
    }
    public static ConfigNode SerializeFloatCurve(string name, FastFloatCurve curve)
    {
      ConfigNode node = new();
      curve.Save(node);
      node.name = name;
      return node;
    }
    public static ConfigNode SerializeGradient(string name, Gradient gradient, float lower = 0f, float upper = 1f)
    {
      CreateCurvesFromGradient(gradient, out FastFloatCurve r, out FastFloatCurve g, out FastFloatCurve b, out FastFloatCurve a, lower, upper);

      ConfigNode node = new();
      node.name = name;

      node.AddNode(SerializeFloatCurve("red", r));
      node.AddNode(SerializeFloatCurve("green", g));
      node.AddNode(SerializeFloatCurve("blue", b));
      node.AddNode(SerializeFloatCurve("alpha", a));
      return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ApproximatelyEqual(float a, float b)
    {
      return Math.Abs(a - b) < 1e-4f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref T a, ref T b)
    {
      T temp = a;
      a = b;
      b = temp;
    }
  }

  public static class ConfigNodeParseExtension
  {
    public static bool TryParseVector3(this ConfigNode theNode, string valueName, ref Vector3 result)
    {
      if (!theNode.HasValue(valueName))
        return false;

      result = ConfigNode.ParseVector3(theNode.GetValue(valueName));
      return true;
    }
    public static bool TryParseGradient(this ConfigNode node, ref Gradient result)
    {
      FastFloatCurve rCurve = new();
      FastFloatCurve gCurve = new();
      FastFloatCurve bCurve = new();
      FastFloatCurve aCurve = new();
      ConfigNode curveNode = new();

      bool failed = false;
      if (node.TryGetNode("red", ref curveNode))
      {
        rCurve.Load(curveNode);
      }
      else
      {
        failed = true;
      }
      if (node.TryGetNode("green", ref curveNode))
      {
        gCurve.Load(curveNode);
      }
      else
      {
        failed = true;
      }
      if (node.TryGetNode("blue", ref curveNode))
      {
        bCurve.Load(curveNode);
      }
      else
      {
        failed = true;
      }
      if (node.TryGetNode("alpha", ref curveNode))
      {
        aCurve.Load(curveNode);
      }
      else
      {
        failed = true;
      }
      if (!failed)
      {
        result = Utils.CreateGradientFromCurves(rCurve, gCurve, bCurve, aCurve, out float l, out float u); ;
      }

      return failed;
    }
  }

  public static class TransformDeepChildExtension
  {
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
      var queue = new Queue<Transform>();
      queue.Enqueue(aParent);
      while (queue.Count > 0)
      {
        var c = queue.Dequeue();
        if (c.name == aName)
          return c;
        foreach (Transform t in c)
          queue.Enqueue(t);
      }
      return null;
    }

  }
}