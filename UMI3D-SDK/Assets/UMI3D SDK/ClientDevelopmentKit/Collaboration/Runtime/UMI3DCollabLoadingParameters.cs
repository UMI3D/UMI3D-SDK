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

using System.Threading.Tasks;
using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.volumes;
using umi3d.cdk.collaboration.userCapture.animation;
using umi3d.cdk.collaboration.userCapture.binding;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using umi3d.cdk.userCapture.tracking.constraint;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Loading helper in a collaboration context.
    /// </summary>
    [CreateAssetMenu(fileName = "CollabLoadingParameters", menuName = "UMI3D/Collab Loading Parameters")]
    public class UMI3DCollabLoadingParameters : UMI3DUserCaptureLoadingParameters, IUMI3DCollabLoadingParameters
    {
        #region Collab User Capture
        [Header("Collab User Capture")]
        [SerializeField]
        private bool collaborationUserCaptureActivated;
        public bool CollaborationUserCaptureActivated
        {
            get => collaborationUserCaptureActivated;
            set => collaborationUserCaptureActivated = value;
        }

        [SerializeField]
        private GameObject collabTrackedSkeleton;
        public GameObject CollabTrackedSkeleton => collabTrackedSkeleton;

        #endregion Collab User Capture

        #region Emotes
        [Header("Emotes")]
        [SerializeField, Tooltip("Is the browser supporting emotes?")]
        public bool areEmotesSupported;

        /// <summary>
        /// Icon displayed by default when no icon is defined for an emote.
        /// </summary>
        [SerializeField, Tooltip("Icon displayed by default when no icon is defined for an emote.")]
        public Sprite defaultEmoteIcon;

        #endregion Emotes

        public override void Init()
        {
            // force instanciation
            _ = UMI3DCollaborationEnvironmentLoader.Instance;

            nodeLoader = new UMI3DNodeLoader();

            (loader = new EntityGroupLoader())
            .SetNext(new UMI3DLineRendererLoader())
            .SetNext(new UMI3DInteractionLoader())
            .SetNext(new UMI3DAnimationLoader())
            .SetNext(new PreloadedSceneLoader())
            .SetNext(new UMI3DInteractableLoader())
            .SetNext(new CollaborationSkeletonAnimationNodeLoader())
            .SetNext(new UMI3DMeshNodeLoader())
            .SetNext(new UMI3DSubMeshNodeLoader())
            .SetNext(new UMI3DVolumeLoader())
            .SetNext(new UMI3DUINodeLoader())
            .SetNext(new PoseClipLoader())
            .SetNext(new BoneConstraintLoader())
            .SetNext(new PoseAnimatorLoader())
            .SetNext(new emotes.UMI3DEmotesConfigLoader())
            .SetNext(new emotes.UMI3DEmoteLoader())
            .SetNext(new CollaborationBindingLoader())
            .SetNext(notificationLoader.GetNotificationLoader())
            .SetNext(new WebViewLoader())
            .SetNext(new DistantEnvironmentLoader())
            .SetNext(new UMI3DNodeLoader())
            .SetNext(UMI3DEnvironmentLoader.Instance.nodeLoader)
            .SetNext(new AudioBindingLoader());
            ;
        }

        /// <inheritdoc/>
        public override Task UnknownOperationHandler(DtoContainer operation)
        {
            base.UnknownOperationHandler(operation);

            Tool toolToRelease;
            Tool toolToProject;

            switch (operation.operation)
            {
                case SwitchToolDto switchTool:
#if !UMI3D_NEW_LABEL
                    InteractionMapper.Instance.SwitchTools(operation.environmentId, switchTool.toolId, switchTool.replacedToolId, switchTool.releasable, 0, new interaction.RequestedByEnvironment());
#else
                    // TODO: Projection.releasable
                    ToolManager.@default.TryToFetchTool(out toolToRelease, operation.environmentId, switchTool.replacedToolId);
                    ToolManager.@default.TryToFetchTool(out toolToProject, operation.environmentId, switchTool.toolId);
                    SelectorManager.@default.serverSelector.Switch(toolToRelease, toolToProject);
#endif
                    break;
                case ProjectToolDto projection:
#if !UMI3D_NEW_LABEL
                    InteractionMapper.Instance.SelectTool(operation.environmentId, projection.toolId, projection.releasable, 0, new interaction.RequestedByEnvironment());
#else
                    // TODO: Projection.releasable
                    ToolManager.@default.TryToFetchTool(out toolToProject, operation.environmentId, projection.toolId);
                    SelectorManager.@default.serverSelector.Select(toolToProject);
#endif
                    break;
                case ReleaseToolDto release:
#if !UMI3D_NEW_LABEL
                    InteractionMapper.Instance.ReleaseTool(operation.environmentId, release.toolId, new interaction.RequestedByEnvironment());
#else
                    ToolManager.@default.TryToFetchTool(out toolToRelease, operation.environmentId, release.toolId);
                    SelectorManager.@default.serverSelector.Deselect(toolToRelease);
#endif
                    break;
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task UnknownOperationHandler(uint operationId, ByteContainer container)
        {
            base.UnknownOperationHandler(operationId, container);

            ulong id;
            bool releasable;

            Tool toolToRelease;
            Tool toolToProject;

            switch (operationId)
            {
                case UMI3DOperationKeys.SwitchTool:
                    id = UMI3DSerializer.Read<ulong>(container);
                    ulong oldid = UMI3DSerializer.Read<ulong>(container);
                    releasable = UMI3DSerializer.Read<bool>(container);
#if !UMI3D_NEW_LABEL
                    InteractionMapper.Instance.SwitchTools(container.environmentId, id, oldid, releasable, 0, new interaction.RequestedByEnvironment());
#else
                    // TODO: Projection.releasable
                    ToolManager.@default.TryToFetchTool(out toolToRelease, container.environmentId, oldid);
                    ToolManager.@default.TryToFetchTool(out toolToProject, container.environmentId, id);
                    SelectorManager.@default.serverSelector.Switch(toolToRelease, toolToProject);
#endif
                    break;
                case UMI3DOperationKeys.ProjectTool:
                    id = UMI3DSerializer.Read<ulong>(container);
                    releasable = UMI3DSerializer.Read<bool>(container);
#if !UMI3D_NEW_LABEL
                    InteractionMapper.Instance.SelectTool(container.environmentId, id, releasable, 0, new interaction.RequestedByEnvironment());
#else
                    // TODO: Projection.releasable
                    ToolManager.@default.TryToFetchTool(out toolToProject, container.environmentId, id);
                    SelectorManager.@default.serverSelector.Select(toolToProject);
#endif
                    break;
                case UMI3DOperationKeys.ReleaseTool:
                    id = UMI3DSerializer.Read<ulong>(container);
#if !UMI3D_NEW_LABEL
                    InteractionMapper.Instance.ReleaseTool(container.environmentId, id, new interaction.RequestedByEnvironment());
#else
                    ToolManager.@default.TryToFetchTool(out toolToRelease, container.environmentId, id);
                    SelectorManager.@default.serverSelector.Deselect(toolToRelease);
#endif
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
