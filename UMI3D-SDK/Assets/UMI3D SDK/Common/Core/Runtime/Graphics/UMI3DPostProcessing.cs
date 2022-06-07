/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

#if USING_URP || UNITY_POST_PROCESSING 

using UnityEngine;

#if USING_URP
using UnityEngine.Rendering.Universal;
#else
using UnityEngine.Rendering.PostProcessing;
#endif

namespace umi3d.common.graphics
{
    public class UMI3DPostProcessing : UMI3DAbstractPostProcessing
    {
        public Camera MainCamera;

        protected override Camera _GetCamera()
        {
            return MainCamera ?? base._GetCamera();
        }

#if USING_URP
        public UnityEngine.Rendering.Volume volume;
#else
        public PostProcessVolume m_Volume;
        public Vignette m_Vignette;
        public Bloom m_Bloom;
#endif

        protected override void Awake()
        {
            base.Awake();
#if USING_URP
            Debug.Assert(volume != null);
#else
            Debug.Assert(m_Volume != null);
#endif
        }

        protected override void _ResetBloom()
        {
#if USING_URP
            if (volume != null)
            {
                if (volume.profile.TryGet(out Bloom oldm))
                    volume.profile.Remove<Bloom>();
            }
#else
            // Create an instance of a vignette
            Bloom defaultBloom = ScriptableObject.CreateInstance<Bloom>();

            defaultBloom.enabled.Override(false);
            defaultBloom.intensity.Override(0f);
            defaultBloom.threshold.Override(1f);
            defaultBloom.softKnee.Override(0.5f);
            defaultBloom.clamp.Override(65472f);
            defaultBloom.diffusion.Override(7f);
            defaultBloom.anamorphicRatio.Override(0f);
            defaultBloom.color.Override(Color.white);
            defaultBloom.fastMode.Override(false);
            defaultBloom.dirtTexture.Override(null);
            defaultBloom.dirtIntensity.Override(0f);

            // Use the QuickVolume method to create a volume with a priority of 100, and assign the vignette to this volume
            PostProcessVolume volume = GetCamera().GetComponent<PostProcessVolume>();
            if (volume != null)
            {
                if (volume.sharedProfile.TryGetSettings(out Bloom oldm))
                    volume.sharedProfile.RemoveSettings<Bloom>();
                volume.sharedProfile.AddSettings(defaultBloom);
            }
#endif
        }

        protected override void _ResetFog()
        {
            RenderSettings.fog = false;
            RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.5f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.01f;
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 300f;
        }

        protected override void _ResetVignette()
        {
#if USING_URP
            if (volume != null)
            {
                if (volume.profile.TryGet(out Vignette oldv))
                    volume.profile.Remove<Vignette>();
            }
#else
            // Create an instance of a vignette

            Vignette vignette = ScriptableObject.CreateInstance<Vignette>();
            vignette.enabled.Override(false);
            vignette.mode.Override(VignetteMode.Classic);
            vignette.color.Override(Color.black);
            vignette.center.Override(new Vector2(0.5f, 0.5f));
            vignette.intensity.Override(0f);
            vignette.smoothness.Override(0f);
            vignette.roundness.Override(0f);
            vignette.rounded.Override(false);

            PostProcessVolume volume = GetCamera().GetComponent<PostProcessVolume>();
            if (volume != null)
            {
                if (volume.sharedProfile.TryGetSettings(out Vignette oldv))
                    volume.sharedProfile.RemoveSettings<Vignette>();
                volume.sharedProfile.AddSettings(vignette);
            }
#endif
        }

