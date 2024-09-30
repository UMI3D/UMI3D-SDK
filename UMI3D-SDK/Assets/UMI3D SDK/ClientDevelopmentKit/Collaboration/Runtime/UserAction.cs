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

using System;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class UserAction : UserActionDto
    {
        ulong environmentId;
        Texture2D texture2D;

        public async Task<Texture2D> GetTexture() {
            if (texture2D != null)
                return texture2D;
            if (icon2D == null)
                return null;

            return await loadResources();
        }

        public UserAction(ulong environmentId, UserActionDto dto)
        {
            this.environmentId = environmentId;
            this.id = dto.id;
            this.isPrimary = dto.isPrimary;
            this.name = dto.name;
            this.description = dto.description;
            this.icon3D = dto.icon3D;
            this.icon2D = dto.icon2D;
        }

        async Task<Texture2D> loadResources()
        {

            if (this.icon2D != null && this.icon2D.variants.Count > 0)
            {
                try
                {
                    FileDto fileToLoad = UMI3DEnvironmentLoader.AbstractParameters.ChooseVariant(this.icon2D.variants);
                    if (fileToLoad == null)
                        return null;

                    string ext = fileToLoad.extension;
                    IResourcesLoader loader = UMI3DEnvironmentLoader.AbstractParameters.SelectLoader(ext);
                    if (loader != null)
                    {
                        var o = await UMI3DResourcesManager.LoadFile(UMI3DGlobalID.EnvironmentId, fileToLoad, loader);
                        return (Texture2D)o;
                    }
                }
                catch
                {

                }
            }
            return null;
        }

        public void Call()
        {
            var dto = new UserActionRequestDto() { environmentId = this.environmentId, actionId = this.id };
            UMI3DClientServer.SendRequest(dto, true);
        }

        public static Func<UserActionDto, UserAction> Converter(ulong environmentId) => dto => new(environmentId, dto);
    }
}