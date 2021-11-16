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
    public class UMI3DFogSettings : MonoBehaviour
    {
        //
        // Summary:
        //     Fog mode to use.
        public enum FogMode
        {
            //
            // Summary:
            //     Linear fog.
            Linear = 1,
            //
            // Summary:
            //     Exponential fog.
            Exponential = 2,
            //
            // Summary:
            //     Exponential squared fog (default).
            ExponentialSquared = 3
        }

        [SerializeField]
        private bool fogEnabled = false;

        [SerializeField]
        private Color color = new Color(0.5f, 0.5f, 0.5f);

        [SerializeField]
        private FogMode mode = FogMode.ExponentialSquared;

        [SerializeField]
        private float density = 0.01f;

        [SerializeField]
        private float startDistance = 0f;

        [SerializeField]
        private float endDistance = 300f;

        public bool FogEnabled => fogEnabled;
        public Color Color => color;
        public FogMode Mode => mode;
        public float Density => density;
        public float StartDistance => startDistance;
        public float EndDistance => endDistance;

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
