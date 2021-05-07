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
    [System.Serializable]
    static public class UMI3DPropertyKeys
    {
        public const ulong None = 0;

        #region core
        // 1 - 999
        public const ulong Active = 1;
        public const ulong Static = 2;
        public const ulong ParentId = 3;
        public const ulong Anchor = 4;
        public const ulong VROnly = 5;

        public const ulong Position = 10;
        public const ulong Rotation = 11;
        public const ulong Scale = 12;

        public const ulong EntityGroupIds = 101;
        #endregion

        #region Node
        // 1000 - 1999
        public const ulong XBillboard = 1001;
        public const ulong YBillboard = 1002;
        #endregion

        #region environement
        // 2000 - 2999
        public const ulong PreloadedScenes = 2001;
        public const ulong UserList = 2002;

        public const ulong AmbientType = 2101;
        public const ulong AmbientSkyColor = 2102;
        public const ulong AmbientHorizontalColor = 2103;
        public const ulong AmbientGroundColor = 2104;
        public const ulong AmbientIntensity = 2105;
        public const ulong AmbientSkyboxImage = 2106;
        #endregion

        #region Model
        //3000 -3999
        public const ulong Model = 3001;
        public const ulong ApplyCustomMaterial = 3002;
        public const ulong AreSubobjectsTracked = 3003;
        public const ulong CastShadow = 3004;
        public const ulong ReceiveShadow = 3005;
        public const ulong IgnoreModelMaterialOverride = 3006;
        #endregion

        #region KHR_light
        // 4000 - 4999
        public const ulong Light = 4001;
        public const ulong LightColor = 4002;
        public const ulong LightIntensity = 4003;
        public const ulong LightRange = 4004;
        public const ulong LightType = 4005;
        public const ulong LightSpot = 4006;
        #endregion

        #region tool
        // 5000 - 5999
        public const ulong ToolboxName = 5001;
        public const ulong ToolboxDescription = 5002;
        public const ulong ToolboxIcon2D = 5003;
        public const ulong ToolboxIcon3D = 5004;
        public const ulong ToolboxTools = 5005;
        public const ulong ToolboxSceneId = 5006;

        public const ulong ToolboxActive = 5010;
        public const ulong ToolActive = 5011;

        public const ulong AbstractToolName = 5101;
        public const ulong AbstractToolDescription = 5102;
        public const ulong AbstractToolIcon2D = 5103;
        public const ulong AbstractToolIcon3D = 5104;
        public const ulong AbstractToolInteractions = 5105;

        public const ulong InteractableNodeId = 5201;
        public const ulong InteractableNotifyHoverPosition = 5202;
        public const ulong InteractableNotifySubObject = 5203;
        public const ulong InteractableHasPriority = 5204;
        #endregion

        #region UI
        // 6000-9999
        #region core
        // 6000-6999
        public const ulong AnchoredPosition = 6001;
        public const ulong AnchoredPosition3D = 6002;
        public const ulong AnchorMax = 6003;
        public const ulong AnchorMin = 6004;
        public const ulong OffsetMax = 6005;
        public const ulong OffsetMin = 6006;
        public const ulong Pivot = 6007;
        public const ulong SizeDelta = 6008;
        public const ulong RectMask = 6009;
        #endregion

        #region Canvas
        //7000-7999
        public const ulong DynamicPixelsPerUnit = 7001;
        public const ulong ReferencePixelsPerUnit = 7002;
        public const ulong OrderInLayer = 7003;
        #endregion

        #region Text
        //8000-8999
        public const ulong Alignement = 8001;
        public const ulong AlignByGeometry = 8002;
        public const ulong TextColor = 8003;
        public const ulong TextFont = 8004;
        public const ulong FontSize = 8005;
        public const ulong FontStyle = 8006;
        public const ulong HorizontalOverflow = 8007;
        public const ulong VerticalOverflow = 8008;
        public const ulong LineSpacing = 8009;
        public const ulong ResizeTextForBestFit = 8010;
        public const ulong ResizeTextMaxSize = 8011;
        public const ulong ResizeTextMinSize = 8012;
        public const ulong SupportRichText = 8013;
        public const ulong Text = 8014;
        #endregion

        #region Image
        //9000-9999
        public const ulong ImageColor = 9001;
        public const ulong ImageType = 9002;
        public const ulong Image = 9003;
        #endregion
        #endregion

        #region Collider
        // 10000-10999
        public const ulong HasCollider = 10001;
        public const ulong ColliderType = 10002;
        public const ulong Convex = 10003;
        public const ulong ColliderCenter = 10004;
        public const ulong ColliderRadius = 10005;
        public const ulong ColliderBoxSize = 10006;
        public const ulong ColliderHeight = 10007;
        public const ulong ColliderDirection = 10008;
        public const ulong IsMeshColliderCustom = 10009;
        public const ulong ColliderCustomResource = 10010;
        #endregion

        #region UserTracking
        // 11000-11999
        public const ulong UserBindings = 11001;
        public const ulong ActiveBindings = 11002;
        #endregion

        #region notification
        // 12000-12999
        public const ulong NotificationTitle = 12001;
        public const ulong NotificationContent = 12002;
        public const ulong NotificationDuration = 12003;
        public const ulong NotificationIcon2D = 12004;
        public const ulong NotificationIcon3D = 12005;
        public const ulong NotificationObjectId = 12006;
        #endregion

        #region Animation
        // 13000-13999
        public const ulong AnimationPlaying = 13001;
        public const ulong AnimationLooping = 13002;
        public const ulong AnimationStartTime = 13003;
        public const ulong AnimationPauseFrame = 13004;

        public const ulong AnimationNode = 13101;
        public const ulong AnimationResource = 13102;

        public const ulong AnimationVolume = 13201;
        public const ulong AnimationPitch = 13202;
        public const ulong AnimationSpacialBlend = 13203;

        public const ulong AnimationDuration = 13301;
        public const ulong AnimationChain = 13302;

        #endregion

        #region Material
        //14000-14999
        public const ulong BaseColorFactor = 14001;
        public const ulong MetallicFactor = 14002;
        public const ulong RoughnessFactor = 14003;
        public const ulong EmissiveFactor = 14004;

        public const ulong Maintexture = 14101;
        public const ulong MetallicRoughnessTexture = 14102;
        public const ulong NormalTexture = 14103;
        public const ulong EmissiveTexture = 14104;
        public const ulong OcclusionTexture = 14105;
        public const ulong MetallicTexture = 14106;
        public const ulong RoughnessTexture = 14107;
        public const ulong HeightTexture = 14108;
        public const ulong ChannelTexture = 14109;

        public const ulong TextureTilingOffset = 14201;
        public const ulong TextureTilingScale = 14202;
        public const ulong NormalTextureScale = 14203;
        public const ulong HeightTextureScale = 14204;

        public const ulong ShaderProperties = 14301;
        public const ulong Shader = 14302;

        //List of overided materials
        public const ulong OverideMaterialId = 14401;
        #endregion
    }
}
