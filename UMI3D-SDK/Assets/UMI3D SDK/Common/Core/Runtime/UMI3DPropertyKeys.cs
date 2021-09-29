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
    static public class UMI3DPropertyKeys
    {
        public const uint None = 0;

        #region core
        // 1 - 999
        public const uint Active = 1;
        public const uint Static = 2;
        public const uint ParentId = 3;
        public const uint Anchor = 4;
        public const uint VROnly = 5;

        public const uint Position = 10;
        public const uint Rotation = 11;
        public const uint Scale = 12;

        public const uint EntityGroupIds = 101;
        #endregion

        #region Node
        // 1000 - 1999
        public const uint XBillboard = 1001;
        public const uint YBillboard = 1002;
        #endregion

        #region environement
        // 2000 - 2999
        public const uint PreloadedScenes = 2001;
        public const uint UserList = 2002;

        public const uint AmbientType = 2101;
        public const uint AmbientSkyColor = 2102;
        public const uint AmbientHorizontalColor = 2103;
        public const uint AmbientGroundColor = 2104;
        public const uint AmbientIntensity = 2105;
        public const uint AmbientSkyboxImage = 2106;
        #endregion

        #region Model
        //3000 -3999
        public const uint Model = 3001;
        public const uint ApplyCustomMaterial = 3002;
        public const uint AreSubobjectsTracked = 3003;
        public const uint CastShadow = 3004;
        public const uint ReceiveShadow = 3005;
        public const uint IgnoreModelMaterialOverride = 3006;
        #endregion

        #region KHR_light
        // 4000 - 4999
        public const uint Light = 4001;
        public const uint LightColor = 4002;
        public const uint LightIntensity = 4003;
        public const uint LightRange = 4004;
        public const uint LightType = 4005;
        public const uint LightSpot = 4006;
        #endregion

        #region tool
        // 5000 - 5999
        public const uint ToolboxName = 5001;
        public const uint ToolboxDescription = 5002;
        public const uint ToolboxIcon2D = 5003;
        public const uint ToolboxIcon3D = 5004;
        public const uint ToolboxTools = 5005;
        public const uint ToolboxSceneId = 5006;

        public const uint ToolboxActive = 5010;
        public const uint ToolActive = 5011;

        public const uint AbstractToolName = 5101;
        public const uint AbstractToolDescription = 5102;
        public const uint AbstractToolIcon2D = 5103;
        public const uint AbstractToolIcon3D = 5104;
        public const uint AbstractToolInteractions = 5105;

        public const uint InteractableNodeId = 5201;
        public const uint InteractableNotifyHoverPosition = 5202;
        public const uint InteractableNotifySubObject = 5203;
        public const uint InteractableHasPriority = 5204;
        #endregion

        #region UI
        // 6000-9999
        #region core
        // 6000-6999
        public const uint AnchoredPosition = 6001;
        public const uint AnchoredPosition3D = 6002;
        public const uint AnchorMax = 6003;
        public const uint AnchorMin = 6004;
        public const uint OffsetMax = 6005;
        public const uint OffsetMin = 6006;
        public const uint Pivot = 6007;
        public const uint SizeDelta = 6008;
        public const uint RectMask = 6009;
        #endregion

        #region Canvas
        //7000-7999
        public const uint DynamicPixelsPerUnit = 7001;
        public const uint ReferencePixelsPerUnit = 7002;
        public const uint OrderInLayer = 7003;
        #endregion

        #region Text
        //8000-8999
        public const uint Alignement = 8001;
        public const uint AlignByGeometry = 8002;
        public const uint TextColor = 8003;
        public const uint TextFont = 8004;
        public const uint FontSize = 8005;
        public const uint FontStyle = 8006;
        public const uint HorizontalOverflow = 8007;
        public const uint VerticalOverflow = 8008;
        public const uint LineSpacing = 8009;
        public const uint ResizeTextForBestFit = 8010;
        public const uint ResizeTextMaxSize = 8011;
        public const uint ResizeTextMinSize = 8012;
        public const uint SupportRichText = 8013;
        public const uint Text = 8014;
        #endregion

        #region Image
        //9000-9999
        public const uint ImageColor = 9001;
        public const uint ImageType = 9002;
        public const uint Image = 9003;
        #endregion
        #endregion

        #region Collider
        // 10000-10999
        public const uint HasCollider = 10001;
        public const uint ColliderType = 10002;
        public const uint Convex = 10003;
        public const uint ColliderCenter = 10004;
        public const uint ColliderRadius = 10005;
        public const uint ColliderBoxSize = 10006;
        public const uint ColliderHeight = 10007;
        public const uint ColliderDirection = 10008;
        public const uint IsMeshColliderCustom = 10009;
        public const uint ColliderCustomResource = 10010;
        #endregion

        #region UserTracking
        // 11000-11999
        public const uint UserBindings = 11001;
        public const uint ActiveBindings = 11002;

        public const uint ActiveHandPose = 11003;
        #endregion

        #region notification
        // 12000-12999
        public const uint NotificationTitle = 12001;
        public const uint NotificationContent = 12002;
        public const uint NotificationDuration = 12003;
        public const uint NotificationIcon2D = 12004;
        public const uint NotificationIcon3D = 12005;
        public const uint NotificationObjectId = 12006;
        #endregion

        #region Animation
        // 13000-13999
        public const uint AnimationPlaying = 13001;
        public const uint AnimationLooping = 13002;
        public const uint AnimationStartTime = 13003;
        public const uint AnimationPauseFrame = 13004;

        public const uint AnimationNode = 13101;
        public const uint AnimationResource = 13102;

        public const uint AnimationVolume = 13201;
        public const uint AnimationPitch = 13202;
        public const uint AnimationSpacialBlend = 13203;

        public const uint AnimationDuration = 13301;
        public const uint AnimationChain = 13302;

        public const uint AnimationNodeId = 13401;
        public const uint AnimationStateName = 13402;

        #endregion

        #region Material
        //14000-14999
        public const uint BaseColorFactor = 14001;
        public const uint MetallicFactor = 14002;
        public const uint RoughnessFactor = 14003;
        public const uint EmissiveFactor = 14004;

        public const uint Maintexture = 14101;
        public const uint MetallicRoughnessTexture = 14102;
        public const uint NormalTexture = 14103;
        public const uint EmissiveTexture = 14104;
        public const uint OcclusionTexture = 14105;
        public const uint MetallicTexture = 14106;
        public const uint RoughnessTexture = 14107;
        public const uint HeightTexture = 14108;
        public const uint ChannelTexture = 14109;

        public const uint TextureTilingOffset = 14201;
        public const uint TextureTilingScale = 14202;
        public const uint NormalTextureScale = 14203;
        public const uint HeightTextureScale = 14204;

        public const uint ShaderProperties = 14301;
        public const uint Shader = 14302;

        //List of overided materials
        public const uint OverideMaterialId = 14401;
        #endregion
    }
}
