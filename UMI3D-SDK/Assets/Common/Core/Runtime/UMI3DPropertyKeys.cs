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

namespace umi3d.common
{
    [System.Serializable]
    public static class UMI3DPropertyKeys
    {

        public const string Active = "umi3d_active";
        public const string Static = "umi3d_static";
        public const string ParentId = "umi3d_parent_id";
        public const string Anchor = "umi3d_anchor";
        public const string VROnly = "umi3d_vr_only";

        public const string Position = "position";
        public const string Rotation = "rotation";
        public const string Scale = "scale";

        public const string EntityGroupIds = "entity_group_ids";

        #region Node
        public const string XBillboard = "umi3d_billboard_x";
        public const string YBillboard = "umi3d_billboard_y";
        #endregion

        #region environement
        public const string PreloadedScenes = "umi3d_preloadedScenes";
        public const string UserList = "umi3d_userList";
        public const string AmbientType = "umi3d_ambient_type";
        public const string AmbientSkyColor = "umi3d_ambient_sky_color";
        public const string AmbientHorizontalColor = "umi3d_ambient_horizontal_color";
        public const string AmbientGroundColor = "umi3d_ambient_ground_color";
        public const string AmbientIntensity = "umi3d_ambient_intensity";
        public const string AmbientSkyboxImage = "umi3d_ambient_skybox_image";
        #endregion

        #region Model
        public const string Model = "umi3d_model";
        //public const string OverrideMaterial = "umi3d_model_override_material";
        public const string ApplyCustomMaterial = "umi3d_is_custom_material_applied";
        public const string AreSubobjectsTracked = "umi3d_are_subobject_tracked";
        public const string CastShadow = "umi3d_cast_shadow";
        public const string ReceiveShadow = "umi3d_receive_shadow";
        public const string IgnoreModelMaterialOverride = "umi3d_ignore_model_material_override";
        #endregion

        #region KHR_light
        public const string Light = "KHR_light";
        #endregion

        #region tool
        public const string ToolActive = "umi3d_tool_active";
        public const string ToolBoxActive = "umi3d_toolBox_active";

        public const string ToolboxName = "umi3d_toolbox_name";
        public const string ToolboxDescription = "umi3d_toolbox_description";
        public const string ToolboxIcon2D = "umi3d_toolbox_icon_2d";
        public const string ToolboxIcon3D = "umi3d_toolbox_icon_3d";
        public const string ToolboxTools = "umi3d_toolbox_tools";
        public const string ToolboxSceneId = "umi3d_toolbox_scene_id";

        public const string AbstractToolName = "umi3d_abstract_tool_name";
        public const string AbstractToolDescription = "umi3d_abstract_tool_description";
        public const string AbstractToolIcon2D = "umi3d_abstract_tool_icon_2d";
        public const string AbstractToolIcon3D = "umi3d_abstract_tool_icon_3d";
        public const string AbstractToolInteractions = "umi3d_abstract_tool_interactions";

        public const string InteractableNodeId = "umi3d_interactable_node_id";
        public const string InteractableNotifyHoverPosition = "umi3d_interactable_notify_hover_position";
        public const string InteractableNotifySubObject = "umi3d_interactable_notify_sub_object";
        public const string InteractableHasPriority = "umi3d_interactable_has_priority";
        #endregion

        #region UI
        public const string AnchoredPosition = "umi3d_anchored_position";
        public const string AnchoredPosition3D = "umi3d_anchored_position_3d";
        public const string AnchorMax = "umi3d_anchor_max";
        public const string AnchorMin = "umi3d_anchor_min";
        public const string OffsetMax = "umi3d_offset_max";
        public const string OffsetMin = "umi3d_offset_min";
        public const string Pivot = "umi3d_pivot";
        public const string SizeDelta = "umi3d_size_delta";
        public const string RectMask = "umi3d_rect_mask";

        #region Canvas
        public const string DynamicPixelsPerUnit = "umi3d_dynamic_pixels_per_unit";
        public const string ReferencePixelsPerUnit = "umi3d_reference_pixels_per_unit";
        public const string OrderInLayer = "umi3d_order_in_layer";
        #endregion

