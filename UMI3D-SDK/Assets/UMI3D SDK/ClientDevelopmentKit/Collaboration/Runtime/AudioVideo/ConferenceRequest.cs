using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public static class ConferenceRequest
    {
        public static ConferenceBrowserRequestDto GetChangeMicrophoneStatusRequest(ulong userId, bool value)
        {
            return new ConferenceBrowserRequestDto()
            {
                operation = UMI3DOperationKeys.UserMicrophoneStatus, 
                id = userId,value = value
            };
        }
        public static ConferenceBrowserRequestDto GetChangeAvatarStatusRequest(ulong userId, bool value)
        {
            return new ConferenceBrowserRequestDto()
            {
                operation = UMI3DOperationKeys.UserAvatarStatus,
                id = userId,
                value = value
            };
        }
        public static ConferenceBrowserRequestDto GetChangeAttentionStatusRequest(ulong userId, bool value)
        {
            return new ConferenceBrowserRequestDto()
            {
                operation = UMI3DOperationKeys.UserAttentionStatus,
                id = userId,
                value = value
            };
        }

        public static ConferenceBrowserRequestDto GetMuteAllMicrophoneRequest()
        {
            return new ConferenceBrowserRequestDto() { 
                operation = UMI3DOperationKeys.MuteAllMicrophoneStatus,
                value = false,
                id = 0 
            };
        }
        public static ConferenceBrowserRequestDto GetMuteAllAvatarRequest()
        {
            return new ConferenceBrowserRequestDto() {
                operation = UMI3DOperationKeys.MuteAllAvatarStatus,
                value = false,
                id = 0 
            };
        }
        public static ConferenceBrowserRequestDto GetMuteAllAttentionRequest()
        {
            return new ConferenceBrowserRequestDto() {
                operation = UMI3DOperationKeys.MuteAllAvatarStatus,
                value = false,
                id = 0 
            };
        }
    }
}