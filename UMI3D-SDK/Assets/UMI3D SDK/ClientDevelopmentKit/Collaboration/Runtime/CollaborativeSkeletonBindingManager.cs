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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEditor;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class CollaborativeSkeletonBindingManager : Singleton<CollaborativeSkeletonBindingManager>, IBindingManager
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration;

        #region dependency injection

        private readonly IBindingManager coreBindingService;
        private readonly IBindingManager userCaptureBindingService;
        private readonly UMI3DEnvironmentLoader environmentService;
        private readonly CollaborativeSkeletonManager skeletonService;

        public CollaborativeSkeletonBindingManager() : base()
        {
            coreBindingService = BindingManager.Instance;
            userCaptureBindingService = SkeletonBindingManager.Instance;
            environmentService = UMI3DEnvironmentLoader.Instance;
            skeletonService = CollaborativeSkeletonManager.Instance;
        }

        public CollaborativeSkeletonBindingManager(IBindingManager coreBindingManager, IBindingManager userCaptureBindingService) : base()
        {
            this.coreBindingService = coreBindingManager;
            this.userCaptureBindingService = userCaptureBindingService;
        }

        #endregion dependency injection

        public bool AreBindingsActivated => throw new NotImplementedException();

        public Dictionary<ulong, AbstractBinding> Bindings => throw new NotImplementedException();

        public void AddBinding(BindingDto dto)
        {
            AbstractBinding binding = Load(dto.data, dto.boundNodeId);
            if (binding is null)
                return;
            coreBindingService.AddBinding(binding, dto.boundNodeId);
        }

        public void AddBinding(AbstractBinding binding, ulong boundNodeId)
        {
            coreBindingService.AddBinding(binding, boundNodeId);
        }

        public AbstractBinding Load(AbstractBindingDataDto dto, ulong boundNodeId)
        {
            switch (dto)
            {
                case NodeBindingDataDto:
                    {
                        return coreBindingService.Load(dto, boundNodeId);
                    }
                case RigBoneBindingDataDto riggedBoneBinding:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        var skeleton = skeletonService.skeletons[riggedBoneBinding.userId];
                        if (!skeleton.Bones.ContainsKey(riggedBoneBinding.boneType))
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {riggedBoneBinding.boneType}. Bone does not exist on skeleton.", DEBUG_SCOPE);
                            return null;
                        }
                        Transform rig = boundNode.transform.GetComponentsInChildren<Transform>().Where(t=>t.name == riggedBoneBinding.rigName).FirstOrDefault();
                        if (rig == null)
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {riggedBoneBinding.boneType}. Rig \"{riggedBoneBinding.rigName}\" does not exist on bound node.", DEBUG_SCOPE);
                            return null;
                        }

                        return new RigBoneBinding(riggedBoneBinding, boundNode.transform.Find(riggedBoneBinding.rigName), skeleton);
                        return new RigBoneBinding(riggedBoneBinding, rig, skeleton);
                    }
                case BoneBindingDataDto boneBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        var skeleton = skeletonService.skeletons[boneBindingDataDto.userId];
                        if (!skeleton.Bones.ContainsKey(boneBindingDataDto.boneType))
                        {
                            UMI3DLogger.LogWarning($"Impossible to bind on bone {boneBindingDataDto.boneType}. Bone does not exist on skeleton.", DEBUG_SCOPE);
                            return null;
                        }
                        return new BoneBinding(boneBindingDataDto, boundNode.transform, skeleton);
                    }
                case MultiBindingDataDto multiBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        IEnumerable<(AbstractSimpleBinding binding, bool partialFit)> orderedBindingData = multiBindingDataDto.Bindings
                                                                    .OrderByDescending(x => x.priority)
                                                                    .Select(x => (binding: Load(x, boundNodeId) as AbstractSimpleBinding, 
                                                                                                        partialFit: x.partialFit))
                                                                    .Where(x => x.binding is not null);

                        if (orderedBindingData.Count() == 0)
                        {
                            UMI3DLogger.LogWarning($"Impossible to multi-bind. All bindings are impossible to apply.", DEBUG_SCOPE);
                            return null;
                        }

                        AbstractSimpleBinding[] orderedBindings = orderedBindingData.Select(x => x.binding).ToArray();
                        bool[] partialFits = orderedBindingData.Select(x => x.partialFit).ToArray();
                        return new MultiBinding(orderedBindings, partialFits, boundNode.transform);
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