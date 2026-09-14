using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waterfall
{
  /// <summary>
  /// DirectModifier is a base class for effect modifiers that apply their effect directly to the target without using an integrator
  /// </summary>
  public abstract class DirectModifier : EffectModifier
  {
    protected DirectModifier() { }

    protected DirectModifier(ConfigNode node) : base(node) { }

    /// <summary>
    /// Applies the effect to the target.  Implementers should call UpdateRandomValues() and consume the stored random value
    /// </summary>
    /// <param name="strength"></param>
    public abstract void Apply(float[] strength);

    public override EffectIntegrator CreateIntegrator()
    {
      // hmm, perhaps there should be a different base class for modifiers that use integrators?
      Utils.LogError($"DirectModifier.CreateIntegrator() called but this has no corresponding integrator!");
      return null;
    }
  }
}
