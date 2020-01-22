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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// HD UMI3D Scene.
    /// </summary>
    public class HDScene : AbstractScene
    {



        /// <summary>
        /// Reset the scene.
        /// </summary>
        public override void ResetModule()
        {
            base.ResetModule();
            HDResourceCache.ClearCache();
        }

        string currentSkyboxUrl;

        /// <summary>
        /// Update this media from dto.
        /// </summary>
        /// <param name="dto">Old dto describing the object</param>
        public override void UpdateFromDTO(MediaUpdateDto dto)
        {
            RenderSettings.ambientMode = dto.Type.Convert();
            switch (dto.Type)
            {
                case AmbientType.Skybox:
                    RenderSettings.ambientSkyColor = dto.AmbientColor;
                    RenderSettings.ambientIntensity = dto.Intensity;
                    break;
                case AmbientType.Flat:
                    RenderSettings.ambientLight = dto.AmbientColor;
                    break;
                case AmbientType.Gradient:
                    RenderSettings.ambientSkyColor = dto.SkyColor;
                    RenderSettings.ambientEquatorColor = dto.HorizonColor;
                    RenderSettings.ambientGroundColor = dto.GroundColor;
                    break;
            }
        }
    }
}