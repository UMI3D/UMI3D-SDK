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

using umi3d.common;

namespace umi3d.edk
{
    public class UMI3DSubModel : UMI3DNode

    {
        public UMI3DAsyncProperty<bool> objectMaterialOverrided { get { Register(); return _objectMaterialOverrided; } protected set => _objectMaterialOverrided = value; }

        public UMI3DModel parentModel;
        private UMI3DAsyncProperty<bool> _objectMaterialOverrided;


        /// <summary>
        /// Check if the AbstractObject3D has been registered to to the UMI3DScene and do it if not
        /// </summary>
        public override LoadEntity Register()
        {
            if (objectId == null && UMI3DEnvironment.Exists)
            {
                objectId = parentModel.idGenerator.Replace("{{name}}", gameObject.name).Replace("{{pid}}", parentModel.Id());

                InitDefinition(objectId);
            }
            return GetLoadEntity();
        }

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
        /// Writte the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            SubModelDto subDto = dto as SubModelDto;
            subDto.modelId = parentModel.Id();

        }

    }
}