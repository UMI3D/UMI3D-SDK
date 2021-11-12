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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class NavigationRequest : DispatchableRequest
    {

        public SerializableVector3 position;

        public NavigationRequest(Vector3 position, bool reliable, HashSet<UMI3DUser> users = null) : base(reliable, users)
        {
            this.position = position;

        }

        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.NavigationRequest;
        }

        protected virtual Bytable ToBytable()
        {
            if (position == null) position = new SerializableVector3();
            return UMI3DNetworkingHelper.Write(GetOperationKey())
                + UMI3DNetworkingHelper.Write(position);
        }

        public override byte[] ToBytes()
        {
            return ToBytable().ToBytes();
        }

        public override byte[] ToBson()
        {
            NavigateDto dto = CreateDto();
            WriteProperties(dto);
            return dto.ToBson();
        }

        protected virtual NavigateDto CreateDto() { return new NavigateDto(); }
        protected virtual void WriteProperties(NavigateDto dto) { dto.position = position; }
    }
}