/*
Copyright 2019 - 2023 Inetum

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
using umi3d.common.userCapture.pose;

namespace umi3d.edk.userCapture.pose
{
    public class PoseRequest : Operation
    {
        public ulong poseKey;
        public int indexInList;
        public bool stopPose;

        public PoseRequest(ulong poseKey, int indexInList, bool stopPose = false)
        {
            this.poseKey = poseKey;
            this.indexInList = indexInList;
            this.stopPose = stopPose;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(poseKey)
                + UMI3DSerializer.Write(indexInList)
                + UMI3DSerializer.Write(stopPose);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            ApplyPoseDto dto = CreateDto();
            WriteProperties(dto, user.Id());
            return dto;
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.PlayPoseRequest;
        }

        protected virtual ApplyPoseDto CreateDto()
        { return new ApplyPoseDto(); }

        protected virtual void WriteProperties(ApplyPoseDto dto, ulong userID)
        { dto.userID = userID; dto.indexInList = indexInList; dto.poseKey = poseKey; dto.stopPose = stopPose; }
    }
}