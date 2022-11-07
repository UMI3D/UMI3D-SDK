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
    /// Post-processing deferred fog effect. Overlayfs a color on distant objects depending on their distance.
    /// </summary>
    /// It is useful to simulates the effect of fog or mist.
    public class UMI3DFogSettings : MonoBehaviour
    {
        /// <summary>
        /// Fog mode to use.
        /// </summary>
        public enum FogMode
        {
            /// <summary>
            /// Linear fog.
            /// </summary>
            Linear = 1,
            /// <summary>
            /// Exponential fog.
            /// </summary>
            Exponential = 2,
            /// <summary>
            /// Exponential squared fog (default).
            /// </summary>
            ExponentialSquared = 3
        }

        /// <summary>
        /// Should the fog effect be enabled?
        /// </summary>
        [SerializeField, Tooltip("Should the fog effect be enabled?")]
        private bool fogEnabled = false;

        /// <summary>
        /// Color of the foggy overlay.
        /// </summary>
        [SerializeField, Tooltip("Color of the foggy overlay.")]
        private Color color = new Color(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Fog mode. Configure the increase of the fog effect with distance.
        /// </summary>
        [SerializeField, Tooltip("Fog mode. Configure the increase of the fog effect with distance.")]
        private FogMode mode = FogMode.ExponentialSquared;

        /// <summary>
        /// Fog density. A denser fog will be opaque for closer objects.
        /// </summary>
        [SerializeField, Tooltip("Fog density. A denser fog will be opaque for closer objects.")]
        private float density = 0.01f;

        /// <summary>
        /// Distance at which the deferred fog effect should start to be applied.
        /// </summary>
        [SerializeField, Tooltip("Distance at which the deferred fog effect should start to be applied.")]
        private float startDistance = 0f;

        /// <summary>
        /// Distance at which the deferred fog effect should stop to increase
        /// </summary>
        [SerializeField, Tooltip("Distance at which the deferred fog effect should stop to increase.")]
        private float endDistance = 300f;

        /// <summary>
        /// See <see cref="fogEnabled"/>.
        /// </summary>
        public bool FogEnabled => fogEnabled;
        /// <summary>
        /// See <see cref="color"/>.
        /// </summary
        public Color Color => color;
        /// <summary>
        /// See <see cref="mode"/>.
        /// </summary
        public FogMode Mode => mode;
        /// <summary>
        /// See <see cref="density"/>.
        /// </summary
        public float Density => density;
        /// <summary>
        /// See <see cref="startDistance"/>.
        /// </summary
        public float StartDistance => startDistance;
        /// <summary>
        /// See <see cref="endDistance"/>.
        /// </summary
        public float EndDistance => endDistance;

        /// <summary>
        /// Remove the deferred fog efect.
        /// </summary>
        public static void ResetFogSettings()
        {
            UMI3DAbstractPostProcessing.ResetFog();
        }

        private void ConfigureFog()
        {
            UMI3DAbstractPostProcessing.SetFogSettings(this);
        }

        // Start is called before the first frame update
        private void Start()
        {
            ConfigureFog();
        }

#if UNITY_EDITOR

        // Update is called once per frame
        private void OnValidate()
        {
            ConfigureFog();
        }

#endif

    }
}
