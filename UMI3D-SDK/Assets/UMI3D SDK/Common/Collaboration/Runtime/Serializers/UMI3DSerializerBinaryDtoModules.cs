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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.common
{
    [UMI3DSerializerOrder(1000)]
    public class UMI3DSerializerBinaryDtoModules : UMI3DSerializerModule<BinaryDto>
    {
        public bool IsCountable()
        {
            return true;
        }

        public bool Read(ByteContainer container, out bool readable, out BinaryDto result)
        {
            var b = new BinaryDto();

            b.groupId = UMI3DSerializer.Read<int>(container);
            b.data = UMI3DSerializer.ReadArray<byte>(container);
            readable = true;
            result = b;
            return true;

        }

        public bool Write(BinaryDto value, out Bytable bytable, params object[] parameters)
        {
            bytable = UMI3DSerializer.Write(value.groupId);
            bytable += UMI3DSerializer.WriteCollection(value.data ?? new byte[0]);
            return true;
        }
    }
}