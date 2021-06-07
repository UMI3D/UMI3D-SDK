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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    [CreateAssetMenu(fileName = "Umi3D_Original_Material", menuName = "UMI3D/Umi3D_Original_Material")]
    public class OriginalMaterial : MaterialSO
    {

        public ulong matId;


        private bool registered = false;

        public override Bytable ToBytes(UMI3DUser user)
        {
            throw new NotImplementedException();
        }

        public override GlTFMaterialDto ToDto()
        {
            var res = new GlTFMaterialDto();
            res.extensions = new GlTFMaterialExtensions()
            { umi3d = new UMI3DOriginalMaterialDto() };
            ((UMI3DOriginalMaterialDto)res.extensions.umi3d).id = GetId();

            ((UMI3DOriginalMaterialDto)res.extensions.umi3d).shaderProperties = shaderProperties;
            return res;
        }

        ///<inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        ///<inheritdoc/>
        protected override ulong GetId()
        {
            if (!registered)
            {
                UMI3DOriginalMaterialDto matDto = new UMI3DOriginalMaterialDto();
                RegisterMaterial(matDto);
            }
            return matId;
        }

        ///<inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            Debug.Log("id original mat " + id);
            objectShaderProperties = new UMI3DAsyncDictionnaryProperty<string, object>(id, UMI3DPropertyKeys.ShaderProperties, this.shaderProperties, null, null, null, (d) =>
            {
                return new Dictionary<string, object>(d);
            });
            //   objectShaderProperties.OnInnerValueChanged += (string s, object o) => { shaderProperties[s] = o; };
            //      objectShaderProperties.OnInnerValueAdded += (string s, object o) => { shaderProperties.Add(s, o); };
            //    objectShaderProperties.OnInnerValueRemoved += (string s) => { shaderProperties.Remove(s); };
            objectShaderProperties.OnValueChanged += (Dictionary<string, object> d) => { shaderProperties = d; };
        }

        protected override void OnEnable()
        {

            matId = 0;
            registered = false;
        }

        ///<inheritdoc/>
        protected override void SetId(ulong id)
        {
            registered = true;
            matId = id;
        }


    }
}
