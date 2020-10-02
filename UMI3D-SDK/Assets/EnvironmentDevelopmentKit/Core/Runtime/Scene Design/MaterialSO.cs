﻿/*
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

namespace umi3d.edk
{
    //[CreateAssetMenu(fileName = "Umi3DMaterial", menuName = "UMI3D/Umi3DMaterial")]
    public abstract class MaterialSO : ScriptableObject, UMI3DLoadableEntity
    {

        public enum AlphaMode
        {
            OPAQUE, MASK, BLEND
        }
        public AlphaMode alphaMode = AlphaMode.BLEND;

        protected abstract void OnEnable();

        protected abstract string GetId();

        protected abstract void SetId(string id);

        public abstract GlTFMaterialDto ToDto();

        public string Id()
        {
            return GetId();
        }
        protected abstract void InitDefinition(string id);

        protected void RegisterMaterial(AbstractEntityDto mat)
        {
            //   Debug.Log("try registered");
            if (string.IsNullOrEmpty(mat.id) || UMI3DEnvironment.GetEntity<MaterialSO>(mat.id) == null)
            {
                mat.id = UMI3DEnvironment.Register(this);
                SetId(mat.id);
                InitDefinition(mat.id);

                //      Debug.Log("registered");
            }
        }

        public abstract IEntity ToEntityDto(UMI3DUser user);
   
    }
}
