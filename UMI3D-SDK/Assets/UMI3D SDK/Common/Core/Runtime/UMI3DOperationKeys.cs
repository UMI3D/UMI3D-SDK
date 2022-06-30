/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License; Version 2.0 (the );
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing; software
distributed under the License is distributed on an  BASIS;
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND; either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace umi3d.common
{
    public static class UMI3DOperationKeys
    {
        public const uint Transaction = 1;
        public const uint LoadEntity = 2;
        public const uint DeleteEntity = 3;
        public const uint NavigationRequest = 4;
        public const uint TeleportationRequest = 5;
        public const uint UploadFileRequest = 6;
        public const uint GetLocalInfoRequest = 7;
        public const uint RedirectionRequest = 8;

        public const uint UserMicrophoneStatus = 21;
        public const uint UserAvatarStatus = 22;
        public const uint UserAttentionStatus = 23;
        public const uint MuteAllMicrophoneStatus = 24;
        public const uint MuteAllAvatarStatus = 25;
        public const uint MuteAllAttentionStatus = 26;

        public const uint SetEntityProperty = 101;
        public const uint SetEntityDictionnaryProperty = 103;
        public const uint SetEntityDictionnaryAddProperty = 104;
        public const uint SetEntityDictionnaryRemoveProperty = 105;
        public const uint SetEntityListProperty = 106;
        public const uint SetEntityListAddProperty = 107;
        public const uint SetEntityListRemoveProperty = 108;
        public const uint SetEntityMatrixProperty = 109;

        public const uint MultiSetEntityProperty = 110;


        public const uint ProjectTool = 200;
        public const uint SwitchTool = 201;
        public const uint ReleaseTool = 202;

        public const uint SetUTSTargetFPS = 300;
        public const uint SetStreamedBones = 301;
        public const uint StartInterpolationProperty = 302;
        public const uint StopInterpolationProperty = 303;
        public const uint SetSendingCameraProperty = 304;
        public const uint SetSendingTracking = 305;
        public const uint VehicleRequest = 306;

        public const uint InteractionRequest = 10001;
        public const uint EventStateChanged = 10002;
        public const uint EventTriggered = 10003;
        public const uint FormAnswer = 10004;
        public const uint Hoverred = 10005;
        public const uint HoverStateChanged = 10006;
        public const uint LinkOpened = 10007;
        public const uint ManipulationRequest = 10008;
        public const uint ParameterSettingRequest = 10009;
        public const uint ToolProjected = 10010;
        public const uint ToolReleased = 10011;
        public const uint UserCameraProperties = 10012;
        public const uint UserTrackingFrame = 10013;
        public const uint NotificationCallback = 10014;
        public const uint BoardedVehicleRequest = 10015;

        public const uint VolumeUserTransit = 10100;

    }
    public static class UMI3DParameterKeys
    {
        public const uint FloatRange = 1;
        public const uint IntRange = 2;
        public const uint Bool = 3;
        public const uint Float = 4;
        public const uint Int = 5;
        public const uint String = 6;
        public const uint StringUploadFile = 7;
        public const uint Enum = 8;
    }
}
