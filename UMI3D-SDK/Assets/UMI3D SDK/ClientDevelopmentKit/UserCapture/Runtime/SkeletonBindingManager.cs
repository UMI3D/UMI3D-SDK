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

using inetum.unityUtils;

using System.Collections.Generic;
using System.Linq;

using umi3d.common;
using umi3d.common.userCapture;

using UnityEditor;

using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class SkeletonBindingManager : Singleton<SkeletonBindingManager>, IBindingManager
    {
        protected const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        #region dependency injection

        private readonly IBindingManager coreBindingService;
        private readonly UMI3DEnvironmentLoader environmentService;
        private readonly ISkeletonManager personnalSkeletonService;

        public SkeletonBindingManager() : base()
        {
            coreBindingService = BindingManager.Instance;
            environmentService = UMI3DEnvironmentLoader.Instance;
            personnalSkeletonService = PersonalSkeletonManager.Instance;
        }

        public SkeletonBindingManager(IBindingManager coreBindingManager) : base()
        {
            this.coreBindingService = coreBindingManager;
            environmentService = UMI3DEnvironmentLoader.Instance;
        }

        #endregion dependency injection

        public bool AreBindingsActivated => coreBindingService.AreBindingsActivated;

        public Dictionary<ulong, AbstractBinding> Bindings => coreBindingService.Bindings;

        public void AddBinding(AbstractBinding binding, ulong boundNodeId)
        {
            coreBindingService.AddBinding(binding, boundNodeId);
        }

        public void AddBinding(BindingDto dto)
        {
            AbstractBinding binding = Load(dto.data, dto.boundNodeId);
            if (binding is null)
                return;
            coreBindingService.AddBinding(binding, dto.boundNodeId);
        }

        public AbstractBinding Load(AbstractBindingDataDto dto, ulong boundNodeId)
        {
            switch (dto)
            {
                case NodeBindingDataDto simpleBindingDto:
                    {
                        return coreBindingService.Load(simpleBindingDto, boundNodeId);
                    }
                case RigBoneBindingDataDto riggedBoneBinding:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        if (!personnalSkeletonService.personalSkeleton.Bones.ContainsKey(riggedBoneBinding.boneType))
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {riggedBoneBinding.boneType}. Bone does not exist on skeleton.", DEBUG_SCOPE);
                            return null;
                        }
                        Transform rig = boundNode.transform.GetComponentsInChildren<Transform>().Where(t => t.name == riggedBoneBinding.rigName).FirstOrDefault();
                        if (rig == null)
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {riggedBoneBinding.boneType}. Rig \"{riggedBoneBinding.rigName}\" does not exist on bound node.", DEBUG_SCOPE);
                            return null;
                        }
                        return new RigBoneBinding(riggedBoneBinding, rig, personnalSkeletonService.personalSkeleton);
                    }
                case BoneBindingDataDto boneBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        if (!personnalSkeletonService.personalSkeleton.Bones.ContainsKey(boneBindingDataDto.boneType))
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {boneBindingDataDto.boneType}. Bone does not exist on skeleton", DEBUG_SCOPE);
                            return null;
                        }

                        return new BoneBinding(boneBindingDataDto, boundNode.transform, personnalSkeletonService.personalSkeleton);
                    }
                case MultiBindingDataDto multiBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        (AbstractSimpleBinding binding, bool partialFit)[] orderedBindingData = multiBindingDataDto.Bindings
                                                                    .OrderByDescending(x => x.priority)
                                                                    .Select(x => (binding: Load(x, boundNodeId) as AbstractSimpleBinding, partialFit: x.partialFit))
                                                                    .Where(x => x.binding is not null)
                                                                    .ToArray();

                        if (orderedBindingData.Length == 0)
                        {
                            UMI3DLogger.LogWarning($"Impossible to multi-bind. All bindings are impossible to apply.", DEBUG_SCOPE);
                            return null;
                        }

                        return new MultiBinding(multiBindingDataDto, orderedBindingData, boundNode.transform);
                    }
                default:
                    return null;
            }
        }

        public void RemoveBinding(RemoveBindingDto dto)
        {
            coreBindingService.RemoveBinding(dto);
        }

        public void UpdateBindingsActivation(UpdateBindingsActivationDto dto)
        {
            coreBindingService.UpdateBindingsActivation(dto);
        }
    }
}