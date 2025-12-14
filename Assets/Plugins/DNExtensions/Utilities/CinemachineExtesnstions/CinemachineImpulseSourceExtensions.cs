using System;
using UnityEngine;
using Unity.Cinemachine;


namespace DNExtensions.CinemachineImpulseSystem
{
    public static class CinemachineImpulseSourceExtensions
    {
        public static void GenerateImpulse(this CinemachineImpulseSource source, ImpulseSettings settings)
        {
            if (!source || settings == null) return;

            var impulseDefinition = new CinemachineImpulseDefinition
            {
                ImpulseChannel = settings.impulseChannel,
                ImpulseType = settings.impulseType,
                ImpulseShape = settings.impulseShape,
                ImpulseDuration = settings.duration,
                DissipationDistance = settings.dissipationDistance,
                DissipationRate = settings.dissipationRate,
                PropagationSpeed = settings.propagationSpeed
            };
            
            if (settings.impulseShape == CinemachineImpulseDefinition.ImpulseShapes.Custom)
            {
                impulseDefinition.CustomImpulseShape = settings.customImpulseShape;
            }

            Vector3 velocity = settings.velocity * settings.intensity;
            impulseDefinition.CreateEvent(source.transform.position, velocity);
        }
        
        
        public static void GenerateImpulseWithIntensity(this CinemachineImpulseSource source, ImpulseSettings settings, float intensity)
        {
            if (!source || settings == null) return;

            var impulseDefinition = new CinemachineImpulseDefinition
            {
                ImpulseChannel = settings.impulseChannel,
                ImpulseType = settings.impulseType,
                ImpulseShape = settings.impulseShape,
                ImpulseDuration = settings.duration,
                DissipationDistance = settings.dissipationDistance,
                DissipationRate = settings.dissipationRate,
                PropagationSpeed = settings.propagationSpeed
            };
            
            if (settings.impulseShape == CinemachineImpulseDefinition.ImpulseShapes.Custom)
            {
                impulseDefinition.CustomImpulseShape = settings.customImpulseShape;
            }

            Vector3 velocity = settings.velocity * intensity;
            impulseDefinition.CreateEvent(source.transform.position, velocity);
        }
    }

    [Serializable]
    public class ImpulseSettings
    {
        public int impulseChannel = 1;
        public CinemachineImpulseDefinition.ImpulseTypes impulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        public CinemachineImpulseDefinition.ImpulseShapes impulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        [Min(0.01f)] public float duration = 0.3f;
        [Min(0.1f)] public float intensity = 1f;
        public Vector3 velocity = Vector3.one;

        [Header("Spatial (Dissipating/Propagating)")]
        [Min(0f)] public float dissipationDistance = 100f;
        [Range(0f, 1f)] public float dissipationRate = 0.25f;
        [Min(1f)] public float propagationSpeed = 343f;

        [Header("Custom Shape")]
        public AnimationCurve customImpulseShape = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public ImpulseSettings() { }

        public ImpulseSettings(
            CinemachineImpulseDefinition.ImpulseShapes shape,
            float duration,
            float intensity,
            Vector3 velocity)
        {
            this.impulseShape = shape;
            this.duration = duration;
            this.intensity = intensity;
            this.velocity = velocity;
        }
    }
    

}