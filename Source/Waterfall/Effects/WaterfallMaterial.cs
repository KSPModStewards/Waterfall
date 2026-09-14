using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Waterfall
{
  /// <summary>
  ///   Defines a material managed by Waterfall
  /// </summary>
  public class WaterfallSkinnedMesh
  {
    public SkinnedMeshRenderer SMR;
    public Mesh                SkinedMesh;

    public WaterfallSkinnedMesh(SkinnedMeshRenderer skin, MeshFilter filter)
    {
      SMR        = skin;
      SkinedMesh = skin.sharedMesh;
    }

    public void Recalculate()
    {
      //SMR.BakeMesh(SkinedMesh);
      //if (SkinedMesh)
      //{
      //  SkinedMesh.RecalculateNormals();
      //}
    }
  }

  public class WaterfallMaterial
  {
    public string                          shaderName;
    public string                          transformName        = "";
    public string                          baseTransformName    = "";
    public bool                            useAutoRandomization = true;
    public List<WaterfallMaterialProperty> matProperties;

    public    List<Material>             materials;
    protected Renderer                   targetMeshRenderer;
    protected List<WaterfallSkinnedMesh> skinnedMeshes;
    protected List<MeshFilter>           targetFilter;
    protected List<Mesh>                 bakedMesh;

    public WaterfallMaterial()
    {
      matProperties = new();
    }

    public WaterfallMaterial(ConfigNode node)
    {
      Load(node);
    }

    /// <summary>
    ///   Load from a confignode
    /// </summary>
    /// <param name="node"></param>
    public void Load(ConfigNode node)
    {
      node.TryGetValue("transform",     ref transformName);
      node.TryGetValue("baseTransform", ref baseTransformName);
      node.TryGetValue("shader",        ref shaderName);
      node.TryGetValue("randomizeSeed", ref useAutoRandomization);

      materials = new();
      Utils.Log(String.Format("[WaterfallMaterial]: Loading new material for {0} ", transformName));

      matProperties = new();
      foreach (var subnode in node.GetNodes(WaterfallConstants.TextureNodeName))
      {
        matProperties.Add(new WaterfallMaterialTextureProperty(subnode));
      }

      foreach (var subnode in node.GetNodes(WaterfallConstants.ColorNodeName))
      {
        matProperties.Add(new WaterfallMaterialColorProperty(subnode));
      }

      foreach (var subnode in node.GetNodes(WaterfallConstants.FloatNodeName))
      {
        matProperties.Add(new WaterfallMaterialFloatProperty(subnode));
      }
    }

    /// <summary>
    ///   Save to a config node
    /// </summary>
    /// <returns></returns>
    public ConfigNode Save()
    {
      var node = new ConfigNode();
      node.name = WaterfallConstants.MaterialNodeName;

      if (baseTransformName != "")
        node.AddValue("baseTransform", baseTransformName);

      if (transformName != "")
        node.AddValue("transform", transformName);

      node.AddValue("shader",        shaderName);
      node.AddValue("randomizeSeed", useAutoRandomization);

      foreach (var p in matProperties)
      {
        node.AddNode(p.Save());
      }

      return node;
    }

    /// <summary>
    ///   Initialize the material by finding its target transform, the applying shader properties loaded in configuration to it
    /// </summary>
    /// <param name="parentTransform"></param>
    public void Initialize(Transform parentTransform)
    {
      materials     = new();
      skinnedMeshes = new();
      if (shaderName != null)
      {
        if (baseTransformName != "")
        {
          var candidates = parentTransform.GetComponentsInChildren<Transform>();
          foreach (var t in candidates)
          {
            var r = t.GetComponent<Renderer>();
            if (r != null && r.material != null)
            {
              if (t.GetComponent<SkinnedMeshRenderer>() != null)
              {
                skinnedMeshes.Add(new(t.GetComponent<SkinnedMeshRenderer>(),
                                      t.GetComponent<MeshFilter>()));
              }

              Utils.Log($"Added rendered material from {t.name}");
              materials.Add(r.material);
            }
          }
        }
        else
        {
          var materialTarget = parentTransform.FindDeepChild(transformName);
          targetMeshRenderer = materialTarget.GetComponent<Renderer>();
          if (materialTarget.GetComponent<SkinnedMeshRenderer>() != null)
          {
            skinnedMeshes.Add(new(materialTarget.GetComponent<SkinnedMeshRenderer>(),
                                  materialTarget.GetComponent<MeshFilter>()));
          }

          materials.Add(targetMeshRenderer.material);
        }


        foreach (var mat in materials)
        {
          mat.shader = ShaderLoader.GetShader(shaderName);


          foreach (var p in matProperties)
          {
            p.Initialize(mat);
          }

          if (useAutoRandomization && mat.HasProperty(ShaderPropertyID._Seed))
          {
            mat.SetFloat(ShaderPropertyID._Seed, Random.Range(-1f, 1f));
          }

          Utils.Log(String.Format("[WaterfallMaterial]: Assigned new shader {0} ", mat.shader));
        }
      }
    }

    public void Update()
    {
      if (skinnedMeshes != null)
      {
        foreach (var smr in skinnedMeshes)
        {
          smr.Recalculate();
        }
      }
    }

    /// <summary>
    ///   Sets a shader float on this material. If it doesn't exist as a material property object, create it.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetFloat(string propertyName, float value)
    {
      var prop = matProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallMaterialFloatProperty t && prop != null)
        t.propertyValue = value;
      else
      {
        var newProp = new WaterfallMaterialFloatProperty
        {
          propertyName = propertyName,
          propertyValue = value
        };
        matProperties.Add(newProp);
      }

      foreach (var mat in materials)
      {
        if (propertyName == "_Seed" && useAutoRandomization) { }
        else
        {
          mat.SetFloat(propertyName, value);
        }
      }
    }

    /// <summary>
    ///   Sets a shader Vec4 on this material. If it doesn't exist as a material property object, create it.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetVector4(string propertyName, Vector4 value)
    {
      var prop = matProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallMaterialVector4Property t && prop != null)
        t.propertyValue = value;
      else
      {
        var newProp = new WaterfallMaterialVector4Property
        {
          propertyName = propertyName,
          propertyValue = value
        };
        matProperties.Add(newProp);
      }

      foreach (var mat in materials)
      {
        mat.SetFloatArray(propertyName, new[] { value.x, value.y, value.z, value.w });
      }
    }


    /// <summary>
    ///   Sets the texture scale for this material. If it doesn't exist as a material property object, create it.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetTextureScale(string propertyName, Vector2 value)
    {
      var prop = matProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallMaterialTextureProperty t && prop != null)
        t.textureScale = value;
      else
      {
        var newProp = new WaterfallMaterialTextureProperty
        {
          propertyName = propertyName,
          textureOffset = materials[0].GetTextureOffset(propertyName),
          textureScale = value
        };
        matProperties.Add(newProp);
      }

      foreach (var mat in materials)
      {
        mat.SetTextureScale(propertyName, value);
      }
    }

    /// <summary>
    ///   Sets the texture for this material. If it doesn't exist as a material property object, create it.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetTexture(string propertyName, string value)
    {
      Utils.Log($"[WaterfallMaterial] Changing {propertyName} to {value}");
      var prop = matProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallMaterialTextureProperty t && prop != null)
        t.texturePath = value;
      else
      {
        var newProp = new WaterfallMaterialTextureProperty
        {
          propertyName = propertyName,
          texturePath = value,
          textureScale = materials[0].GetTextureScale(propertyName),
          textureOffset = materials[0].GetTextureOffset(propertyName)
        };
        matProperties.Add(newProp);
      }

      foreach (var mat in materials)
      {
        Utils.Log($"[WaterfallMaterial] Changing {propertyName} to {value} on {mat}");
        mat.SetTexture(propertyName, GameDatabase.Instance.GetTexture(value, false));
      }
    }

    /// <summary>
    ///   Sets the texture offset for this material. If it doesn't exist as a material property object, create it.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetTextureOffset(string propertyName, Vector2 value)
    {
      var prop = matProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallMaterialTextureProperty t && prop != null)
        t.textureOffset = value;
      else
      {
        var newProp = new WaterfallMaterialTextureProperty
        {
          propertyName = propertyName,
          textureScale = materials[0].GetTextureScale(propertyName),
          textureOffset = value
        };
        matProperties.Add(newProp);
      }

      foreach (var mat in materials)
      {
        mat.SetTextureOffset(propertyName, value);
      }
    }

    /// <summary>
    ///   Sets the material colour. If it doesn't exist as a material property object, create it.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetColor(string propertyName, Color value)
    {
      var prop = matProperties.Find(x => x.propertyName == propertyName);
      if (prop is WaterfallMaterialColorProperty t && prop != null)
        t.propertyValue = value;
      else
      {
        var newProp = new WaterfallMaterialColorProperty
        {
          propertyName = propertyName,
          propertyValue = value
        };
        matProperties.Add(newProp);
      }

      foreach (var mat in materials)
      {
        mat.SetColor(propertyName, value);
      }
    }
  }
}
