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
using System.Collections.Generic;

namespace umi3d.common.userCapture
{
    /// <summary>
    /// A request to inform about the current pose of the user.
    /// </summary>
    [Serializable]
    public class UserTrackingFrameDto : AbstractBrowserRequestDto
    {
        public List<BoneDto> bones;

        public SerializableVector3 position;

        public SerializableVector4 rotation;

        public SerializableVector3 scale;

        public float refreshFrequency;

        protected override uint GetOperationId() { return UMI3DOperationKeys.UserTrackingFrame; }

        public override (int, Func<byte[], int, int>) ToByteArray(params object[] parameters)
        {
            var fb = base.ToByteArray(parameters);
            var fbones = UMI3DNetworkingHelper.ToBytes(bones);

            int size = fbones.Item1 + UMI3DNetworkingHelper.GetSize(position) + UMI3DNetworkingHelper.GetSize(rotation) + UMI3DNetworkingHelper.GetSize(scale) + fb.Item1;
            Func<byte[], int, int> func = (b, i) =>
            {
                i += fb.Item2(b, i);
                i += fbones.Item2(b, i);
                i += UMI3DNetworkingHelper.Write(position, b, i);
                i += UMI3DNetworkingHelper.Write(rotation, b, i);
                i += UMI3DNetworkingHelper.Write(scale, b, i);
                i += UMI3DNetworkingHelper.Write(refreshFrequency, b, i);
                return size;
            };
            return (size, func);
        }
    }
}
