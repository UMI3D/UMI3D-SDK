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

namespace umi3d.edk
{
    public class ServerMessageRequest : Operation
    {
        public string message;

        public ServerMessageRequest(string message, UMI3DUser user)
        {
            this.message = message;
            this.users = new() { user };
        }

        public ServerMessageRequest(string message, IEnumerable<UMI3DUser> users)
        {
            this.message = message;
            this.users = new(users);
        }

        /// <inheritdoc/>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.ServerMessageRequest;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(message);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            ServerMessageRequestDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }

        protected virtual ServerMessageRequestDto CreateDto() { return new ServerMessageRequestDto(); }
        protected virtual void WriteProperties(ServerMessageRequestDto dto) { dto.message = message; }
    }
}