        #region Text
        public const string Alignement = "umi3d_text_alignement";
        public const string AlignByGeometry = "umi3d_text_align_by_geometry";
        public const string TextColor = "umi3d_text_color";
        public const string TextFont = "umi3d_text_font";
        public const string FontSize = "umi3d_text_font_size";
        public const string FontStyle = "umi3d_text_font_style";
        public const string HorizontalOverflow = "umi3d_text_horizontal_overflow";
        public const string VerticalOverflow = "umi3d_text_vertical_overflow";
        public const string LineSpacing = "umi3d_text_line_spacing";
        public const string ResizeTextForBestFit = "umi3d_text_resize_for_best_fit";
        public const string ResizeTextMaxSize = "umi3d_text_resize_max";
        public const string ResizeTextMinSize = "umi3d_text_resize_min";
        public const string SupportRichText = "umi3d_text_support_rich_text";
        public const string Text = "umi3d_text_value";
        #endregion

        #region Image
        public const string ImageColor = "umi3d_image_color";
        public const string ImageType = "umi3d_image_type";
        public const string Image = "umi3d_image_value";
        #endregion
        #endregion

        #region Collider
        public const string HasCollider = "umi3d_has_collider";
        public const string ColliderType = "umi3d_collider_type";
        public const string Convex = "umi3d_collider_convexe";
        public const string ColliderCenter = "umi3d_collider_center";
        public const string ColliderRadius = "umi3d_collider_radius";
        public const string ColliderBoxSize = "umi3d_collider_box_size";
        public const string ColliderHeight = "umi3d_collider_height";
        public const string ColliderDirection = "umi3d_collider_direction";
        public const string IsMeshColliderCustom = "umi3d_collider_is_custom";
        public const string ColliderCustomResource = "umi3d_collider_custom_mesh";
        #endregion

        #region UserTracking
        public const string UserBindings = "umi3d_user_bones";
        public const string ActiveBindings = "umi3d_active_bindings";
        #endregion

        #region notification
        public const string NotificationTitle = "umi3d_notification_title";
        public const string NotificationContent = "umi3d_notification_content";
        public const string NotificationDuration = "umi3d_notification_duration";
        public const string NotificationIcon2D = "umi3d_notification_icon_2d";
        public const string NotificationIcon3D = "umi3d_notification_icon_3d";
        public const string NotificationObjectId = "umi3d_notification_object_id";
        #endregion

        #region Animation

        public const string AnimationPlaying = "umi3d_animation_playing";
        public const string AnimationLooping = "umi3d_animation_looping";
        public const string AnimationStartTime = "umi3d_animation_start_time";
        public const string AnimationPauseFrame = "umi3d_animation_pause_frame";

        public const string AnimationNode = "umi3d_animation_node";
        public const string AnimationResource = "umi3d_animation_resource";

        public const string AnimationVolume = "umi3d_animation_audio_volume";
        public const string AnimationPitch = "umi3d_animation_audio_pitch";
        public const string AnimationSpacialBlend = "umi3d_animation_audio_spacial_blend";

        public const string AnimationDuration = "umi3d_animation_duration";
        public const string AnimationChain = "umi3d_animation_chain";

        public const string AnimationNodeId = "umi3d_animation_node_id";
        public const string AnimationStateName = "umi3d_animation_state_name";

        #endregion

        #region Material

        //Material properties
        public const string BaseColorFactor = "umi3D_material_base_color_factor";
        public const string MetallicFactor = "umi3D_material_metallic_factor";
        public const string RoughnessFactor = "umi3D_material_roughness_factor";
        public const string EmissiveFactor = "umi3D_material_emissive_factor";

        public const string Maintexture = "umi3D_material_main_texture";
        public const string MetallicRoughnessTexture = "umi3D_material_metallic_roughness_texture";
        public const string NormalTexture = "umi3D_material_normal_texture";
        public const string EmissiveTexture = "umi3D_material_emissive_texture";
        public const string OcclusionTexture = "umi3D_material_occlusion_texture";
        public const string MetallicTexture = "umi3D_material_metallic_texture";
        public const string RoughnessTexture = "umi3D_material_roughness_texture";
        public const string HeightTexture = "umi3D_material_height_texture";
        public const string ChannelTexture = "umi3D_material_channel_texture";

        public const string TextureTilingOffset = "umi3D_material_texture_tiling_offset";
        public const string TextureTilingScale = "umi3D_material_texture_tiling_scale";
        public const string NormalTextureScale = "umi3D_material_normal_texture_scale";
        public const string HeightTextureScale = "umi3D_material_height_texture_scale";

        public const string ShaderProperties = "umi3D_material_shader_properties";
        public const string Shader = "umi3D_material_shader";

        //List of overided materials
        public const string OverideMaterialId = "umi3D_material_overrider";


        #endregion
    }
}
