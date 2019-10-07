/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Data Tranfert Object for UMI3D media Update  
    /// </summary>
    [System.Serializable]
    public class MediaUpdateDto : UMI3DDto
    {
        /// <summary>
        /// Ambient ligth Color for Flat Type.
        /// </summary>
        public SerializableColor AmbientColor;

        /// <summary>
        /// Sky light Color for Gradient Type.
        /// </summary>
        public SerializableColor SkyColor;

        /// <summary>
        /// Horizon light Color for Gradient Type.
        /// </summary>
        public SerializableColor HorizonColor;

        /// <summary>
        /// Ground light Color for Gradient Type.
        /// </summary>
        public SerializableColor GroundColor;

        /// <summary>
        /// Ambient light Intensity.
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Ambient Light Type.
        /// </summary>
        public AmbientType Type;

        ///// <summary>
        ///// Resource for skybox image
        ///// </summary>
        //public ResourceDto SkyboxImage;

        public MediaUpdateDto() : base() { }

    }
}
