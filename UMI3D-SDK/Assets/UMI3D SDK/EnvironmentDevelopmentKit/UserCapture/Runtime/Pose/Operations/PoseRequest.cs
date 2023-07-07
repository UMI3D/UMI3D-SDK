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
    /// <summary>
    /// An operation to tell a client to play a specific pose
    /// </summary>
    public class PoseRequest : Operation
    {
        /// <summary>
        /// The index of the pose in the array
        /// </summary>
        public int indexInList;
        /// <summary>
        /// True stops the pose, false starts the pose
        /// </summary>
        public bool stopPose;

        public PoseRequest(int indexInList, bool stopPose = false)
        {
            this.indexInList = indexInList;
            this.stopPose = stopPose;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
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
        { dto.userID = userID; dto.indexInList = indexInList; dto.stopPose = stopPose; }
    }
}