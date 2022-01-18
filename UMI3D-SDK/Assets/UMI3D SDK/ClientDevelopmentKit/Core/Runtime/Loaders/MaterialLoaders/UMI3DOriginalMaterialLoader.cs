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
    public class UMI3DOriginalMaterialLoader : AbstractUMI3DMaterialLoader
    {
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading|DebugScope.Material;

        ///<inheritdoc/>        
        public override bool IsSuitableFor(GlTFMaterialDto gltfMatDto)
        {
            if (gltfMatDto.extensions.umi3d is UMI3DOriginalMaterialDto)
                return true;
            return false;
        }

        ///<inheritdoc/>
        public override void LoadMaterialFromExtension(GlTFMaterialDto dto, Action<Material> callback, Material oldMaterial = null)
        {
            var originalMat = dto.extensions.umi3d as UMI3DOriginalMaterialDto;

            if (originalMat != null)
            {

                callback.Invoke(null);



            }
            else
            {
                UMI3DLogger.LogWarning("extension is null",scope);
            }
        }


    }
}
