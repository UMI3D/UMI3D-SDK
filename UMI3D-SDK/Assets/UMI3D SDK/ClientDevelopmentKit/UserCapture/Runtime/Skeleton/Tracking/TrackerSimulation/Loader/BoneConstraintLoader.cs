/*
Copyright 2019 - 2024 Inetum

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
using umi3d.common;
using umi3d.common.userCapture.tracking.constraint;

namespace umi3d.cdk.userCapture.tracking.constraint
{
    /// <summary>
    /// Loader for <see cref="AbstractBoneConstraintDto"/>.
    /// </summary>
    public class BoneConstraintLoader : AbstractLoader<AbstractBoneConstraintDto, AbstractBoneConstraint>
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        private static readonly UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.7", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;



        #region Dependencies Injection

        private readonly IEnvironmentManager environmentService;
        private readonly ILoadingManager loadingService;
        private readonly ISkeletonManager skeletonService;
        private readonly ISkeletonConstraintService trackerSimulationService;

        public BoneConstraintLoader() : this(environmentService: UMI3DEnvironmentLoader.Instance,
                                           loadingService: UMI3DEnvironmentLoader.Instance,
                                           skeletonService: PersonalSkeletonManager.Instance,
                                           trackerSimulationService: SkeletonConstraintService.Instance)
        {
        }

        public BoneConstraintLoader(IEnvironmentManager environmentService,
                                  ILoadingManager loadingService,
                                  ISkeletonManager skeletonService,
                                  ISkeletonConstraintService trackerSimulationService)
        {
            this.environmentService = environmentService;
            this.loadingService = loadingService;
            this.skeletonService = skeletonService;
            this.trackerSimulationService = trackerSimulationService;
        }

        #endregion Dependencies Injection

        public override async Task<AbstractBoneConstraint> Load(ulong environmentId, AbstractBoneConstraintDto dto)
        {
            AbstractBoneConstraint constraint = null;
            switch (dto)
            {
                case NodeBoneConstraintDto nodeBoneConstraintDto:
                    {
                        UMI3DNodeInstance nodeReference = await loadingService.WaitUntilNodeInstanceLoaded(environmentId, nodeBoneConstraintDto.ConstrainingNodeId, null);

                        constraint = new NodeBoneConstraint(nodeBoneConstraintDto, nodeReference);

                        trackerSimulationService.RegisterConstraint(constraint);
                        environmentService.RegisterEntity(environmentId, dto.id, dto, constraint, () => Delete(environmentId, dto.id)).NotifyLoaded();
                        return constraint;
                    }
                case BoneBoneConstraintDto boneBoneConstraintDto:
                    {
                        if (!skeletonService.PersonalSkeleton.Bones.TryGetValue(boneBoneConstraintDto.ConstrainingBone, out ISkeleton.Transformation boneReference))
                        {
                            await Task.Run(() => UMI3DLogger.LogWarning($"Bone {boneBoneConstraintDto.ConstrainingBone} not found for applying BoneBone constraint.", DEBUG_SCOPE));
                            break;
                        }

                        constraint = new BoneBoneConstraint(boneBoneConstraintDto, boneReference);
                        trackerSimulationService.RegisterConstraint(constraint);
                        environmentService.RegisterEntity(environmentId, dto.id, dto, constraint, () => Delete(environmentId, dto.id)).NotifyLoaded();
                        return constraint;
                    }
                case FloorBoneConstraintDto floorBoneConstraintDto:
                    {
                        await Task.Run(() => UMI3DLogger.LogWarning("FloorAnchor is not yet implemented.", DEBUG_SCOPE));
                        return null;
                    }
            }

            return null;
        }

        public override void Delete(ulong environmentId, ulong id)
        {
            if (!environmentService.TryGetEntity(environmentId, id, out AbstractBoneConstraint constraint))
                return;

            trackerSimulationService.UnregisterConstraint(constraint);
        }
    }
}