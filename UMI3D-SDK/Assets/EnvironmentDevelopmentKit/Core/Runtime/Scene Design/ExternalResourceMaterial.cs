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
    [CreateAssetMenu(fileName = "Umi3D_External_Material", menuName = "UMI3D/Umi3D_External_Material")]
    public class ExternalResourceMaterial : MaterialSO
    {

        public string matId;
        public UMI3DResource resource;

        public Dictionary<string, object> shaderProperties = new Dictionary<string, object>();
        public UMI3DAsyncDictionnaryProperty<string, object> objectShaderProperties { get { Id(); return _objectShaderProperties; } protected set => _objectShaderProperties = value; }

        private UMI3DAsyncDictionnaryProperty<string, object> _objectShaderProperties;

        ///<inheritdoc/>
        public override GlTFMaterialDto ToDto()
        {
            var res = new GlTFMaterialDto();
            res.extensions = new GlTFMaterialExtensions()
            { umi3d = new ExternalMaterialDto() };
            ((ExternalMaterialDto)res.extensions.umi3d).resource = resource.ToDto();
            ((ExternalMaterialDto)res.extensions.umi3d).id = GetId();
            ((ExternalMaterialDto)res.extensions.umi3d).shaderProperties = shaderProperties;


            return res;
        }

        ///<inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        private bool registered = false;

        ///<inheritdoc/>
        protected override string GetId()
        {
            if (!registered)
            {
                ExternalMaterialDto matDto = new ExternalMaterialDto()
                {
                    resource = resource.ToDto(),
                };
                RegisterMaterial(matDto);
            }
            return matId;
        }

        ///<inheritdoc/>
        protected override void InitDefinition(string id)
        {
            Debug.Log("id external mat " + id);
            objectShaderProperties = new UMI3DAsyncDictionnaryProperty<string, object>(id, UMI3DPropertyKeys.ShaderProperties, this.shaderProperties, null, null, null, (d) =>
            {
                return new Dictionary<string, object>(d);
            });
            //   objectShaderProperties.OnInnerValueChanged += (string s, object o) => { shaderProperties[s] = o; };
            //      objectShaderProperties.OnInnerValueAdded += (string s, object o) => { shaderProperties.Add(s, o); };
            //    objectShaderProperties.OnInnerValueRemoved += (string s) => { shaderProperties.Remove(s); };
            objectShaderProperties.OnValueChanged += (Dictionary<string, object> d) => { shaderProperties = d; };
        }

        ///<inheritdoc/>
        protected override void OnEnable()
        {
            matId = null;
            registered = false;
        }

        ///<inheritdoc/>
        protected override void SetId(string id)
        {
            registered = true;
            matId = id;
        }
    }
}