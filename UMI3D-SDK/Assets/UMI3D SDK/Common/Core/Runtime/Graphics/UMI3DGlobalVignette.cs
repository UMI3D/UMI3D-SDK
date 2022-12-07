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
    /// Post processing vignetting effect. Vignetting reduce the image's brightness toward the periphery compared to the image center. 
    /// </summary>
    /// It is a useful effect to add a soft fade at the edges of one's camera.
    public class UMI3DGlobalVignette : MonoBehaviour
    {
        /// <summary>
        /// Should the vignette effect be enabled?
        /// </summary>
        [SerializeField, Tooltip("Should the vignette effect be enabled?")]
        private bool vignetteEnabled = false;

        /// <summary>
        /// Define the vignette color, the color that is applied to the camera.
        /// </summary>
        /// Use the alpha channel for transparency.
        [SerializeField, Tooltip("Define the vignette color, the color that is applied to the camera. Use the alpha channel for transparency.")]
        private Color color = Color.black;

        /// <summary>
        /// Point used as the center of the vignette effect.
        /// </summary>
        /// Default is the screen center.
        [SerializeField, Tooltip("Point used as the center of the vignette effect.")]
        private Vector2 center = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// Controls the amount of the effect.
        /// </summary>
        /// A lower value will results is a smaller vignetting effect.
        [SerializeField, Tooltip("Controls the amount of the effect. A lower value will result is a smaller vignetting effect.")]
        [Range(0f, 1f)]
        private float intensity = 0f;

        /// <summary>
        /// Controls the smoothness of the vignette's borders transition.
        /// </summary>
        [SerializeField]
        [Range(0f, 1f), Tooltip("Controls the smoothness of the vignette's borders transition.")]
        private float smoothness = 0f;

        /// <summary>
        /// If true, the vignette is perfectly round. Otherwise, the shape will depend of the ratio of the screen used.
        /// </summary>
        [SerializeField, Tooltip("If true, the vignette is perfectly round. Otherwise, the shape will depend of the ratio of the screen used.")]
        private bool rounded = false;

#if USING_URP

#else
        /// <summary>
        /// The two different modes for the vignette effect.
        /// </summary>
        public enum VignetteMode
        {
            /// <summary>
            /// This mode offers parametric controls for the position, shape and intensity of the Vignette.
            /// </summary>
            Classic,

            /// <summary>
            /// This mode multiplies a custom texture mask over the screen to create a Vignette effect.
            /// </summary>
            Masked
        }

        /// <summary>
        /// Mode used for vignette rendering.
        /// </summary>
        [SerializeField, Tooltip("Mode used for vignette rendering.")]
        private VignetteMode mode = VignetteMode.Classic;

        /// <summary>
        /// Controls the squared shape of the vignette. 
        /// Higher values tend towards a more circular shape while lowers tend to a more squared shape.
        /// </summary>
        [SerializeField, Tooltip("Controls the squared shape of the vignette. \n"
                                +"Higher values tend towards a more circular shape while lowers tend to a more squared shape.")]
        [Range(0f, 1f)]
        private float roundness = 0f;
#endif
        /// <summary>
        /// See <see cref="vignetteEnabled"/>.
        /// </summary>
        public bool VignetteEnabled => vignetteEnabled;
        /// <summary>
        /// See <see cref="color"/>.
        /// </summary>
        public Color Color => color;
        /// <summary>
        /// See <see cref="center"/>.
        /// </summary>
        public Vector2 Center => center;
        /// <summary>
        /// See <see cref="intensity"/>.
        /// </summary>
        public float Intensity => intensity;
        /// <summary>
        /// See <see cref="smoothness"/>.
        /// </summary>
        public float Smoothness => smoothness;
        /// <summary>
        /// See <see cref="rounded"/>.
        /// </summary>
        public bool Rounded => rounded;

#if !USING_URP

    /// <summary>
    /// See <see cref="mode"/>.
    /// </summary>
    public VignetteMode Mode => mode;
    /// <summary>
    /// See <see cref="roundness"/>.
    /// </summary>
    public float Roundness => roundness;

#endif
        /// <summary>
        /// Remove the vignette effet.
        /// </summary>
        public static void ResetGlobalVignette()
        {
            UMI3DAbstractPostProcessing.ResetVignette();
        }

        private void Start()
        {
            UMI3DAbstractPostProcessing.SetVignette(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UMI3DAbstractPostProcessing.SetVignette(this);
        }
#endif

    }
}