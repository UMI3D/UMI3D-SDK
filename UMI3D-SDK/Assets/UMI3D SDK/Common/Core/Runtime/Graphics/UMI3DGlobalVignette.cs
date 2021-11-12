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
    public class UMI3DGlobalVignette : MonoBehaviour
    {
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

        [SerializeField]
        private bool vignetteEnabled = false;

        [SerializeField]
        private VignetteMode mode = VignetteMode.Classic;

        [SerializeField]
        private Color color = Color.black;

        [SerializeField]
        private Vector2 center = new Vector2(0.5f, 0.5f);

        [SerializeField]
        [Range(0f, 1f)]
        private float intensity = 0f;

        [SerializeField]
        [Range(0f, 1f)]
        private float smoothness = 0f;

        [SerializeField]
        [Range(0f, 1f)]
        private float roundness = 0f;

        [SerializeField]
        private bool rounded = false;

        public bool VignetteEnabled { get => vignetteEnabled; }
        public VignetteMode Mode { get => mode; }
        public Color Color { get => color;  }
        public Vector2 Center { get => center;  }
        public float Intensity { get => intensity; }
        public float Smoothness { get => smoothness;  }
        public float Roundness { get => roundness; }
        public bool Rounded { get => rounded; }

        public static void ResetGlobalVignette()
        {
            UMI3DAbstractPostProcessing.ResetVignette();
        }

        void Start()
        {
            UMI3DAbstractPostProcessing.SetVignette(this);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            UMI3DAbstractPostProcessing.SetVignette(this);
        }
#endif

    }
}