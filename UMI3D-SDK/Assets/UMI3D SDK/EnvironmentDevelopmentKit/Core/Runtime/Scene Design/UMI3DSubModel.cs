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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DSubModel : AbstractRenderedNode


    {

        public UMI3DModel parentModel;
        public string subModelName { get; protected set; }
        //    private UMI3DAsyncProperty<bool> _objectMaterialOverrided;
        [SerializeField, EditorReadOnly]
        protected bool ignoreModelMaterialOverride = false;
        public UMI3DAsyncProperty<bool> objectIgnoreModelMaterialOverride;


        /// <summary>
        /// If true, the mesh will be used for navmesh generation on the browser.
        /// </summary>
        public bool isPartOfNavmesh = false;

        /// <summary>
        /// Indicate whether or not the user is allowed to navigate through this object.
        /// </summary>
        public bool isTraversable = true;


        ///<inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            objectIgnoreModelMaterialOverride = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.IgnoreModelMaterialOverride, ignoreModelMaterialOverride);
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

        ///<inheritdoc/>
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
            subDto.ignoreModelMaterialOverride = ignoreModelMaterialOverride;
            subDto.isTraversable = isTraversable;
            subDto.isPartOfNavmesh = isPartOfNavmesh;
        }

        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(this.parentModel.Id())
                + UMI3DNetworkingHelper.Write(this.subModelName)
                + UMI3DNetworkingHelper.Write(this.ignoreModelMaterialOverride)
                + UMI3DNetworkingHelper.Write(this.isPartOfNavmesh)
                + UMI3DNetworkingHelper.Write(this.isTraversable);
        }
    }
}