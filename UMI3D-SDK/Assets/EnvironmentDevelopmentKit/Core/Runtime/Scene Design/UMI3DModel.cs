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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DModel : UMI3DNode
    {
        [Obsolete("will be removed soon")]
        public bool lockColliders = false;


        [SerializeField]
        UMI3DResource model = new UMI3DResource();
        public UMI3DAsyncProperty<UMI3DResource> objectModel { get { Register(); return _objectModel; } protected set => _objectModel = value; }

        [HideInInspector] public string idGenerator = "{{pid}}_[{{name}}]";

        public bool overrideModelMaterials = false;
        public List<MaterialOverrider> materialsOverider = new List<MaterialOverrider>();

        [Serializable]
        public class MaterialOverrider
        {
            public MaterialSO newMaterial;
            public List<string> overidedMaterials;

            public UMI3DMeshNodeDto.MaterialOverrideDto ToDto()
            {

                return new UMI3DMeshNodeDto.MaterialOverrideDto()
                {
                    newMaterialId = newMaterial.Id(),
                    overridedMaterialsId = overidedMaterials
                };
            }

        }

        // Should not be modified after init 
        public bool areSubobjectsTracked = false;

        public UMI3DAsyncProperty<bool> objectMaterialsOverrided { get { Register(); return _objectMaterialsOverrided; } protected set => _objectMaterialsOverrided = value; }
        public UMI3DAsyncListProperty<MaterialOverrider> objectMaterrialOveriders { get { Register(); return _objectMaterrialOveriders; } protected set => _objectMaterrialOveriders = value; }

        [SerializeField]
        protected bool castShadow = true;
        [SerializeField]
        protected bool receiveShadow = true;
        private UMI3DAsyncProperty<UMI3DResource> _objectModel;
        private UMI3DAsyncProperty<bool> _objectMaterialsOverrided;
        private UMI3DAsyncListProperty<MaterialOverrider> _objectMaterrialOveriders;
        private UMI3DAsyncProperty<bool> _objectCastShadow;
        private UMI3DAsyncProperty<bool> _objectReceiveShadow;

        public UMI3DAsyncProperty<bool> objectCastShadow { get { Register(); return _objectCastShadow; } protected set => _objectCastShadow = value; }
        public UMI3DAsyncProperty<bool> objectReceiveShadow { get { Register(); return _objectReceiveShadow; } protected set => _objectReceiveShadow = value; }


        protected override void InitDefinition(string id)
        {
            base.InitDefinition(id);

            objectMaterialsOverrided = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.IsMaterialOverided, this.overrideModelMaterials);
            objectMaterialsOverrided.OnValueChanged += (bool value) => overrideModelMaterials = value;

            objectMaterrialOveriders = new UMI3DAsyncListProperty<MaterialOverrider>(objectId, UMI3DPropertyKeys.OverideMaterialId, this.materialsOverider);//.ConvertAll((mat) => mat.ToDto()));

            objectMaterrialOveriders.OnInnerValueChanged += (int index, MaterialOverrider value) => Debug.LogError("not implemented");

            objectCastShadow = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.CastShadow, castShadow);
            objectReceiveShadow = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.ReceiveShadow, receiveShadow);

            if (areSubobjectsTracked)
            {
                SetSubHierarchy();
            }

            objectModel = new UMI3DAsyncProperty<UMI3DResource>(objectId, UMI3DPropertyKeys.Model, model, (r, u) => r.ToDto());


        }

        public void SetSubHierarchy()
        {
            if (idGenerator == null || idGenerator.Length < 1)
            {
                Debug.LogWarning("idGenerator is required");
                return;
            }

            //Debug.Log("add subobjects in hierarchy for " + gameObject.name);
            foreach (Transform child in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.GetComponent<UMI3DAbstractNode>() == null)
                {
                    UMI3DSubModel subModel = child.gameObject.AddComponent<UMI3DSubModel>();
                    subModel.parentModel = this;
                }
                else if (child.gameObject.GetComponent<UMI3DSubModel>() != null)
                {
                    UMI3DSubModel subModel = child.gameObject.GetComponent<UMI3DSubModel>();
                    subModel.parentModel = this;
                }
            }
        }


        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DMeshNodeDto();
        }

        /// <summary>
        /// Writte the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            UMI3DMeshNodeDto meshDto = dto as UMI3DMeshNodeDto;
            meshDto.mesh = objectModel.GetValue(user).ToDto();
            //   meshDto.isSubHierarchyAllowedToBeModified = isSubHierarchyAllowedToBeModified;
            meshDto.areSubobjectsTracked = areSubobjectsTracked;
            meshDto.idGenerator = idGenerator;
            meshDto.receiveShadow = objectReceiveShadow.GetValue(user);
            meshDto.castShadow = objectCastShadow.GetValue(user);

            if (this.overrideModelMaterials)
            {
                meshDto.overridedMaterials = materialsOverider.ConvertAll((mat) => mat.ToDto());

            }

        }

        internal override List<GlTFMaterialDto> GetGlTFMaterialsFor(UMI3DUser user)
        {

            return materialsOverider.ConvertAll(mat => mat.newMaterial.ToDto());

        }
    }

}
