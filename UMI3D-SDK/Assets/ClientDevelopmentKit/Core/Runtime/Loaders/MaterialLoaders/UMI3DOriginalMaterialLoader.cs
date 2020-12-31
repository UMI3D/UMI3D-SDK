using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DOriginalMaterialLoader : AbstractUMI3DMaterialLoader
    {
        ///<inheritdoc/>        
        public override bool IsSuitableFor(GlTFMaterialDto gltfMatDto)
        {
            if (gltfMatDto.extensions.umi3d is UMI3DOriginalMaterialDto)
                return true;
            return false;
        }

        ///<inheritdoc/>
        public override void LoadMaterialFromExtension(GlTFMaterialDto dto, Action<Material> callback)
        {
            UMI3DOriginalMaterialDto originalMat = dto.extensions.umi3d as UMI3DOriginalMaterialDto;

            if (originalMat != null)
            {

                callback.Invoke(null);



            }
            else
            {
                Debug.LogWarning("extension is null");
            }
        }


    }
}