        protected override void _SetBloom(UMI3DGlobalBloom bloom)
        {
#if USING_URP
            if (!volume.profile.TryGet(out Bloom bloomComponent))
                bloomComponent = volume.profile.Add<Bloom>(true);

            bloomComponent.active = bloom.BloomEnabled;
            bloomComponent.intensity.value = bloom.Intensity;
            bloomComponent.threshold.value = bloom.Threshold;
            bloomComponent.scatter.value = bloom.Scatter;
            bloomComponent.tint.value = bloom.Tint;
            bloomComponent.clamp.value = bloom.Clamp;
            bloomComponent.highQualityFiltering.value = bloom.HighQualityFiltering;
#if UNITY_2020
            bloomComponent.skipIterations.value = bloom.SkipIterations;
#endif
            bloomComponent.dirtTexture.value = bloom.Dirt_Texture;
            bloomComponent.dirtIntensity.value = bloom.Dirt_Intensity;
#else
            // Create an instance of a vignette
            m_Bloom = ScriptableObject.CreateInstance<Bloom>();

            m_Bloom.enabled.Override(bloom.BloomEnabled);
            m_Bloom.intensity.Override(bloom.Intensity);
            m_Bloom.threshold.Override(bloom.Threshold);
            m_Bloom.softKnee.Override(bloom.SoftKnee);
            m_Bloom.clamp.Override(bloom.Clamp);
            m_Bloom.diffusion.Override(bloom.Diffusion);
            m_Bloom.anamorphicRatio.Override(bloom.AnamorphicRatio);
            m_Bloom.color.Override(bloom.Tint);
            m_Bloom.fastMode.Override(bloom.FastMode);
            m_Bloom.dirtTexture.Override(bloom.Dirt_Texture);
            m_Bloom.dirtIntensity.Override(bloom.Dirt_Intensity);

            if (m_Volume == null)
                m_Volume = GetCamera().GetComponent<PostProcessVolume>();
            if (m_Volume.sharedProfile.TryGetSettings(out Bloom oldm))
                m_Volume.sharedProfile.RemoveSettings<Bloom>();
            m_Volume.sharedProfile.AddSettings(m_Bloom);
#endif
        }

        protected override void _SetFog(UMI3DFogSettings fogSettings)
        {
            RenderSettings.fog = fogSettings.FogEnabled;
            RenderSettings.fogColor = fogSettings.Color;
            RenderSettings.fogMode = Convert(fogSettings.Mode);
            RenderSettings.fogDensity = fogSettings.Density;
            RenderSettings.fogStartDistance = fogSettings.StartDistance;
            RenderSettings.fogEndDistance = fogSettings.EndDistance;
        }

        protected override void _SetVignette(UMI3DGlobalVignette vignette)
        {
#if USING_URP
            if (!volume.profile.TryGet(out Vignette vignetteComponent))
                vignetteComponent = volume.profile.Add<Vignette>(true);

            vignetteComponent.active = vignette.VignetteEnabled;
            vignetteComponent.color.value = vignette.Color;
            vignetteComponent.center.value = vignette.Center;
            vignetteComponent.intensity.value = vignette.Intensity;
            vignetteComponent.smoothness.value = vignette.Smoothness;
            vignetteComponent.rounded.value = vignette.Rounded;
#else
// Create an instance of a vignette
            if (m_Vignette == null)
                m_Vignette = ScriptableObject.CreateInstance<Vignette>();
            m_Vignette.enabled.Override(vignette.VignetteEnabled);
            m_Vignette.mode.Override(Convert(vignette.Mode));
            m_Vignette.color.Override(vignette.Color);
            m_Vignette.center.Override(vignette.Center);
            m_Vignette.intensity.Override(vignette.Intensity);
            m_Vignette.smoothness.Override(vignette.Smoothness);
            m_Vignette.roundness.Override(vignette.Roundness);
            m_Vignette.rounded.Override(vignette.Rounded);


            if (m_Volume == null)
                m_Volume = GetCamera().GetComponent<PostProcessVolume>();
            if (m_Volume.sharedProfile.TryGetSettings(out Vignette oldv))
                m_Volume.sharedProfile.RemoveSettings<Vignette>();
            m_Volume.sharedProfile.AddSettings(m_Vignette);
#endif
        }

#if !USING_URP
        private VignetteMode Convert(UMI3DGlobalVignette.VignetteMode vignette)
        {
            switch (vignette)
            {
                case UMI3DGlobalVignette.VignetteMode.Classic:
                    return VignetteMode.Classic;
                case UMI3DGlobalVignette.VignetteMode.Masked:
                    return VignetteMode.Masked;
            }
            return 0;
        }
#endif

        private FogMode Convert(UMI3DFogSettings.FogMode fogMode)
        {
            switch (fogMode)
            {
                case UMI3DFogSettings.FogMode.Linear:
                    return FogMode.Linear;
                case UMI3DFogSettings.FogMode.Exponential:
                    return FogMode.Exponential;
                case UMI3DFogSettings.FogMode.ExponentialSquared:
                    return FogMode.ExponentialSquared;
            }
            return 0;
        }

    }
}
#endif