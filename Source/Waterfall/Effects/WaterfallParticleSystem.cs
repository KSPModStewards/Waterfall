using UnityEngine;

namespace Waterfall
{
  /// <summary>
  /// TODO: Reimplement this
  /// </summary>
  public class WaterfallParticleSystem
  {
    private Transform transform;
    private Transform parent;
    public ParticleSystem emitter;

    private ParticleSystemRenderer renderer;

    private ParticleSystem.Particle[] particleBuffer;
    private ParticleSystem.MainModule particleMain;
    private ParticleSystem.EmissionModule particleEmit;
    private ParticleSystem.ShapeModule particleShape;

    public WaterfallParticleSystem(ParticleSystem source)
    {
      transform = source.transform;
      parent = transform.parent;

      emitter = source;
      renderer = source.GetComponent<ParticleSystemRenderer>();


      if (emitter)
      {
        particleMain = emitter.main;
        particleEmit = emitter.emission;
        particleShape = emitter.shape;

        Utils.Log($"[WaterfallParticleSystem]: Set up emitter {emitter} on {transform.name}. \n" +
          $"emit: {particleEmit} \n" +
          $"shape {particleShape} \n" +
          $"main {particleMain}");
      }
    }


    public void Update()
    {
      if (emitter != null)
      {
        if (FlightGlobals.ActiveVessel != null && (Krakensbane.GetFrameVelocity().magnitude > 0f))
        {
          Vector3d frameVel = Krakensbane.GetFrameVelocity();
          particleBuffer = new ParticleSystem.Particle[emitter.particleCount];

          int particleCount = emitter.GetParticles(particleBuffer);
          int pc = particleCount;

          float distancePerFrame = (float)frameVel.magnitude * TimeWarp.deltaTime;
          Vector3 nrmVelocity = -frameVel.normalized;
          while (particleCount > 0)
          {
            particleBuffer[particleCount - 1].position =
              particleBuffer[particleCount - 1].position +
              (-frameVel * TimeWarp.deltaTime) -
              UnityEngine.Random.Range(0f, distancePerFrame) * nrmVelocity;
            particleCount--;

          }
          emitter.SetParticles(particleBuffer, pc);
        }
      }
    }
  }
}
