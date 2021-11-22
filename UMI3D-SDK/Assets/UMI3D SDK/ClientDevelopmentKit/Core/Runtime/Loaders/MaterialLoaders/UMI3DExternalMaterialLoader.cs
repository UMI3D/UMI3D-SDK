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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DExternalMaterialLoader : AbstractUMI3DMaterialLoader
    {
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading|DebugScope.Material;

        ///<inheritdoc/>
        public override bool IsSuitableFor(GlTFMaterialDto gltfMatDto)
        {
            if (gltfMatDto.extensions.umi3d is ExternalMaterialDto)
                return true;
            return false;
        }

        ///<inheritdoc/>
        public override void LoadMaterialFromExtension(GlTFMaterialDto dto, Action<Material> callback)
        {
            var externalMat = dto.extensions.umi3d as ExternalMaterialDto;
            KHR_texture_transform KhrTT = dto.extensions.KHR_texture_transform;
            if (externalMat != null)
            {
                FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(externalMat.resource.variants);  // Peut etre ameliore

                string url = fileToLoad.url;
                string ext = fileToLoad.extension;
                string authorization = fileToLoad.authorization;
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                if (loader != null)
                {
                    UMI3DResourcesManager.LoadFile(
                   (dto.extensions.umi3d as ExternalMaterialDto)?.id ?? 0,
                   fileToLoad,
                   loader.UrlToObject,
                   loader.ObjectFromCache,
                   (o) =>
                   {
                       var newMat = (Material)o;
                       if (newMat != null)
                       {
                           try
                           {
                               //     ApplyTiling(KhrTT.offset, KhrTT.scale, newMat);

                               callback.Invoke(newMat);
                               ReadAdditionalShaderProperties(externalMat.id, externalMat.shaderProperties, newMat);
                           }
                           catch
                           {
                               UMI3DLogger.LogError("Fail to load material : " + url,scope);
                           }

                       }
                       else
                       {
                           UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(Material)}",scope);
                       }
                   },
                   e => UMI3DLogger.LogWarning(e,scope),
                   loader.DeleteObject
                   );
                }
                else
                {
                    UMI3DLogger.LogWarning("Loader is null for external material",scope);
                }



            }
            else
            {
                UMI3DLogger.LogWarning("extension is null",scope);
            }
        }
    }
}
