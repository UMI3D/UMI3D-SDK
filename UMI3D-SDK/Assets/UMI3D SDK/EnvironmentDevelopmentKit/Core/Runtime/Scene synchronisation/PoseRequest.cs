using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.edk
{
    public class PoseRequest : Operation
    {

        public ulong poseKey;
        public int indexInList;
        public bool stopPose;

        public PoseRequest(ulong poseKey ,int indexInList, bool stopPose = false)
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
            PlayPoseDto dto = CreateDto();
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

        protected virtual PlayPoseDto CreateDto() { return new PlayPoseDto(); }
        protected virtual void WriteProperties(PlayPoseDto dto, ulong userID) { dto.userID = userID; dto.indexInList = indexInList; dto.poseKey = poseKey; dto.stopPose = stopPose; }
    }
}
