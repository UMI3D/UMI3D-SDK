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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using umi3d.common;
using UnityEngine;
using UnityEngine.Rendering;
using VolumetricLines;
using UnityEngine.UI;
using System.Collections;

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
                    //GetOrAddComponent<ResourceDtoLoader>().LoadDTO(dto.SkyboxImage, (Resource r) => {
                    //    if (currentSkyboxUrl == r.Url) return;
                    //    currentSkyboxUrl = r.Url;
                        
                    //    HDResourceCache.Download(r, (Texture2D text) =>
                    //    {
                    //        Material m = new Material(UMI3DBrowser.Scene.defaultMeshShader);
                    //        m.SetTexture("skybox", text);
                    //        RenderSettings.skybox = m;
                    //    });
                    //      });
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




        /// <summary>
        /// Update a scene object from dto.
        /// </summary>
        /// <param name="go">Gameobject to update</param>
        /// <param name="olddto">previous dto</param>
        /// <param name="newdto">dto to update to</param>
        private void m_UpdateObject3D(GameObject go, AbstractObject3DDto olddto, AbstractObject3DDto newdto)
        {
            if (newdto == null)
                return;

            go.name = newdto.Name;
            go.transform.localPosition = newdto.Position;
            go.transform.localScale = newdto.Scale;

            var billboard = go.GetComponent<Billboard>();
            if (newdto.Billboard)
            {
                if (billboard == null)
                    go.AddComponent<Billboard>();
            }
            else
            {
                if (billboard != null)
                    Destroy(billboard);
                go.transform.localRotation = newdto.Rotation;
            }

            if (olddto == null || newdto.Pid != olddto.Pid)
            {
                GameObject _p = null;
                if (newdto.Pid == null || newdto.Pid.Length == 0)
                    _p = gameObject;
                else
                    _p = GetObject(newdto.Pid);

                if (_p == null)
                    RemoveObject(newdto.Id);

                else if (go.transform.parent != _p.transform)
                    go.transform.SetParent(_p.transform, false);
            }
        }


    }
}