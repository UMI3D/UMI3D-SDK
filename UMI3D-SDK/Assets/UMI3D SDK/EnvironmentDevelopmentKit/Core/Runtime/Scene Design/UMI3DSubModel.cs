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

using inetum.unityUtils;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DSubModel : AbstractRenderedNode
    {
        /// <summary>
        /// Model that serves as a based for the submodel.
        /// </summary>
        [Tooltip("Model that serves as a based for the submodel.")]
        public UMI3DModel parentModel;

        /// <summary>
        /// Named label of the submodel.
        /// </summary>
        public string subModelName { get; protected set; }

        /// <summary>
        /// List of submodel name in hierachy from root to this subModel
        /// </summary>
        public List<string> subModelHierachyNames = new List<string>();/*{ get; set; }*/

        /// <summary>
        /// List of submodel index in hierachy from root to this subModel
        /// </summary>
        public List<int> subModelHierachyIndexes = new List<int>();/*{ get; set; }*/

        //    private UMI3DAsyncProperty<bool> _objectMaterialOverrided;
        /// <summary>
        /// If true, the submodel won't be impacted by a change of material on the model.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("If true, the submodel won't be impacted by a change of material on the model.")]
        protected bool ignoreModelMaterialOverride = false;
        /// <summary>
        /// See <see cref="ignoreModelMaterialOverride"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectIgnoreModelMaterialOverride;


        /// <summary>
        /// If true, the mesh will be used for navmesh generation on the browser.
        /// </summary>
        public bool isPartOfNavmesh = false;

        /// <summary>
        /// Property to change if th object is part of the navmesh. This property has priority over <see cref="objectTraversable"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectPartOfNavmesh { get { Register(); return _objectPartOfNavmesh; } protected set => _objectPartOfNavmesh = value; }

        private UMI3DAsyncProperty<bool> _objectPartOfNavmesh;

        /// <summary>
        /// Indicate whether or not the user is allowed to navigate through this object.
        /// </summary>
        public bool isTraversable = true;

        /// <summary>
        /// Property to set or not the object as traversable.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectTraversable { get { Register(); return _objectTraversable; } protected set => _objectTraversable = value; }

        private UMI3DAsyncProperty<bool> _objectTraversable;


        /// <summary>
        /// Should the user be allowed to navigate through this object?
        /// </summary>
        /// When set to true, it will alter the navigation mesh generation.
        [Tooltip("Should the user be allowed to navigate through this object?")]
        public bool isBlockingInteraction = true;

        /// <summary>
        /// Property to set or not the object as traversable.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectIsBlockingInteraction { get { Register(); return _objectIsBlockingInteraction; } protected set => _objectIsBlockingInteraction = value; }

        private UMI3DAsyncProperty<bool> _objectIsBlockingInteraction;

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            objectIgnoreModelMaterialOverride = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.IgnoreModelMaterialOverride, ignoreModelMaterialOverride);

            _objectPartOfNavmesh = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.IsPartOfNavmesh, isPartOfNavmesh);
            _objectPartOfNavmesh.OnValueChanged += b => isPartOfNavmesh = b;

            _objectTraversable = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.IsTraversable, isTraversable);
            _objectTraversable.OnValueChanged += b => isTraversable = b;

            _objectIsBlockingInteraction = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.IsBlockingInteraction, isBlockingInteraction);
            _objectIsBlockingInteraction.OnValueChanged += b => isBlockingInteraction = b;

            var skmr = GetComponent<SkinnedMeshRenderer>();
            if (skmr != null && skmr.sharedMesh.blendShapeCount > 0)
            {
                for (int i = 0; i < skmr.sharedMesh.blendShapeCount; i++)
                {
                    blendShapesValues.Add(skmr.GetBlendShapeWeight(i));
                }
                objectBlendShapesValues.SetValue(blendShapesValues);

            }
        }

        /// <summary>
        /// Check if the AbstractObject3D has been registered to to the UMI3DScene and do it if not
        /// </summary>
        public override void Register()
        {
            if (objectId == 0 && UMI3DEnvironment.Exists)
            {
                objectId = UMI3DEnvironment.Register(this);
                subModelName = gameObject.name;
                InitDefinition(objectId);
            }
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToGlTFNodeDto(user);
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override UMI3DNodeDto CreateDto()
        {
            return new SubModelDto();
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
            var subDto = dto as SubModelDto;
            subDto.modelId = parentModel.Id();
            subDto.subModelName = subModelName;
            subDto.subModelHierachyIndexes = subModelHierachyIndexes;
            subDto.subModelHierachyNames = subModelHierachyNames;
            subDto.ignoreModelMaterialOverride = ignoreModelMaterialOverride;
            subDto.isTraversable = isTraversable;
            subDto.isPartOfNavmesh = isPartOfNavmesh;
            subDto.isBlockingInteraction = isBlockingInteraction;
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DSerializer.Write(this.parentModel.Id())
                + UMI3DSerializer.Write(this.subModelName)
                + UMI3DSerializer.Write(this.subModelHierachyIndexes)
                + UMI3DSerializer.Write(this.subModelHierachyNames)
                + UMI3DSerializer.Write(this.ignoreModelMaterialOverride)
                + UMI3DSerializer.Write(this.isPartOfNavmesh)
                + UMI3DSerializer.Write(this.isTraversable);
        }
    }
}