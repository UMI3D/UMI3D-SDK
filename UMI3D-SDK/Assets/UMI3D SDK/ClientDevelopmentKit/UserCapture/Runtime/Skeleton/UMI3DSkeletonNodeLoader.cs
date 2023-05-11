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
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Loader called to load <see cref="UMI3DSkeletonNodeDto"/>.
    /// </summary>
    /// It is based upon the <see cref="UMI3DMeshNodeDto"/> as the animation ressources are packaged in a bundle just like in a model.
    public class UMI3DSkeletonNodeLoader : UMI3DMeshNodeLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        #region Dependency Injection

        protected readonly ISkeletonManager personnalSkeletonService;

        public UMI3DSkeletonNodeLoader() : base()
        {
            personnalSkeletonService = PersonalSkeletonManager.Instance;
        }

        public UMI3DSkeletonNodeLoader(ISkeletonManager personnalSkeletonService) : base()
        {
            this.personnalSkeletonService = personnalSkeletonService;
        }

        #endregion Dependency Injection

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DSkeletonNodeDto && base.CanReadUMI3DExtension(data);
        }

        /// <inheritdoc/>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            if (data.dto is not UMI3DSkeletonNodeDto)
                UMI3DLogger.LogError("DTO should be an UM3DSkeletonNodeDto", DEBUG_SCOPE);

            await base.ReadUMI3DExtension(data);

            Load(data.dto as UMI3DSkeletonNodeDto);
        }

        public void Load(UMI3DSkeletonNodeDto skeletonNodeDto)
        {
            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(skeletonNodeDto.id);

            var go = nodeInstance.gameObject;

            var modelTracker = go.GetOrAddComponent<ModelTracker>();

            if (!go.TryGetComponent(out Animator animator)) //! may delete that if already done in base class
                return;

            modelTracker.animatorsToRebind.Add(animator);

            if (go.TryGetComponent(out SkeletonMapper skeletonMapper))
            {
                if (go.TryGetComponent(out TrackedSkeletonBone bone))
                    skeletonMapper.BoneAnchor = new BonePoseDto() { bone = bone.boneType, Position = bone.transform.position.Dto(), Rotation = bone.transform.rotation.Dto() };
                else
                {
                    UMI3DLogger.LogWarning($"No bone found to attach skeleton.", DEBUG_SCOPE);
                    return;
                }
            }
            else
            { // if null, we assume that the hiearchy is the same than the UMI3D standard one
                skeletonMapper = AutoMapAnimatorSkeleton(animator, skeletonNodeDto);
            }

            // create subSkeletonand add it to a skeleton
            AnimatedSkeleton animationSubskeleton = new(skeletonMapper);
            AttachToSkeleton(skeletonNodeDto.userId, animationSubskeleton);

            // hide the model if it has any renderers
            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
                renderer.gameObject.layer = LayerMask.NameToLayer("Invisible");
        }

        /// <summary>
        /// Map a skeleton from its animator structure
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="skeletonNodeDto"></param>
        /// <returns></returns>
        protected SkeletonMapper AutoMapAnimatorSkeleton(Animator animator, UMI3DSkeletonNodeDto skeletonNodeDto)
        {
            SkeletonMapper skeletonMapper = animator.gameObject.AddComponent<SkeletonMapper>();

            // umi3d default anchor is hips
            skeletonMapper.BoneAnchor = new BonePoseDto() { bone = BoneType.Hips, Position = animator.rootPosition, Rotation = animator.rootRotation };

            skeletonMapper.animations = skeletonNodeDto.relatedAnimationsId;

            // map animator bones to umi3d ones
            List<SkeletonMapping> mappings = new();
            var umi3dBones = (UMI3DEnvironmentLoader.Parameters as UMI3DUserCaptureLoadingParameters).SkeletonHierarchy.BoneRelations
                            .Select(x => x.Bonetype)
                            .ToArray();
            foreach (var bone in umi3dBones)
            {
                var unityBone = BoneTypeConverter.ConvertToBoneType(bone);
                if (!unityBone.HasValue)
                    continue;
                Transform boneTransform = animator.GetBoneTransform(unityBone.Value);

                var newMapping = new SkeletonMapping(bone, new GameNodeLink(boneTransform));

                mappings.Add(newMapping);
            }

            return skeletonMapper;
        }

        protected virtual void AttachToSkeleton(ulong userId, AnimatedSkeleton subskeleton)
        {
            personnalSkeletonService.personalSkeleton.Skeletons.Add(subskeleton);
        }
    }
}