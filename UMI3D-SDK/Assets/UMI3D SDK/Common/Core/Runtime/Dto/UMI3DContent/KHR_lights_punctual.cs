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

namespace umi3d.common
{
    /// <summary>
    /// glTF 2.0 extension for ligths
    /// </summary>
    /// See <a href="https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_lights_punctual"/>.
    [System.Serializable]
    public class KHR_lights_punctual
    {
        public SerializableColor color = Color.white;
        public string name;
        public float intensity = 1f;
        public float range = 2f;
        public string type = LightTypes.Point;
        public KHR_spot spot = null;

        [System.Serializable]
        public class KHR_spot
        {
            public float innerConeAngle = 0f;
            public float outerConeAngle = Mathf.PI / 4f;
        }

        public static class LightTypes
        {
            public static string Directional = "directional";
            public static string Point = "point";
            public static string Spot = "spot";
        }
    }
}
