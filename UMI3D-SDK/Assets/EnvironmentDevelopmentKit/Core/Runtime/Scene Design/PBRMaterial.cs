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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.edk;
using UnityEngine;

namespace umi3d.edk
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

        // list of supported type for object: float, int, vector2-3-4, Color, TextureDto
        public Dictionary<string, object> shaderProperties = new Dictionary<string, object>();

        public Vector2 tilingScale = Vector2.one;
        public Vector2 tilingOffset = Vector2.zero;


        public override GlTFMaterialDto ToDto()
        {
            var extensions = new GlTFMaterialExtensions() { umi3d = textures.ToDto() };
            extensions.umi3d.shaderProperties = shaderProperties;
            extensions.umi3d.id = GetId();
            extensions.KHR_texture_transform.offset = tilingOffset;
            extensions.KHR_texture_transform.scale = tilingScale;
            return new GlTFMaterialDto()
            {
                alphaMode = alphaMode.ToString(),
                doubleSided = false,
                name = name,
                pbrMetallicRoughness = new PBRMaterialDto()
                {
                    baseColorFactor = baseColorFactor,
                    metallicFactor = metallicFactor,
                    roughnessFactor = roughnessFactor,

                },
                emissiveFactor = (Vector3)(Vector4)emissive,
                extensions = extensions
            };
        }

        protected override string GetId()
        {
            if (!registered)
            {
                UMI3DMaterialDto matDto = this.textures.ToDto();
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


        public UMI3DAsyncProperty<Color> objectBaseColorFactor;
        public UMI3DAsyncProperty<float> objectMetallicFactor;
        public UMI3DAsyncProperty<float> objectRoughnessFactor;
        public UMI3DAsyncProperty<Color> objectEmissiveFactor;
        public UMI3DAsyncProperty<UMI3DTextureResource> objectMaintexture;
        public UMI3DAsyncProperty<UMI3DTextureResource> objectMetallicRoughnessTexture;
        public UMI3DAsyncProperty<UMI3DScalableTextureReource> objectNormalTexture;
        public UMI3DAsyncProperty<UMI3DTextureResource> objectEmissiveTexture;
        public UMI3DAsyncProperty<UMI3DTextureResource> objectOcclusionTexture;
        public UMI3DAsyncProperty<UMI3DTextureResource> objectMetallicTexture;
        public UMI3DAsyncProperty<UMI3DTextureResource> objectRoughnessTexture;
        public UMI3DAsyncProperty<UMI3DScalableTextureReource> objectHeightTexture;
        public UMI3DAsyncProperty<Vector2> objectTextureTilingScale;
        public UMI3DAsyncProperty<Vector2> objectTextureTilingOffset;
        public UMI3DAsyncProperty<float> objectNormalTextureScale;
        public UMI3DAsyncProperty<float> objectHeightTextureScale;
        public UMI3DAsyncDictionnaryProperty<string, object> objectShaderProperties; // not totaly implemented

        private UMI3DAsyncPropertyEquality pCompare = new UMI3DAsyncPropertyEquality();


        protected override void InitDefinition(string id)
        {
            Debug.Log("id mat " + id);
            objectRoughnessFactor = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.RoughnessFactor, this.roughnessFactor, null, pCompare.FloatEquality);
            objectRoughnessFactor.OnValueChanged += (float f) =>
            {
                roughnessFactor = f;
                /*Debug.Log("change roughness " + f.ToString());*/
            };

            objectMetallicFactor = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.MetallicFactor, this.metallicFactor, null, pCompare.FloatEquality);
            objectMetallicFactor.OnValueChanged += (float f) => { metallicFactor = f; };

            objectBaseColorFactor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.BaseColorFactor, this.baseColorFactor, ToUMI3DSerializable.ToSerializableColor, pCompare.ColorEquality);
            objectBaseColorFactor.OnValueChanged += (Color c) => { baseColorFactor = c; };

            objectEmissiveFactor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.EmissiveFactor, this.emissive, ToUMI3DSerializable.ToSerializableColor, pCompare.ColorEquality);
            objectEmissiveFactor.OnValueChanged += (Color c) => { emissive = c; };

            objectMaintexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.Maintexture, this.textures.baseColorTexture, (x, u) => { return x.ToDto(); });
            objectMaintexture.OnValueChanged += (UMI3DTextureResource t) => { textures.baseColorTexture = t; };

            objectMetallicRoughnessTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.MetallicRoughnessTexture, this.textures.metallicRoughnessTexture, (x, u) => { return x.ToDto(); });
            objectMetallicRoughnessTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.metallicRoughnessTexture = t; };

            objectNormalTexture = new UMI3DAsyncProperty<UMI3DScalableTextureReource>(id, UMI3DPropertyKeys.NormalTexture, this.textures.normalTexture, (x, u) => { return x.ToDto(); });
            objectNormalTexture.OnValueChanged += (UMI3DScalableTextureReource t) => { textures.normalTexture = t; };

            objectEmissiveTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.EmissiveTexture, this.textures.emissiveTexture, (x, u) => { return x.ToDto(); });
            objectEmissiveTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.emissiveTexture = t; };

            objectOcclusionTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.OcclusionTexture, this.textures.occlusionTexture, (x, u) => { return x.ToDto(); });
            objectOcclusionTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.occlusionTexture = t; };

            objectMetallicTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.MetallicTexture, this.textures.metallicTexture, (x, u) => { return x.ToDto(); });
            objectMetallicTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.metallicTexture = t; };

            objectRoughnessTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.RoughnessTexture, this.textures.roughnessTexture, (x, u) => { return x.ToDto(); });
            objectRoughnessTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.roughnessTexture = t; };

            objectHeightTexture = new UMI3DAsyncProperty<UMI3DScalableTextureReource>(id, UMI3DPropertyKeys.HeightTexture, this.textures.heightTexture, (x, u) => { return x.ToDto(); });
            objectHeightTexture.OnValueChanged += (UMI3DScalableTextureReource t) => { textures.heightTexture = t; };


            objectTextureTilingOffset = new UMI3DAsyncProperty<Vector2>(id, UMI3DPropertyKeys.TextureTilingOffset, new Vector2(tilingOffset.x, tilingOffset.y), ToUMI3DSerializable.ToSerializableVector2, pCompare.Vector2Equality);
            objectTextureTilingOffset.OnValueChanged += (Vector2 v) => { tilingOffset = new Vector2(v.x, v.y); };

            objectTextureTilingScale = new UMI3DAsyncProperty<Vector2>(id, UMI3DPropertyKeys.TextureTilingScale, new Vector2(tilingScale.x, tilingScale.y), ToUMI3DSerializable.ToSerializableVector2, pCompare.Vector2Equality);
            objectTextureTilingScale.OnValueChanged += (Vector2 v) => { tilingScale = new Vector2(v.x, v.y); };

            objectNormalTextureScale = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.NormalTextureScale, this.textures.normalTexture.scale, null, pCompare.FloatEquality);
            objectNormalTextureScale.OnValueChanged += (float f) => { textures.normalTexture.scale = f; };

            objectHeightTextureScale = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.HeightTextureScale, this.textures.heightTexture.scale, null, pCompare.FloatEquality);
            objectHeightTextureScale.OnValueChanged += (float f) => { textures.heightTexture.scale = f; };

            objectShaderProperties = new UMI3DAsyncDictionnaryProperty<string, object>(id, UMI3DPropertyKeys.ShaderProperties, this.shaderProperties, null, null, null, (d) =>
            {
                return new Dictionary<string, object>(d);
            });
            objectShaderProperties.OnInnerValueChanged += (string s, object o) => { shaderProperties[s] = o; };
            objectShaderProperties.OnInnerValueAdded += (string s, object o) => { shaderProperties.Add(s, o); };
            objectShaderProperties.OnInnerValueRemoved += (string s) => { shaderProperties.Remove(s); };
            objectShaderProperties.OnValueChanged += (Dictionary<string, object> d) => { shaderProperties = d; };
        }

        public static void UpdateTexture(UMI3DAsyncProperty<UMI3DTextureResource> objectTexture, UMI3DTextureResource newTexture)
        {
            Transaction transaction = new Transaction();
            Operation op = objectTexture.SetValue(newTexture);
            if (op != null)
                transaction.Operations.Add(op);
            if (transaction.Operations.Count > 0)
            {
                transaction.reliable = false;
                UMI3DServer.Dispatch(transaction);
                //  transaction = new Transaction();
            }
        }

    }
    [System.Serializable]
    public class CustomTextures
    {
        public string id = null;
        // public string shaderName; // unused
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