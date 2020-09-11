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
using umi3d.common;
using umi3d.edk;
using UnityEngine;

namespace umi3d.cdk
{
    [CreateAssetMenu(fileName = "Umi3D_PBR_Material", menuName = "UMI3D/Umi3D_PBR_Material")]
    public class PBRMaterial : MaterialSO
    {
        public Color baseColorFactor;
        [Range(0.0f, 1.0f)]
        public float metallicFactor;
        [Range(0.0f, 1.0f)]
        public float roughnessFactor;

        public Color emissive;
        //   public UMI3DMaterialDto textures;
        public CustomTextures textures;// = new CustomTextures();

        public override GlTFMaterialDto ToDto()
        {
            var extensions = new GlTFMaterialExtensions() { umi3d = textures.ToDto() };
            extensions.umi3d.id = GetId();
            return new GlTFMaterialDto()
            {
                alphaMode = alphaMode.ToString(),
                doubleSided = false,
                name = name,
                pbrMetallicRoughness = new PBRMaterialDto()
                {
                    baseColorFactor = (Vector4)baseColorFactor,
                    metallicFactor = metallicFactor,
                    roughnessFactor = roughnessFactor,

                },
                emissiveFactor = (Vector3)(Vector4)emissive,
                extensions = extensions
            };
        }

        public override string GetId()
        {
            if (!registered)
            {
                var matDto = this.textures.ToDto();
                RegisterMaterial(matDto);
                registered = true;
                textures.id = matDto.id;
            }
            //   Debug.Log("Material id : " + textures.id);
            return textures.id;
        }
        private bool registered = false;

        protected override void OnEnable()
        {
            //        Debug.Log("init mat id");

            textures.id = null;
            registered = false;

        }



        public void RegisterMaterial(AbstractEntityDto mat)
        {
            //   Debug.Log("try registered");
            if (string.IsNullOrEmpty(mat.id) || UMI3DEnvironment.GetEntity<MaterialSO>(mat.id) == null)
            {
                mat.id = UMI3DEnvironment.Register(this);
                //      Debug.Log("registered");
            }
        }
    }
    [System.Serializable]
    public class CustomTextures
    {
        public string id = null;
        public UMI3DTextureResource baseColorTexture;
        public UMI3DTextureResource metallicRoughnessTexture;
        public UMI3DScalableTextureReource normalTexture;
        public UMI3DTextureResource emissiveTexture;
        public UMI3DTextureResource occlusionTexture;
        public UMI3DTextureResource metallicTexture;
        public UMI3DTextureResource roughnessTexture;
        public UMI3DScalableTextureReource heightTexture;

        public UMI3DMaterialDto ToDto()
        {
            return new UMI3DMaterialDto()
            {
                baseColorTexture = baseColorTexture.ToDto(),
                emissiveTexture = emissiveTexture.ToDto(),
                heightTexture = heightTexture.ToDto(),
                metallicRoughnessTexture = metallicRoughnessTexture.ToDto(),
                metallicTexture = metallicTexture.ToDto(),
                normalTexture = normalTexture.ToDto(),
                occlusionTexture = occlusionTexture.ToDto(),
                roughnessTexture = roughnessTexture.ToDto(),

            };
        }

    }
}