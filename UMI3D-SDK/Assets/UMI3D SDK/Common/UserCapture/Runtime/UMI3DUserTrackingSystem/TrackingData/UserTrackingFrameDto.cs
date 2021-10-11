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
        public ulong userId;

        public List<BoneDto> bones;

        public float skeletonHighOffset;

        public SerializableVector3 position;

        public SerializableVector4 rotation;

        public float refreshFrequency;

        protected override uint GetOperationId() { return UMI3DOperationKeys.UserTrackingFrame; }

        public override Bytable ToBytableArray(params object[] parameters)
        {
            return base.ToBytableArray(parameters)
                + UMI3DNetworkingHelper.Write(userId)
                + UMI3DNetworkingHelper.Write(skeletonHighOffset)
                + UMI3DNetworkingHelper.Write(position)
                + UMI3DNetworkingHelper.Write(rotation)
                + UMI3DNetworkingHelper.Write(refreshFrequency)
                + UMI3DNetworkingHelper.WriteIBytableCollection(bones);
        }
    }
}
