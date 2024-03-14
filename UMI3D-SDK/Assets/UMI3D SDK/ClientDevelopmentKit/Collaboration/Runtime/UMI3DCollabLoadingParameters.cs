﻿/*
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
using System;

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
        public bool CollaborationUserCaptureActivated => collaborationUserCaptureActivated;

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

        public static event Func<DtoContainer, Task> unknownOperationHandlerDto;
        public static event Func<uint, ByteContainer, Task> unknownOperationHandlerByte;

        public override void Init()
        {
            // force instanciation
            _ = UMI3DCollaborationEnvironmentLoader.Instance;

            nodeLoader = new UMI3DNodeLoader();

            (loader = new EntityGroupLoader())
            .SetNext(new UMI3DInteractionLoader())
            .SetNext(new UMI3DAnimationLoader())
            .SetNext(new PreloadedSceneLoader())
            .SetNext(new UMI3DInteractableLoader())
            .SetNext(new UMI3DGlobalToolLoader())
            .SetNext(new CollaborationSkeletonAnimationNodeLoader())
            .SetNext(new UMI3DMeshNodeLoader())
            .SetNext(new UMI3DLineRendererLoader())
            .SetNext(new UMI3DSubMeshNodeLoader())
            .SetNext(new UMI3DVolumeLoader())
            .SetNext(new UMI3DUINodeLoader())
            .SetNext(new PoseClipLoader())
            .SetNext(new PoseAnimatorLoader())
            .SetNext(new emotes.UMI3DEmotesConfigLoader())
            .SetNext(new emotes.UMI3DEmoteLoader())
            .SetNext(new CollaborationBindingLoader())
            .SetNext(notificationLoader.GetNotificationLoader())
            .SetNext(new WebViewLoader())
            .SetNext(new DistantEnvironmentLoader())
            .SetNext(new UMI3DNodeLoader())
            .SetNext(UMI3DEnvironmentLoader.Instance.nodeLoader)
            ;
        }

        /// <inheritdoc/>
        public override Task UnknownOperationHandler(DtoContainer operation)
        {
            base.UnknownOperationHandler(operation);
            return unknownOperationHandlerDto?.Invoke(operation) ?? Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task UnknownOperationHandler(uint operationId, ByteContainer container)
        {
            base.UnknownOperationHandler(operationId, container);
            return unknownOperationHandlerByte?.Invoke(operationId, container) ?? Task.CompletedTask;
        }
    }
}
