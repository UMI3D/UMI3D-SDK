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

using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.binding;
using umi3d.common;
using umi3d.common.dto.binding;
using umi3d.common.userCapture.binding;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture.binding
{
    /// <summary>
    /// Loader for bone bindings, i.e. <see cref="BoneBindingDataDto"/> and <see cref="RigBoneBindingDataDto"/>.
    /// </summary>
    public class UserCaptureBindingLoader : BindingLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Loading;

        #region DependencyInjection

        private readonly IEnvironmentManager environmentService;
        private readonly ISkeletonManager personnalSkeletonService;

        public UserCaptureBindingLoader() : base()
        {
            environmentService = UMI3DEnvironmentLoader.Instance;
            personnalSkeletonService = PersonalSkeletonManager.Instance;
        }

        public UserCaptureBindingLoader(IBindingBrowserService bindingManager,
                                        IEnvironmentManager environmentService,
                                        ILoadingManager loadingManager,
                                        ISkeletonManager personnalSkeletonService)
             : base(loadingManager, environmentService, bindingManager)
        {
            this.environmentService = environmentService;
            this.personnalSkeletonService = personnalSkeletonService;
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
                        if (!personnalSkeletonService.PersonalSkeleton.Bones.ContainsKey(riggedBoneBinding.boneType))
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
                        return new RigBoneBinding(riggedBoneBinding, rig, personnalSkeletonService.PersonalSkeleton, boundNode.transform);
                    }
                case BoneBindingDataDto boneBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        if (!personnalSkeletonService.PersonalSkeleton.Bones.ContainsKey(boneBindingDataDto.boneType))
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {boneBindingDataDto.boneType} - {BoneTypeHelper.GetBoneName(boneBindingDataDto.boneType)}. Bone does not exist on skeleton", DEBUG_SCOPE);
                            return null;
                        }
                        return new BoneBinding(boneBindingDataDto, boundNode.transform, personnalSkeletonService.PersonalSkeleton);
                    }
                default:
                    return null;
            }
        }
    }
}