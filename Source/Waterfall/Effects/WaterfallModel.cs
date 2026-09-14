using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Waterfall
{
  public class WaterfallModel
  {

    public string path;
    public string asset;
    public string positionOffsetString;
    public string rotationOffestString;
    public string scaleOffsetString;


    public string overrideShader = "";
    public List<WaterfallMaterial> materials;
    public List<WaterfallLight> lights;
    public List<WaterfallParticle> particles;


    public List<Transform> modelTransforms;
    public Vector3 modelPositionOffset = Vector3.zero;
    public Vector3 modelRotationOffset = Vector3.zero;
    public Vector3 modelScaleOffset = Vector3.one;

    protected bool randomized = true;

    public WaterfallModel()
    {
      modelTransforms = new();
    }

    public WaterfallModel(string modelPath, string shader, bool randomizeSeed)
    {
      modelTransforms = new();
      path = modelPath;
      overrideShader = shader;
      randomized = randomizeSeed;
    }

    public WaterfallModel(WaterfallAsset modelAsset, WaterfallAsset shaderAsset, WaterfallAsset particleAsset, bool randomizeSeed)
    {

      modelTransforms = new();
      path = modelAsset.Path;
      if (particleAsset != null)
      {
        asset = particleAsset.Asset;
      }

      if (shaderAsset == null)
      {
        overrideShader = null;
      }
      else
      {
        overrideShader = shaderAsset.Name;
      }
      randomized = randomizeSeed;
    }

    public WaterfallModel(ConfigNode node) : this()
    {
      Load(node);
    }

    public void Load(ConfigNode node)
    {
      if (!node.TryGetValue("path", ref path))
        Utils.LogError("[WaterfallModel]: Unabled to find required path string in MODEL node");
      node.TryGetValue("positionOffset", ref modelPositionOffset);
      node.TryGetValue("rotationOffset", ref modelRotationOffset);
      node.TryGetValue("scaleOffset", ref modelScaleOffset);
      node.TryGetValue("asset", ref asset);

      materials = new();
      foreach (var materialNode in node.GetNodes(WaterfallConstants.MaterialNodeName))
      {
        materials.Add(new(materialNode));
      }

      lights = new();
      foreach (var lightNode in node.GetNodes(WaterfallConstants.LightNodeName))
      {
        lights.Add(new(lightNode));
      }

      particles = new List<WaterfallParticle>();
      foreach (ConfigNode pNode in node.GetNodes(WaterfallConstants.ParticleNodeName))
      {
        particles.Add(new WaterfallParticle(pNode));
      }
    }

    public ConfigNode Save()
    {
      var node = new ConfigNode();
      node.name = WaterfallConstants.ModelNodeName;
      node.AddValue("path", path);
      if (asset != null)
      {
        node.AddValue("asset", asset);
      }

      node.AddValue("positionOffset", modelPositionOffset);
      node.AddValue("rotationOffset", modelRotationOffset);
      node.AddValue("scaleOffset", modelScaleOffset);

      foreach (var m in materials)
      {
        node.AddNode(m.Save());
      }
      foreach (var l in lights)
      {
        node.AddNode(l.Save());
      }
      foreach (var p in particles)
      {
        node.AddNode(p.Save());
      }
      return node;
    }

    public void Cleanup()
    {
      for (int i = modelTransforms.Count - 1; i >= 0; i--)
      {
        Object.Destroy(modelTransforms[i].gameObject);
      }
      modelTransforms.Clear();
    }

    public void Initialize(Transform parent, bool fromNothing)
    {
      Utils.Log(String.Format("[WaterfallModel]: Instantiating model from {0} ", path));
      if (!GameDatabase.Instance.ExistsModel(path))
        Utils.LogError(String.Format("[WaterfallModel]: Unabled to find model {0} in GameDatabase", path));
      var inst = Object.Instantiate(GameDatabase.Instance.GetModelPrefab(path), parent.position, parent.rotation);
      inst.SetLayerRecursive(1);
      inst.SetActive(true);

      var modelTransform = inst.GetComponent<Transform>();
      modelTransform.SetParent(parent, true);
      modelTransforms.Add(modelTransform);

      if (asset != null && asset != "")
      {
        Utils.Log($"[WaterfallModel]: Instantiating particle asset from{asset} ");
        GameObject go = GameObject.Instantiate(WaterfallParticleLoader.GetParticles(asset),
          Vector3.zero, Quaternion.identity) as GameObject;

        go.transform.SetParent(modelTransform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
      }

      var renderers = modelTransform.GetComponentsInChildren<Renderer>();
      var lightObjs = modelTransform.GetComponentsInChildren<Light>();
      var particleSystems = modelTransform.GetComponentsInChildren<ParticleSystem>();

      if (fromNothing)
      {
        materials = new();
        lights = new();
        particles = new();


        if (lightObjs.Length > 0)
        {
          var l = new WaterfallLight();
          l.lights = new();
          l.transformName = lightObjs[0].name;
          lights.Add(l);
        }

        foreach (var r in renderers)
        {
          var m = new WaterfallMaterial();
          if (overrideShader != null)
            m.shaderName = overrideShader;
          m.useAutoRandomization = randomized;
          m.materials = new();
          m.transformName = r.transform.name;
          materials.Add(m);
        }
        foreach (var p in particleSystems)
        {
          WaterfallParticle wfP = new WaterfallParticle();
          wfP.transformName = p.transform.name;
          wfP.systems = new();
          particles.Add(wfP);
        }
      }

      foreach (var m in materials)
      {
        m.useAutoRandomization = randomized;
        m.Initialize(modelTransform);
      }
      foreach (var l in lights)
      {
        l.Initialize(modelTransform);
      }
      foreach (var p in particles)
      {
        Utils.Log(String.Format("[WaterfallModel]: Initializing system {0} ", p.transformName));
        p.Initialize(modelTransform);
      }

      ApplyOffsets(modelPositionOffset, modelRotationOffset, modelScaleOffset);
    }

    public void Update()
    {
      foreach (var m in materials)
      {
        m.Update();
      }
    }
    public void ResetParticleSystem(bool playImmediately)
    {
      foreach (var particleSystem in particles)
      {
        particleSystem.Reset(playImmediately);
      }
    }
    public void ApplyOffsets(Vector3 position, Vector3 rotation, Vector3 scale)
    {
      modelPositionOffset = position;
      modelRotationOffset = rotation;
      modelScaleOffset = scale;

      Utils.Log($"[WaterfallModel] Applying model offsets {position}, {rotation}, {scale}");

      positionOffsetString = $"{position.x}, {position.y}, {position.z}";
      rotationOffestString = $"{rotation.x}, {rotation.y}, {rotation.z}";
      scaleOffsetString = $"{scale.x}, {scale.y}, {scale.z}";

      foreach (var modelTransform in modelTransforms)
      {
        modelTransform.localPosition = modelPositionOffset;
        modelTransform.localScale = modelScaleOffset;

        if (modelRotationOffset == Vector3.zero)
        {
          modelTransform.localRotation = Quaternion.identity;
        }
        else
        {
          modelTransform.localEulerAngles = modelRotationOffset;
        }
      }
    }

    public void SetEnabled(bool state)
    {
      foreach (var modelTransform in modelTransforms)
      {
        var skinRenderers = modelTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var renderer in skinRenderers)
        {
          renderer.enabled = state;
        }
        var renderers = modelTransform.GetComponentsInChildren<Renderer>();

      }
    }

    public void SetTexture(WaterfallMaterial targetMat, string propertyName, string value)
    {
      foreach (var m in materials)
      {
        if (m == targetMat)
        {
          m.SetTexture(propertyName, value);
          if (modelTransforms.Count > 1)
          {
            foreach (var t in modelTransforms)
            {
              foreach (var r in t.GetComponentsInChildren<Renderer>())
              {
                if (r.name == m.transformName)
                {
                  r.material.SetTexture(propertyName, GameDatabase.Instance.GetTexture(value, false));
                }
              }
            }
          }
        }
      }
    }

    public void SetFloat(WaterfallMaterial targetMat, string propertyName, float value)
    {
      foreach (var m in materials)
      {
        if (m == targetMat)
        {
          m.SetFloat(propertyName, value);

          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var r in t.GetComponentsInChildren<Renderer>())
              {
                if (r.name == m.transformName)
                {
                  if (propertyName == "_Seed" && m.useAutoRandomization) { }
                  else
                  {
                    r.material.SetFloat(propertyName, value);
                  }
                }
              }
            }
        }
      }
    }

    public void SetVector4(WaterfallMaterial targetMat, string propertyName, Vector4 value)
    {
      foreach (var m in materials)
      {
        if (m == targetMat)
        {
          m.SetVector4(propertyName, value);
          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var r in t.GetComponentsInChildren<Renderer>())
              {
                if (r.name == m.transformName)
                {
                  r.material.SetVector(propertyName, value);
                }
              }
            }
        }
      }
    }

    public void SetTextureScale(WaterfallMaterial targetMat, string propertyName, Vector2 value)
    {
      foreach (var m in materials)
      {
        if (m == targetMat)
        {
          m.SetTextureScale(propertyName, value);
          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var r in t.GetComponentsInChildren<Renderer>())
              {
                if (r.name == m.transformName)
                {
                  r.material.SetTextureScale(propertyName, value);
                }
              }
            }
        }
      }
    }

    public void SetTextureOffset(WaterfallMaterial targetMat, string propertyName, Vector2 value)
    {
      foreach (var m in materials)
      {
        if (m == targetMat)
        {
          m.SetTextureOffset(propertyName, value);
          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var r in t.GetComponentsInChildren<Renderer>())
              {
                if (r.name == m.transformName)
                {
                  r.material.SetTextureOffset(propertyName, value);
                }
              }
            }
        }
      }
    }

    public void SetColor(WaterfallMaterial targetMat, string propertyName, Color value)
    {
      foreach (var m in materials)
      {
        if (m == targetMat)
        {
          m.SetColor(propertyName, value);

          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var r in t.GetComponentsInChildren<Renderer>())
              {
                if (r.name == m.transformName)
                {
                  r.material.SetColor(propertyName, value);
                }
              }
            }
        }
      }
    }

    public void SetLightRange(WaterfallLight targetLight, float value)
    {
      foreach (var l in lights)
      {
        if (l == targetLight)
        {
          l.SetRange(value);

          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var li in t.GetComponentsInChildren<Light>())
              {
                li.range = value;
              }
            }
        }
      }
    }

    public void SetLightIntensity(WaterfallLight targetLight, float value)
    {
      foreach (var l in lights)
      {
        if (l == targetLight)
        {
          l.SetIntensity(value);

          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var li in t.GetComponentsInChildren<Light>())
              {
                li.intensity = value;
              }
            }
        }
      }
    }

    public void SetLightAngle(WaterfallLight targetLight, float value)
    {
      foreach (var l in lights)
      {
        if (l == targetLight)
        {
          l.SetAngle(value);

          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var li in t.GetComponentsInChildren<Light>())
              {
                li.spotAngle = value;
              }
            }
        }
      }
    }

    public void SetLightColor(WaterfallLight targetLight, Color value)
    {
      foreach (var l in lights)
      {
        if (l == targetLight)
        {
          l.SetColor(value);

          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var li in t.GetComponentsInChildren<Light>())
              {
                li.color = value;
              }
            }
        }
      }
    }

    public void SetLightType(WaterfallLight targetLight, LightType value)
    {
      foreach (var l in lights)
      {
        if (l == targetLight)
        {
          l.SetLightType(value);

          if (modelTransforms.Count > 1)
            foreach (var t in modelTransforms)
            {
              foreach (var li in t.GetComponentsInChildren<Light>())
              {
                li.type = value;
              }
            }
        }
      }
    }

  }
}