﻿/*
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

using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.binding;
using umi3d.cdk.userCapture.binding;
using umi3d.common;
using umi3d.common.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;

using UnityEngine;

namespace umi3d.cdk.collaboration.binding
{
    /// <summary>
    /// Loader for bindings and bone bindings on other users' skeleton.
    /// </summary>
    public class CollaborationBindingLoader : BindingLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Loading;

        #region DependencyInjection

        private readonly UMI3DEnvironmentLoader environmentService;
        private readonly ICollaborativeSkeletonsManager skeletonService;

        public CollaborationBindingLoader() : base()
        {
            environmentService = UMI3DCollaborationEnvironmentLoader.Instance;
            skeletonService = CollaborativeSkeletonManager.Instance;
        }

        public CollaborationBindingLoader(IBindingBrowserService bindingManager,
                                                UMI3DCollaborationEnvironmentLoader environmentLoader,
                                                ICollaborativeSkeletonsManager skeletonService) :
                                                base(bindingManager, environmentLoader)
        {
            this.environmentService = environmentLoader;
            this.skeletonService = skeletonService;
        }

        #endregion DependencyInjection

        /// <inheritdoc/>
        protected override async Task<AbstractBinding> LoadData(ulong boundNodeId, AbstractBindingDataDto dto)
        {
            switch (dto)
            {
                case NodeBindingDataDto
                    or MultiBindingDataDto:
                    {
                        return await base.LoadData(boundNodeId, dto);
                    }
                case RigBoneBindingDataDto riggedBoneBinding:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        var skeleton = skeletonService.skeletons[riggedBoneBinding.userId];
                        if (!skeleton.Bones.ContainsKey(riggedBoneBinding.boneType))
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {riggedBoneBinding.boneType} - {BoneTypeHelper.GetBoneName(riggedBoneBinding.boneType)}. Bone does not exist on skeleton.", DEBUG_SCOPE);
                            return null;
                        }
                        Transform rig = boundNode.transform.GetComponentsInChildren<Transform>().Where(t => t.name == riggedBoneBinding.rigName).FirstOrDefault();
                        if (rig == null)
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {riggedBoneBinding.boneType} - {BoneTypeHelper.GetBoneName(riggedBoneBinding.boneType)}. Rig \"{riggedBoneBinding.rigName}\" does not exist on bound node.", DEBUG_SCOPE);
                            return null;
                        }

                        return new RigBoneBinding(riggedBoneBinding, rig, skeleton);
                    }
                case BoneBindingDataDto boneBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        var skeleton = skeletonService.skeletons[boneBindingDataDto.userId];
                        if (!skeleton.Bones.ContainsKey(boneBindingDataDto.boneType))
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {boneBindingDataDto.boneType} - {BoneTypeHelper.GetBoneName(boneBindingDataDto.boneType)}. Bone does not exist on skeleton.", DEBUG_SCOPE);
                            return null;
                        }
                        return new BoneBinding(boneBindingDataDto, boundNode.transform, skeleton);
                    }
                default:
                    return null;
            }
        }
    }
}