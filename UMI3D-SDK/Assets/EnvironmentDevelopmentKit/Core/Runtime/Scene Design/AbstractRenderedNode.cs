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
    public abstract class AbstractRenderedNode : UMI3DNode
    {
        public bool overrideModelMaterials = false;
        public List<MaterialOverrider> materialsOverrider = new List<MaterialOverrider>();


        public UMI3DAsyncProperty<bool> objectMaterialsOverrided { get { Register(); return _objectMaterialsOverrided; } protected set => _objectMaterialsOverrided = value; }
        public UMI3DAsyncListProperty<MaterialOverrider> objectMaterialOverriders { get { Register(); return _objectMaterialOverriders; } protected set => _objectMaterialOverriders = value; }

        [SerializeField]
        protected bool castShadow = true;
        [SerializeField]
        protected bool receiveShadow = true;


        protected UMI3DAsyncProperty<bool> _objectMaterialsOverrided;
        protected UMI3DAsyncListProperty<MaterialOverrider> _objectMaterialOverriders;
        protected UMI3DAsyncProperty<bool> _objectCastShadow;
        protected UMI3DAsyncProperty<bool> _objectReceiveShadow;


        public UMI3DAsyncProperty<bool> objectCastShadow { get { Register(); return _objectCastShadow; } protected set => _objectCastShadow = value; }
        public UMI3DAsyncProperty<bool> objectReceiveShadow { get { Register(); return _objectReceiveShadow; } protected set => _objectReceiveShadow = value; }


        protected override void InitDefinition(string id)
        {
            base.InitDefinition(id);

            objectMaterialsOverrided = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.ApplyCustomMaterial, this.overrideModelMaterials);
            objectMaterialsOverrided.OnValueChanged += (bool value) => overrideModelMaterials = value;

            objectMaterialOverriders = new UMI3DAsyncListProperty<MaterialOverrider>(objectId, UMI3DPropertyKeys.OverideMaterialId, this.materialsOverrider, (x, u) => x.ToDto(), (a, b) => { return a.GetHashCode().Equals(b.GetHashCode()); });



            //     objectMaterialOverriders.OnInnerValueChanged += (int index, MaterialOverrider value) => materialsOverrider[index] = value;
            //   objectMaterialOverriders.OnInnerValueRemoved += (int index, MaterialOverrider value) => materialsOverrider.RemoveAt(index);
            objectMaterialOverriders.OnInnerValueAdded += (int index, MaterialOverrider value) => { Debug.Log("add inner value in matOverriders"); /* materialsOverrider.Add(value); */};
            //     objectMaterialOverriders.OnValueChanged += (List<MaterialOverrider> value) => materialsOverrider = value;




            objectCastShadow = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.CastShadow, castShadow);
            objectReceiveShadow = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.ReceiveShadow, receiveShadow);

        }


        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DRenderedNodeDto();
           // rajouter un abstract dto 
        }

        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            UMI3DRenderedNodeDto meshDto = dto as UMI3DRenderedNodeDto;

            meshDto.receiveShadow = objectReceiveShadow.GetValue(user);
            meshDto.castShadow = objectCastShadow.GetValue(user);

            meshDto.applyCustomMaterial = overrideModelMaterials;
            meshDto.overridedMaterials = materialsOverrider.ConvertAll((mat) => mat.ToDto());
        }

    }


}