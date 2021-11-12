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
    public class UMI3DGlobalBloom : MonoBehaviour
    {


        [SerializeField]
        private bool bloomEnabled = false;

        [SerializeField]
        private float intensity = 0f;

        [SerializeField]
        private float threshold = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float softKnee = 0.5f;

        [SerializeField]
        private float clamp = 65472f;

        [SerializeField]
        [Range(1f, 10f)]
        private float diffusion = 7f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float anamorphicRatio = 0f;

        [SerializeField]
        [ColorUsageAttribute(false, true)]
        private Color color = Color.white;

        [SerializeField]
        private bool fastMode = false;

        [Header("Dirtiness")]
        private Texture dirt_Texture = null;

        [SerializeField]
        private float dirt_Intensity = 0f;

        public bool BloomEnabled { get => bloomEnabled; }
        public float Intensity { get => intensity;  }
        public float Threshold { get => threshold;  }
        public float SoftKnee { get => softKnee;  }
        public float Clamp { get => clamp;  }
        public float Diffusion { get => diffusion; }
        public float AnamorphicRatio { get => anamorphicRatio; }
        public Color Color { get => color;  }
        public bool FastMode { get => fastMode;  }
        public Texture Dirt_Texture { get => dirt_Texture;  }
        public float Dirt_Intensity { get => dirt_Intensity;  }

        public static void ResetGlobalBloom()
        {
            UMI3DAbstractPostProcessing.ResetBloom();
        }

        void Start()
        {
            UMI3DAbstractPostProcessing.SetBloom(this);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            UMI3DAbstractPostProcessing.SetBloom(this);
        }
#endif

    }
}