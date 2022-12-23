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

using System.Collections.Generic;

namespace umi3d.common
{
    /// <summary>
    /// DTO describing an Environment under the UMI3D standard.
    /// </summary>
    [System.Serializable]
    public class UMI3DEnvironmentDto : UMI3DDto
    {
        /// <summary>
        /// List of Asset Libraries available for the environment, by their named id.
        /// </summary>
        public List<string> LibrariesId;

        /// <summary>
        /// List of scene that are directly packaged with the environment.
        /// </summary>
        public List<PreloadedSceneDto> preloadedScenes;

        /// <summary>
        /// Ambient lighting type in the environment.
        /// </summary>
        public AmbientType ambientType;

        /// <summary>
        /// Default color of the sky.
        /// </summary>
        public SerializableColor skyColor;

        /// <summary>
        /// Default color
        /// </summary>
        public SerializableColor horizontalColor;

        /// <summary>
        /// Default color of the ground.
        /// </summary>
        public SerializableColor groundColor;

        /// <summary>
        /// Default intensity of the ambient light.
        /// </summary>
        public float ambientIntensity;

        /// <summary>
        /// Ressource of the asset for the skybox.
        /// </summary>
        public ResourceDto skybox;

        /// <summary>
        /// Format for <see cref="skybox"/>.
        /// </summary>
        public SkyboxType skyboxType;

        /// <summary>
        /// Rotation for <see cref="skybox"/>, only works with <see cref="SkyboxType.Equirectangular"/> [0, 360] degrees.
        /// </summary>
        public float skyboxRotation = 0;

        /// <summary>
        /// ressource of the material applied to all objects by default.
        /// </summary>
        public ResourceDto defaultMaterial;
    }
}
