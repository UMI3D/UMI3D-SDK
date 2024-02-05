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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Request to move a user in the clients.
    /// </summary>
    public class NavigationRequest : Operation
    {
        /// <summary>
        /// New positon of the user.
        /// </summary>
        public Vector3Dto position;

        public NavigationRequest(Vector3 position)
        {
            this.position = position.Dto();

        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            NavigateDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.NavigationRequest;
        }

        protected virtual NavigateDto CreateDto() { return new NavigateDto(); }
        protected virtual void WriteProperties(NavigateDto dto) { dto.position = position; }



        public override Bytable ToBytable(UMI3DUser user)
        {
            if (position == null) position = new Vector3Dto();
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(position);
        }
    }
}