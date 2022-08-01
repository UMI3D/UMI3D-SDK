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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public partial class UMI3DModel : AbstractRenderedNode
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Core;

        [Obsolete("will be removed soon")]
        public bool lockColliders = false;


        [SerializeField, EditorReadOnly]
        private UMI3DResource model = new UMI3DResource();
        public UMI3DAsyncProperty<UMI3DResource> objectModel { get { Register(); return _objectModel; } protected set => _objectModel = value; }

        [HideInInspector] public string idGenerator = "{{pid}}_[{{name}}]";

        // Should not be modified after init 
        public bool areSubobjectsTracked = false;
        /// <summary>
        /// State if submodel have already been added under this model.
        /// </summary>
        public bool areSubobjectsAlreadyMarked = false;

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
                areSubobjectsAlreadyMarked = true;

                SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    for (int i = 0; i < skinnedMeshRenderer.bones.Length; i++)
                    {
                        if (skinnedMeshRenderer.bones[i].TryGetComponent(out UMI3DNode node))
                        {
                            node.skinnedRendererLinks.Add(skinnedMeshRenderer.gameObject.GetComponent<UMI3DNode>().Id(), i);
                        }
                    }
                }
            }

            objectModel = new UMI3DAsyncProperty<UMI3DResource>(objectId, UMI3DPropertyKeys.Model, model, (r, u) => r.ToDto());
            objectModel.OnValueChanged += v => model = v;
        }

        public void SetSubHierarchy()
        {
            if (idGenerator == null || idGenerator.Length < 1)
            {
                UMI3DLogger.LogWarning("idGenerator is required", scope);
                return;
            }

            //UMI3DLogger.Log("add subobjects in hierarchy for " + gameObject.name,scope);
            foreach (GameObjectInfo child in GetSubModelGameObjectOfUMI3DModel(gameObject.transform))
            {
                if (child.parent == null) continue;

                if (child.gameObject.GetComponent<UMI3DAbstractNode>() == null)
                {
                    if (!areSubobjectsAlreadyMarked)
                        if (child.gameObject.GetComponent<Renderer>() != null)
                        {
                            UMI3DSubModel subModel = child.gameObject.AddComponent<UMI3DSubModel>();

                            subModel.parentModel = this;

                            subModel.subModelHierachyIndexes = child.getIndexes();
                            subModel.subModelHierachyNames = child.getNames();

                            subModel.objectCastShadow.SetValue(this.castShadow);
                            subModel.objectReceiveShadow.SetValue(this.receiveShadow);
                        }
                        else if (child.gameObject.GetComponent<ReflectionProbe>() != null)
                        {
                            UMI3DSubModel subModel = child.gameObject.AddComponent<UMI3DSubModel>();
                            subModel.parentModel = this;
                            subModel.subModelHierachyIndexes = child.getIndexes();
                            subModel.subModelHierachyNames = child.getNames();
                        }
                        else
                        {
                            child.gameObject.AddComponent<UMI3DNode>();
                        }
                }
                else if (child.gameObject.GetComponent<UMI3DSubModel>() != null)
                {
                    UMI3DSubModel subModel = child.gameObject.GetComponent<UMI3DSubModel>();

                    subModel.parentModel = this;
                    subModel.subModelHierachyIndexes = child.getIndexes();
                    subModel.subModelHierachyNames = child.getNames();
                }
            }
        }

        class GameObjectInfo
        {
            public GameObjectInfo parent;
            public GameObject gameObject;
            public int index;

            List<int> indexes;
            List<string> names;

            public List<string> getNames()
            {
                if (names == null)
                {
                    if (parent?.names != null)
                    {
                        names = parent?.getNames();
                        names?.Add(gameObject.name);
                    }
                    else
                    {
                        names = new List<string>();
                        names.Add(gameObject.name);
                    }
                }
                return names;
            }
            public List<int> getIndexes()
            {
                Debug.Log(indexes);
                if (indexes == null)
                {
                    indexes = new List<int>();
                    if (parent?.indexes != null)
                    {
                        indexes.AddRange(parent?.getIndexes());
                        indexes.Add(index);
                    }
                    else
                    {
                        indexes.Add(index);
                    }

                }
                return indexes;
            }

            public GameObjectInfo(GameObjectInfo parent, GameObject node, int index)
            {
                this.parent = parent;
                this.index = index;
                this.gameObject = node;
            }

            public GameObjectInfo(GameObject root)
            {
                this.parent = null;
                this.index = -1;
                this.gameObject = root;
            }

        }

        private List<GameObjectInfo> GetSubModelGameObjectOfUMI3DModel(Transform modelRoot)
        {
            List<GameObjectInfo> result = new List<GameObjectInfo>();
            var root = new GameObjectInfo(modelRoot.gameObject);
            if (modelRoot.GetComponent<Renderer>() != null)
                result.Add(root);
            GetChildrenWhithoutOtherModel(modelRoot, root, result);

            return result;
        }

        private void GetChildrenWhithoutOtherModel(Transform tr, GameObjectInfo parent, List<GameObjectInfo> result)
        {
            for (int i = 0; i < tr.childCount; i++)
            {
                Transform child = tr.GetChild(i);

                if (!child.GetComponent<UMI3DModel>())
                {
                    var node = new GameObjectInfo(parent, child.gameObject, i);
                    result.Add(node);
                    GetChildrenWhithoutOtherModel(child, node, result);
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
        /// Write the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var meshDto = dto as UMI3DMeshNodeDto;
            meshDto.mesh = objectModel.GetValue(user).ToDto();
            //   meshDto.isSubHierarchyAllowedToBeModified = isSubHierarchyAllowedToBeModified;
            meshDto.areSubobjectsTracked = areSubobjectsTracked;
            meshDto.isRightHanded = areSubobjectsTracked ? isRightHanded : true;
            meshDto.idGenerator = idGenerator;
            meshDto.isPartOfNavmesh = isPartOfNavmesh;
            meshDto.isTraversable = isTraversable;
        }

        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(areSubobjectsTracked)
                + UMI3DNetworkingHelper.Write(areSubobjectsTracked ? isRightHanded : true)
                + UMI3DNetworkingHelper.Write(idGenerator)
                + UMI3DNetworkingHelper.Write(isPartOfNavmesh)
                + UMI3DNetworkingHelper.Write(isTraversable)
                + objectModel.GetValue(user).ToByte();
        }

        ///<inheritdoc/>
        internal override List<GlTFMaterialDto> GetGlTFMaterialsFor(UMI3DUser user)
        {

            return materialsOverrider.ConvertAll(mat => mat.newMaterial.ToDto());

        }
    }

}
