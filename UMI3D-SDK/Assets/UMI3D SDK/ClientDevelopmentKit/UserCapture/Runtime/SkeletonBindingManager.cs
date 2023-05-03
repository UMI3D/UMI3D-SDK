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
        #region dependency injection

        private readonly IBindingManager coreBindingService;
        private readonly UMI3DEnvironmentLoader environmentService;
        private readonly ISkeletonManager personnalSkeletonService;

        public SkeletonBindingManager() : base()
        {
            coreBindingService = BindingManager.Instance;
            environmentService = UMI3DEnvironmentLoader.Instance;
            personnalSkeletonService = UserCaptureSkeletonManager.Instance;
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
                        return new RigBoneBinding(riggedBoneBinding, boundNode.transform.Find(riggedBoneBinding.rigName), personnalSkeletonService.skeleton);
                    }
                case BoneBindingDataDto boneBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        return new BoneBinding(boneBindingDataDto, boundNode.transform, personnalSkeletonService.skeleton);
                    }
                case MultiBindingDataDto multiBindingDataDto:
                    {
                        UMI3DNodeInstance boundNode = environmentService.GetNodeInstance(boundNodeId);
                        IEnumerable<(AbstractSimpleBinding binding, bool partialFit)> orderedBindingData = multiBindingDataDto.Bindings
                                                                    .OrderByDescending(x => x.priority)
                                                                    .Select(x => (Load(x, boundNodeId) as AbstractSimpleBinding, x.partialFit));

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