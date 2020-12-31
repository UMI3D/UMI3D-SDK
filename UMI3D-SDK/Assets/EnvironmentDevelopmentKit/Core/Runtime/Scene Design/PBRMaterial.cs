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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    [CreateAssetMenu(fileName = "Umi3D_PBR_Material", menuName = "UMI3D/Umi3D_PBR_Material")]
    public class PBRMaterial : MaterialSO
    {
        public Color baseColorFactor = Color.white;
        [Range(0.0f, 1.0f)]
        public float metallicFactor;
        [Range(0.0f, 1.0f)]
        public float roughnessFactor;

        public Color emissive;
        //   public UMI3DMaterialDto textures;
        public CustomTextures textures = new CustomTextures();


        public Vector2 tilingScale = Vector2.one;
        public Vector2 tilingOffset = Vector2.zero;

        ///<inheritdoc/>
        public override GlTFMaterialDto ToDto()
        {
            var extensions = new GlTFMaterialExtensions() { umi3d = textures.ToDto() };
            ((UMI3DMaterialDto)extensions.umi3d).shaderProperties = shaderProperties;
            ((UMI3DMaterialDto)extensions.umi3d).id = GetId();
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

        ///<inheritdoc/>
        protected override string GetId()
        {
            if (!registered)
            {
                UMI3DMaterialDto matDto = this.textures.ToDto();
                RegisterMaterial(matDto);
            }
            //   Debug.Log("Material id : " + textures.id);
            return textures.id;
        }

        ///<inheritdoc/>
        protected override void SetId(string id)
        {
            textures.id = id;
            registered = true;
        }

        private bool registered = false;

        ///<inheritdoc/>
        protected override void OnEnable()
        {
            //        Debug.Log("init mat id");

            textures.id = null;
            registered = false;

        }


        public UMI3DAsyncProperty<Color> objectBaseColorFactor { get { Id(); return _objectBaseColorFactor; } protected set => _objectBaseColorFactor = value; }
        public UMI3DAsyncProperty<float> objectMetallicFactor { get { Id(); return _objectMetallicFactor; } protected set => _objectMetallicFactor = value; }
        public UMI3DAsyncProperty<float> objectRoughnessFactor { get { Id(); return _objectRoughnessFactor; } protected set => _objectRoughnessFactor = value; }
        public UMI3DAsyncProperty<Color> objectEmissiveFactor { get { Id(); return _objectEmissiveFactor; } protected set => _objectEmissiveFactor = value; }
        public UMI3DAsyncProperty<UMI3DTextureResource> objectMaintexture { get { Id(); return _objectMaintexture; } protected set => _objectMaintexture = value; }
        public UMI3DAsyncProperty<UMI3DTextureResource> objectMetallicRoughnessTexture { get { Id(); return _objectMetallicRoughnessTexture; } protected set => _objectMetallicRoughnessTexture = value; }
        public UMI3DAsyncProperty<UMI3DScalableTextureResource> objectNormalTexture { get { Id(); return _objectNormalTexture; } protected set => _objectNormalTexture = value; }
        public UMI3DAsyncProperty<UMI3DTextureResource> objectEmissiveTexture { get { Id(); return _objectEmissiveTexture; } protected set => _objectEmissiveTexture = value; }
        public UMI3DAsyncProperty<UMI3DTextureResource> objectOcclusionTexture { get { Id(); return _objectOcclusionTexture; } protected set => _objectOcclusionTexture = value; }
        public UMI3DAsyncProperty<UMI3DTextureResource> objectMetallicTexture { get { Id(); return _objectMetallicTexture; } protected set => _objectMetallicTexture = value; }
        public UMI3DAsyncProperty<UMI3DTextureResource> objectRoughnessTexture { get { Id(); return _objectRoughnessTexture; } protected set => _objectRoughnessTexture = value; }
        public UMI3DAsyncProperty<UMI3DTextureResource> objectChannelTexture { get { Id(); return _objectChannelTexture; } protected set => _objectChannelTexture = value; }
        public UMI3DAsyncProperty<UMI3DScalableTextureResource> objectHeightTexture { get { Id(); return _objectHeightTexture; } protected set => _objectHeightTexture = value; }
        public UMI3DAsyncProperty<Vector2> objectTextureTilingScale { get { Id(); return _objectTextureTilingScale; } protected set => _objectTextureTilingScale = value; }
        public UMI3DAsyncProperty<Vector2> objectTextureTilingOffset { get { Id(); return _objectTextureTilingOffset; } protected set => _objectTextureTilingOffset = value; }
        public UMI3DAsyncProperty<float> objectNormalTextureScale { get { Id(); return _objectNormalTextureScale; } protected set => _objectNormalTextureScale = value; }
        public UMI3DAsyncProperty<float> objectHeightTextureScale { get { Id(); return _objectHeightTextureScale; } protected set => _objectHeightTextureScale = value; }

        private UMI3DAsyncPropertyEquality pCompare = new UMI3DAsyncPropertyEquality();

        private UMI3DAsyncProperty<Color> _objectBaseColorFactor;
        private UMI3DAsyncProperty<float> _objectMetallicFactor;
        private UMI3DAsyncProperty<float> _objectRoughnessFactor;
        private UMI3DAsyncProperty<Color> _objectEmissiveFactor;
        private UMI3DAsyncProperty<UMI3DTextureResource> _objectMaintexture;
        private UMI3DAsyncProperty<UMI3DTextureResource> _objectMetallicRoughnessTexture;
        private UMI3DAsyncProperty<UMI3DScalableTextureResource> _objectNormalTexture;
        private UMI3DAsyncProperty<UMI3DTextureResource> _objectEmissiveTexture;
        private UMI3DAsyncProperty<UMI3DTextureResource> _objectOcclusionTexture;
        private UMI3DAsyncProperty<UMI3DTextureResource> _objectMetallicTexture;
        private UMI3DAsyncProperty<UMI3DTextureResource> _objectChannelTexture;
        private UMI3DAsyncProperty<UMI3DTextureResource> _objectRoughnessTexture;
        private UMI3DAsyncProperty<UMI3DScalableTextureResource> _objectHeightTexture;
        private UMI3DAsyncProperty<Vector2> _objectTextureTilingScale;
        private UMI3DAsyncProperty<Vector2> _objectTextureTilingOffset;
        private UMI3DAsyncProperty<float> _objectNormalTextureScale;
        private UMI3DAsyncProperty<float> _objectHeightTextureScale;
        private UMI3DAsyncDictionnaryProperty<string, object> _objectShaderProperties;

        ///<inheritdoc/>
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

            objectNormalTexture = new UMI3DAsyncProperty<UMI3DScalableTextureResource>(id, UMI3DPropertyKeys.NormalTexture, this.textures.normalTexture, (x, u) => { return x.ToDto(); });
            objectNormalTexture.OnValueChanged += (UMI3DScalableTextureResource t) => { textures.normalTexture = t; };

            objectEmissiveTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.EmissiveTexture, this.textures.emissiveTexture, (x, u) => { return x.ToDto(); });
            objectEmissiveTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.emissiveTexture = t; };

            objectOcclusionTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.OcclusionTexture, this.textures.occlusionTexture, (x, u) => { return x.ToDto(); });
            objectOcclusionTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.occlusionTexture = t; };

            objectMetallicTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.MetallicTexture, this.textures.metallicTexture, (x, u) => { return x.ToDto(); });
            objectMetallicTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.metallicTexture = t; };

            objectRoughnessTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.RoughnessTexture, this.textures.roughnessTexture, (x, u) => { return x.ToDto(); });
            objectRoughnessTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.roughnessTexture = t; };

            objectChannelTexture = new UMI3DAsyncProperty<UMI3DTextureResource>(id, UMI3DPropertyKeys.ChannelTexture, this.textures.channelTexture, (x, u) => { return x.ToDto(); });
            objectChannelTexture.OnValueChanged += (UMI3DTextureResource t) => { textures.channelTexture = t; };

            objectHeightTexture = new UMI3DAsyncProperty<UMI3DScalableTextureResource>(id, UMI3DPropertyKeys.HeightTexture, this.textures.heightTexture, (x, u) => { return x.ToDto(); });
            objectHeightTexture.OnValueChanged += (UMI3DScalableTextureResource t) => { textures.heightTexture = t; };


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

        ///<inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }


    }
    [System.Serializable]
    public class CustomTextures
    {
        public string id = null;
        // public string shaderName; // unused
        public UMI3DTextureResource baseColorTexture = new UMI3DTextureResource();
        public UMI3DTextureResource channelTexture = new UMI3DTextureResource();
        public UMI3DTextureResource metallicRoughnessTexture = new UMI3DTextureResource();
        public UMI3DScalableTextureResource normalTexture = new UMI3DScalableTextureResource();
        public UMI3DTextureResource emissiveTexture = new UMI3DTextureResource();
        public UMI3DTextureResource occlusionTexture = new UMI3DTextureResource();
        public UMI3DTextureResource metallicTexture = new UMI3DTextureResource();
        public UMI3DTextureResource roughnessTexture = new UMI3DTextureResource();
        public UMI3DScalableTextureResource heightTexture = new UMI3DScalableTextureResource();

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
                channelTexture = channelTexture.ToDto(),
                roughnessTexture = roughnessTexture.ToDto(),
            };
        }

    }
}