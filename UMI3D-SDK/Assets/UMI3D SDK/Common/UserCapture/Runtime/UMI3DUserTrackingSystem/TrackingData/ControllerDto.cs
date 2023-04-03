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

namespace umi3d.common.userCapture
{
    [Serializable]
    public class ControllerDto : BoneDto, IBytable
    {
        /// <summary>
        /// Position relative to the tracked node.
        /// </summary>
        public SerializableVector3 position;

        /// <inheritdoc/>
        bool IBytable.IsCountable()
        {
            return true;
        }

        /// <inheritdoc/>
        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            return
                UMI3DSerializer.Write(boneType)
                + UMI3DSerializer.Write(rotation ?? new SerializableVector4())
                + UMI3DSerializer.Write(position ?? new SerializableVector3());
        }
    }
}
