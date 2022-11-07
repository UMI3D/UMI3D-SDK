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
    /// <summary>
    /// 3D model, representation of an object with rendered sufaces.
    /// </summary>
    public partial class UMI3DModel : AbstractRenderedNode
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Core;

        [Obsolete("will be removed soon")]
        public bool lockColliders = false;

        /// <summary>
        /// Model's ressource.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Model's ressource.")]
        private UMI3DResource model = new UMI3DResource();
        /// <summary>
        /// <see cref="model"/>.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DResource> objectModel { get { Register(); return _objectModel; } protected set => _objectModel = value; }

        [HideInInspector] public string idGenerator = "{{pid}}_[{{name}}]";

        /// <summary>
        /// Should the subobjects considered as seperated nodes?
        /// </summary>
        /// Should not be modified after init
        [Tooltip("Should the subobjects considered as seperated nodes?")]
        public bool areSubobjectsTracked = false;
        /// <summary>
        /// Have submodels already been added under this model?
        /// </summary>
        /// Set to true after calling <see cref="SetSubHierarchy"/>.
        [Tooltip("Have submodels already been added under this model?")]
        public bool areSubobjectsAlreadyMarked = false;

        /// <summary>
        /// Is the model in right hand coordinates?
        /// </summary>
        /// Should not be modified after init 
        [Tooltip("Is the model in right hand coordinates?")]
        public bool isRightHanded = true;

        /// <summary>
        /// Should this mesh be used for navmesh generation on the browser?
        /// </summary>
        [Tooltip("Should this mesh be used for navmesh generation on the browser?")]
        public bool isPartOfNavmesh = false;

        /// <summary>
        /// Should the user be allowed to navigate through this object?
        /// </summary>
        /// When set to true, it will alter the navigation mesh generation.
        [Tooltip("Should the user be allowed to navigate through this object?")]
        public bool isTraversable = true;

        /// <summary>
        /// <see cref="model"/>.
        /// </summary>
        private UMI3DAsyncProperty<UMI3DResource> _objectModel;

        /// <inheritdoc/>
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

            objectModel = new UMI3DAsyncProperty<UMI3DResource>(objectId, UMI3DPropertyKeys.Model, model, (r, u) => r?.ToDto());
            objectModel.OnValueChanged += v => model = v;
        }

        /// <summary>
        /// Add automatically <see cref="UMI3DSubModel"/> nodes on all subobjects of the model that possess a renderer, and <see cref="UMI3DNode"/> on others.
        /// </summary>
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

        private class GameObjectInfo
        {
            public GameObjectInfo parent;
            public GameObject gameObject;
            public int index;
            private List<int> indexes;
            private List<string> names;

            public List<string> getNames()
            {
                if (names == null)
                {
                    names = new List<string>();
                    if (parent?.names != null)
                    {
                        names.AddRange(parent?.getNames());
                        names?.Add(gameObject.name);
                    }
                    else
                    {
                        names.Add(gameObject.name);
                    }
                }
                return names;
            }
            public List<int> getIndexes()
            {
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
            var result = new List<GameObjectInfo>();
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

        /// <inheritdoc/>
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
            meshDto.isRightHanded = !areSubobjectsTracked || isRightHanded;
            meshDto.idGenerator = idGenerator;
            meshDto.isPartOfNavmesh = isPartOfNavmesh;
            meshDto.isTraversable = isTraversable;
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(areSubobjectsTracked)
                + UMI3DNetworkingHelper.Write(!areSubobjectsTracked || isRightHanded)
                + UMI3DNetworkingHelper.Write(idGenerator)
                + UMI3DNetworkingHelper.Write(isPartOfNavmesh)
                + UMI3DNetworkingHelper.Write(isTraversable)
                + objectModel.GetValue(user).ToByte();
        }

        /// <inheritdoc/>
        internal override List<GlTFMaterialDto> GetGlTFMaterialsFor(UMI3DUser user)
        {

            return materialsOverrider.ConvertAll(mat => mat.newMaterial.ToDto());

        }
    }
}
