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
    public partial class UMI3DModel : AbstractRenderedNode
    {
        [Obsolete("will be removed soon")]
        public bool lockColliders = false;


        [SerializeField, EditorReadOnly]
        UMI3DResource model = new UMI3DResource();
        public UMI3DAsyncProperty<UMI3DResource> objectModel { get { Register(); return _objectModel; } protected set => _objectModel = value; }

        [HideInInspector] public string idGenerator = "{{pid}}_[{{name}}]";

        // Should not be modified after init 
        public bool areSubobjectsTracked = false;

        // Should not be modified after init 
        public bool isRightHanded = true;

        /// <summary>
        /// If true, the mesh will be used for navmesh generation on the browser.
        /// </summary>
        public bool isPartOfNavmesh = false;

        /// <summary>
        /// Indicate whether or not the user is allowed to navigate through this object.
        /// </summary>
        public bool isTraversable = true;


        private UMI3DAsyncProperty<UMI3DResource> _objectModel;

        ///<inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            if (areSubobjectsTracked)
            {
                SetSubHierarchy();
            }

            objectModel = new UMI3DAsyncProperty<UMI3DResource>(objectId, UMI3DPropertyKeys.Model, model, (r, u) => r.ToDto());
            objectModel.OnValueChanged += v => model = v;
        }

        public void SetSubHierarchy()
        {
            if (idGenerator == null || idGenerator.Length < 1)
            {
                Debug.LogWarning("idGenerator is required");
                return;
            }

            //Debug.Log("add subobjects in hierarchy for " + gameObject.name);
            foreach (GameObject child in GetSubModelGameObjectOfUMI3DModel(gameObject.transform))
            {
                if (child.gameObject.GetComponent<UMI3DAbstractNode>() == null)
                {
                    if (child.gameObject.GetComponent<Renderer>() != null)
                    {
                        UMI3DSubModel subModel = child.gameObject.AddComponent<UMI3DSubModel>();
                        subModel.parentModel = this;
                        subModel.objectCastShadow.SetValue(this.castShadow);
                        subModel.objectReceiveShadow.SetValue(this.receiveShadow);
                    }
                    else if (child.gameObject.GetComponent<ReflectionProbe>() != null)
                    {
                        UMI3DSubModel subModel = child.gameObject.AddComponent<UMI3DSubModel>();
                        subModel.parentModel = this;
                    }
                    else
                    {
                        UMI3DNode node = child.gameObject.AddComponent<UMI3DNode>();
                    }
                }
                else if (child.gameObject.GetComponent<UMI3DSubModel>() != null)
                {
                    UMI3DSubModel subModel = child.gameObject.GetComponent<UMI3DSubModel>();
                    subModel.parentModel = this;

                }
            }
        }

        public List<GameObject> GetSubModelGameObjectOfUMI3DModel(Transform modelRoot)
        {
            var res = GetChildrenWhithoutOtherModel(modelRoot);
            if (modelRoot.GetComponent<Renderer>() != null)
                res.Add(modelRoot.gameObject);
            return res;
        }

        private List<GameObject> GetChildrenWhithoutOtherModel(Transform tr)
        {
            var res = new List<GameObject>();
            for (int i = 0; i < tr.childCount; i++)

            {
                var child = tr.GetChild(i);

                if (!child.GetComponent<UMI3DModel>())
                {
                    res.Add(child.gameObject);
                    res.AddRange(GetChildrenWhithoutOtherModel(child));
                }
            }
            return res;
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
            meshDto.isRightHanded = areSubobjectsTracked ? isRightHanded : true;
            meshDto.idGenerator = idGenerator;
            meshDto.isPartOfNavmesh = isPartOfNavmesh;
            meshDto.isTraversable = isTraversable;
        }

        public override (int, Func<byte[], int, int>) ToBytes(UMI3DUser user)
        {
            var fm = objectModel.GetValue(user).ToByte();
            var fp = base.ToBytes(user);

            int size = 4 * sizeof(bool)
                + UMI3DNetworkingHelper.GetSize(idGenerator)
                + fm.Item1
                + fp.Item1;
            Func<byte[], int, int> func = (b, i) => {
                i += UMI3DNetworkingHelper.Write(areSubobjectsTracked, b, i);
                i += UMI3DNetworkingHelper.Write(areSubobjectsTracked ? isRightHanded : true, b, i);
                i += UMI3DNetworkingHelper.Write(idGenerator, b, i);
                i += UMI3DNetworkingHelper.Write(isPartOfNavmesh, b, i);
                i += UMI3DNetworkingHelper.Write(isTraversable, b, i);
                i += fm.Item2(b, i);
                i += fp.Item2(b, i);
                return size;
            };
            return (size, func);
        }

        ///<inheritdoc/>
        internal override List<GlTFMaterialDto> GetGlTFMaterialsFor(UMI3DUser user)
        {

            return materialsOverrider.ConvertAll(mat => mat.newMaterial.ToDto());

        }
    }

}
