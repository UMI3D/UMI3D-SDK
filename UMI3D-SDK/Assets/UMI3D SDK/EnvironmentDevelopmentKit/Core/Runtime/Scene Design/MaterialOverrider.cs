﻿/*
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
    [Serializable]
    public class MaterialOverrider : IBytable
    {
        public MaterialSO newMaterial;
        [SerializeField]
        private OverridedMaterialList materialListToOverride = new OverridedMaterialList();

        public bool overrideAllMaterial { get => materialListToOverride.overrideAllMaterial; set => materialListToOverride.overrideAllMaterial = value; }

        public bool addMaterialIfNotExists { get => materialListToOverride.addMaterialIfNotExists; set => materialListToOverride.addMaterialIfNotExists = value; }

        public List<string> overidedMaterials { get => materialListToOverride.overidedMaterials; set => materialListToOverride.overidedMaterials = value; }

        [Serializable]
        public class OverridedMaterialList
        {
            public bool overrideAllMaterial = false;
            public bool addMaterialIfNotExists = false;
            public List<string> overidedMaterials = new List<string>();
        }

        private static readonly List<string> ANY_mat = new List<string>() { "ANY_mat" };

        public UMI3DRenderedNodeDto.MaterialOverrideDto ToDto()
        {
            if (overrideAllMaterial)
                return new UMI3DRenderedNodeDto.MaterialOverrideDto()
                {
                    newMaterialId = newMaterial.Id(),
                    overridedMaterialsId = ANY_mat,
                    addMaterialIfNotExists = addMaterialIfNotExists
                };

            return new UMI3DRenderedNodeDto.MaterialOverrideDto()
            {
                newMaterialId = newMaterial.Id(),
                overridedMaterialsId = overidedMaterials,
                addMaterialIfNotExists = addMaterialIfNotExists
            };
        }

        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            return UMI3DNetworkingHelper.Write(newMaterial.Id())
                + UMI3DNetworkingHelper.Write(addMaterialIfNotExists)
                + UMI3DNetworkingHelper.WriteCollection((overrideAllMaterial ? ANY_mat : overidedMaterials));
        }

        bool IBytable.IsCountable()
        {
            return false;
        }
    }


}
