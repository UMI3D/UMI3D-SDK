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

using UnityEngine;

namespace umi3d.common.graphics
{
    /// <summary>
    /// Post-processing bloom effect. Produce a glowing effect on light sources.
    /// </summary>
    /// It is a useful effect to produce the illusion that light sources are intensely bright.
    public class UMI3DGlobalBloom : MonoBehaviour
    {
        /// <summary>
        /// Should the bloom effect be enabled?
        /// </summary>
        [SerializeField, Tooltip("Should the bloom effect be enabled?")]
        private bool bloomEnabled = false;

        /// <summary>
        /// Strength of the bloom effect.
        /// </summary>
        [SerializeField, Tooltip("Strength of the bloom effect.")]
        private float intensity = 0f;

        /// <summary>
        /// Thresdhold under which pixels with a lower brightness are filtered out.
        /// </summary>
        [SerializeField, Tooltip("Thresdhold under which pixels with a lower brightness are filtered out.")]
        private float threshold = 1f;

#if USING_URP
        /// <summary>
        /// Radius of the bloom effect.
        /// </summary>
        /// Used by Unity's URP.
        [SerializeField]
        [Range(0f, 1f), Tooltip("Radius of the bloom effect.")]
        private float scatter = 0.5f;

        /// <summary>
        /// If true, high quality sampling is used.
        /// </summary>
        /// Used by Unity's URP.
        [SerializeField, Tooltip("If true, high quality sampling is used. Imrpove quality but can decrease performance.")]
        private bool highQualityFiltering = false;

        /// <summary>
        /// Number of final iterations to avoid. 
        /// </summary>
        /// Used by Unity's URP. Higher values reduce perfomance cost.
        [SerializeField]
        [Range(0, 16), Tooltip("Number of final iterations to avoid. Higher values reduce perfomance cost.")]
        private int skipIterations = 1;

#else
        /// <summary>
        /// Transition softness.
        /// </summary>
        [SerializeField]
        [Range(0f, 1f), Tooltip("Transition softness.")]
        private float softKnee = 0.5f;

        /// <summary>
        /// Extent of veiling effect.
        /// </summary>
        /// Do not change this value at runtime.
        [SerializeField]
        [Range(1f, 10f), Tooltip("Extent of veiling effect.")]
        private float diffusion = 7f;

        /// <summary>
        /// Distort the bloom.
        /// </summary>
        /// Negative value for vertical distortion, positive for horizontal one.
        [SerializeField]
        [Range(-1f, 1f), Tooltip("Distort the bloom. Negative value for vertical distortion, positive for horizontal one.")]
        private float anamorphicRatio = 0f;

        /// <summary>
        /// Improve the performance at the cost of bloom quality.
        /// </summary>
        [SerializeField, Tooltip("Improve the performance at the cost of bloom quality.")]
        private bool fastMode = false;
#endif
        /// <summary>
        /// Tint color of the bloom effect.
        /// </summary>
        [SerializeField, Tooltip("Tint color of the bloom effect.")]
        [ColorUsageAttribute(false, true)]
        private Color tint = Color.white;

        /// <summary>
        /// Value in the gamma space to clamp the pixels.
        /// </summary>
        [SerializeField, Tooltip("Value in the gamma space to clamp the pixels.")]
        private float clamp = 65472f;

        /// <summary>
        /// Fullscreen texture mask that diffract the bloom effect.
        /// </summary>
        [Header("Dirtiness")]
        private readonly Texture dirt_Texture = null;

        /// <summary>
        /// Strength of the diffraction effect applied by dirt texture masking.
        /// </summary>
        [SerializeField, Tooltip("Strength of the diffraction effect applied by dirt texture masking.")]
        private float dirt_Intensity = 0f;

        /// <summary>
        /// See <see cref="bloomEnabled"/>.
        /// </summary>
        public bool BloomEnabled => bloomEnabled;
        /// <summary>
        /// See <see cref="intensity"/>.
        /// </summary>
        public float Intensity => intensity;
        /// <summary>
        /// See <see cref="threshold"/>.
        /// </summary>
        public float Threshold => threshold;
        /// <summary>
        /// See <see cref="clamp"/>.
        /// </summary>
        public float Clamp => clamp;
        /// <summary>
        /// See <see cref="tint"/>.
        /// </summary>
        public Color Tint => tint;

#if USING_URP
        /// <summary>
        /// See <see cref="scatter"/>.
        /// </summary>
        public float Scatter => scatter;
        /// <summary>
        /// See <see cref="highQualityFiltering"/>.
        /// </summary>
        public bool HighQualityFiltering => highQualityFiltering;
        /// <summary>
        /// See <see cref="skipIterations"/>.
        /// </summary>
        public int SkipIterations => skipIterations;

#else
        /// <summary>
        /// See <see cref="diffusion"/>.
        /// </summary>
        public float Diffusion => diffusion;
        /// <summary>
        /// See <see cref="anamorphicRatio"/>.
        /// </summary>
        public float AnamorphicRatio => anamorphicRatio;
        /// <summary>
        /// See <see cref="fastMode"/>.
        /// </summary>
        public bool FastMode => fastMode;
        /// <summary>
        /// See <see cref="softKnee"/>.
        /// </summary>
        public float SoftKnee => softKnee;
#endif
        /// <summary>
        /// See <see cref="dirt_Texture"/>.
        /// </summary>
        public Texture Dirt_Texture => dirt_Texture;
        /// <summary>
        /// See <see cref="dirt_Intensity"/>.
        /// </summary>
        public float Dirt_Intensity => dirt_Intensity;

        /// <summary>
        /// Remove the global bloom effect.
        /// </summary>
        public static void ResetGlobalBloom()
        {
            UMI3DAbstractPostProcessing.ResetBloom();
        }

        private void Start()
        {
            UMI3DAbstractPostProcessing.SetBloom(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UMI3DAbstractPostProcessing.SetBloom(this);
        }
#endif

    }
}