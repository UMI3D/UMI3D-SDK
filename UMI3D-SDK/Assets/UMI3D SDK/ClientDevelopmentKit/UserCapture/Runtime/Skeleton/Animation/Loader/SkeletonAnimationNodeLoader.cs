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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.userCapture.description;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.animation;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture.animation
{
    /// <summary>
    /// Loader called to load <see cref="SkeletonAnimationNodeDto"/>.
    /// </summary>
    /// It is based upon the <see cref="UMI3DMeshNodeDto"/> as the animation ressources are packaged in a bundle just like in a model.
    public class SkeletonAnimationNodeLoader : UMI3DMeshNodeLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        private bool isRegisteredForPersonalSkeletonCleanup;

        #region Dependency Injection

        protected readonly ISkeletonManager personalSkeletonService;
        protected readonly IUMI3DClientServer clientServer;

        public SkeletonAnimationNodeLoader() : base()
        {
            personalSkeletonService = PersonalSkeletonManager.Instance;
            clientServer = UMI3DClientServer.Instance;
        }

        public SkeletonAnimationNodeLoader(IEnvironmentManager environmentManager,
                                           ILoadingManager loadingManager,
                                           IUMI3DResourcesManager resourcesManager,
                                           ICoroutineService coroutineManager,
                                           ISkeletonManager personalSkeletonService,
                                           IUMI3DClientServer clientServer)
            : base(environmentManager, loadingManager, resourcesManager, coroutineManager)
        {
            this.personalSkeletonService = personalSkeletonService;
            this.clientServer = clientServer;
        }

        #endregion Dependency Injection

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is SkeletonAnimationNodeDto && base.CanReadUMI3DExtension(data);
        }

        /// <inheritdoc/>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            if (data.dto is not SkeletonAnimationNodeDto)
            {
                UMI3DLogger.LogError("Cannot load DTO. DTO is not an UM3DSkeletonNodeDto", DEBUG_SCOPE);
                return;
            }

            await base.ReadUMI3DExtension(data);

            await Load(data.environmentId, data.dto as SkeletonAnimationNodeDto);
        }

        public async Task Load(ulong environmentId, SkeletonAnimationNodeDto skeletonNodeDto)
        {
            UMI3DNodeInstance nodeInstance = environmentManager.GetNodeInstance(environmentId, skeletonNodeDto.id);  //node exists because of base call of ReadUMI3DExtensiun

            // a skeleton node should contain an animator
            Animator animator = nodeInstance.GameObject.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                UMI3DLogger.LogWarning($"Cannot load skeleton animation {skeletonNodeDto.id}. No animator was found on node for user {skeletonNodeDto.userId}. ", DEBUG_SCOPE);
                return;
            }

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // required for applying movements on your body when you're not looking at it

            // get skeleton mapper from model or create one
            ISkeletonMapper skeletonMapper = RetrieveSkeletonMapper(skeletonNodeDto, animator);
            if (skeletonMapper == null) // failed infinding/adding skeletonMapper
            {
                UMI3DLogger.LogWarning($"No skeleton mapper was provided for skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId} and cannot auto-extract from animator failed.", DEBUG_SCOPE);
                return;
            }

            var modelTracker = nodeInstance.GameObject.GetOrAddComponent<ModelTracker>();
            modelTracker.animatorsToRebind.Add(animator);

            // hide the model if it has any renderers
            foreach (var renderer in nodeInstance.GameObject.GetComponentsInChildren<Renderer>())
                renderer.gameObject.layer = LayerMask.NameToLayer("Invisible");

            // scale the subskeleton to fit the scale of the user
            nodeInstance.transform.localScale = personalSkeletonService.PersonalSkeleton.worldSize;

            Task attachtask = WaitAndAttach(); // do not await purposefully

            async Task WaitAndAttach()
            {
                // get animation related to the skeleton node
                Queue<UMI3DAnimatorAnimation> animations = new(skeletonNodeDto.relatedAnimationsId.Length);
                foreach (var id in skeletonNodeDto.relatedAnimationsId)
                {
                    var instance = await loadingManager.WaitUntilEntityLoaded(environmentId, id, null);
                    animations.Enqueue(instance.Object as UMI3DAnimatorAnimation);
                }

                // create subSkeleton and add it to a skeleton
                ISkeleton parentSkeleton = GetParentSkeleton(environmentId, skeletonNodeDto.userId);
                if (parentSkeleton == null)
                {
                    UMI3DLogger.LogWarning($"Skeleton of user {skeletonNodeDto.userId} not found. Cannot attach skeleton node.", DEBUG_SCOPE);
                    return;
                }

                // cull animators of other skeletons when they are not visible
                if (parentSkeleton is not IPersonalSkeleton)
                {
                    parentSkeleton.VisibilityChanged += AutoCullAnimator;

                    void AutoCullAnimator(bool isVisible)
                    {
                        if (animator == null)
                            return;

                        if (isVisible && animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
                        {
                            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                            animator.Update(0);
                        }
                        else if (!isVisible && animator.cullingMode != AnimatorCullingMode.CullCompletely)
                        {
                            animator.cullingMode = AnimatorCullingMode.CullCompletely;
                        }
                    }
                }

                ISubskeletonDescriptionInterpolationPlayer player = new SubskeletonDescriptionInterpolationPlayer(skeletonMapper, skeletonNodeDto.IsInterpolable, parentSkeleton);
                AnimatedSubskeleton animationSubskeleton = new AnimatedSubskeleton(skeletonNodeDto, player, skeletonMapper, animations.ToArray(), skeletonNodeDto.animatorSelfTrackedParameters);
                AttachToSkeleton(parentSkeleton, animationSubskeleton);
            };
            nodeInstance.Delete = () => Delete(skeletonNodeDto.userId);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get the parent skeleton of the animated subskeleton.
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected virtual ISkeleton GetParentSkeleton(ulong environmentId, ulong userId)
        {
            return personalSkeletonService.PersonalSkeleton;
        }

        protected virtual void Delete(ulong userId)
        {
            if (isRegisteredForPersonalSkeletonCleanup)
            {
                RemoveSkeletons();
            }
        }

        #region SkeletonMapping

        /// <summary>
        /// Extract skeleton mapper from skeleton or create one
        /// </summary>
        /// <param name="skeletonNodeDto"></param>
        /// <param name="animator"></param>
        /// <returns></returns>
        protected ISkeletonMapper RetrieveSkeletonMapper(SkeletonAnimationNodeDto skeletonNodeDto, Animator animator)
        {
            // if the designer added a skeleton mapper, uses its links
            SkeletonMapper skeletonMapper = animator.gameObject.GetComponentInChildren<SkeletonMapper>();
            if (skeletonMapper != null)
            {
                if (skeletonMapper.Mappings.Count == 0)
                {
                    UMI3DLogger.LogWarning($"Error when getting Skeleton Mapper. SkeletonMapper found on skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId}, but no mapping could be retrieved.", DEBUG_SCOPE);
                    return null;
                }
                if (skeletonMapper.BoneAnchor == null)
                {
                    UMI3DLogger.LogWarning($"Error when getting Skeleton Mapper. SkeletonMapper found on skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId}, but an anchor is missing.", DEBUG_SCOPE);
                    return null;
                }
            }
            else if (animator.avatar != null && animator.avatar.isHuman)// if null, we try to adapt the unity avatar (rigs) by ourselves assuming it is close to the UMI3D standard one
            {
                skeletonMapper = AutoMapAnimatorSkeleton(animator, skeletonNodeDto);
            }
            return skeletonMapper;
        }

        /// <summary>
        /// Map a skeleton from its animator structure
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="skeletonNodeDto"></param>
        /// <returns></returns>
        protected SkeletonMapper AutoMapAnimatorSkeleton(Animator animator, SkeletonAnimationNodeDto skeletonNodeDto)
        {
            SkeletonMapper skeletonMapper = animator.gameObject.AddComponent<SkeletonMapper>();

            // umi3d default anchor is hips
            skeletonMapper.BoneAnchor = new PoseAnchorDto() { bone = BoneType.Hips, position = animator.rootPosition.Dto(), rotation = animator.rootRotation.Dto() };

            // map animator unity bones to umi3d ones
            var boneUnityMapping = FindBonesTransform(animator);

            // if no bone can be mapped, then extract from animator
            if (boneUnityMapping.Length == 0)
            {
                ExtractRigsFromAnimator(animator);
                animator.Rebind();
                boneUnityMapping = FindBonesTransform(animator);

                // if still no bones can be retrieved, the avatar mask in the animator cannot be adapted
                if (boneUnityMapping.Length == 0)
                {
                    UMI3DLogger.LogWarning($"No skeleton mapper was provided for skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId} and attempt to auto-extract from animator failed", DEBUG_SCOPE);
                    return null;
                }
            }

            // create link for each rig. May be improved with distance analysis for more complex links
            skeletonMapper.Mappings = (from bone in boneUnityMapping
                                       where bone.transform != null
                                       select new SkeletonMapping(bone.umi3dBoneType, new GameNodeLink(bone.transform))).ToList();

            return skeletonMapper;
        }

        /// <summary>
        /// Look for bones contained in an animator and their associated transform.
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        protected (uint umi3dBoneType, Transform transform)[] FindBonesTransform(Animator animator)
        {
            return (from relationUMI3D in (loadingManager.AbstractLoadingParameters as IUMI3DUserCaptureLoadingParameters).SkeletonHierarchyDefinition.Relations
                    let relationUnity = (umi3dBoneType: relationUMI3D.boneType, unityBoneContainer: BoneTypeConvertingExtensions.ConvertToBoneType(relationUMI3D.boneType))
                    where relationUnity.unityBoneContainer.HasValue
                    let relationTransform = (relationUnity.umi3dBoneType, transform: animator.GetBoneTransform(relationUnity.unityBoneContainer.Value))
                    where relationTransform.transform != null
                    select relationTransform).ToArray();
        }

        /// <summary>
        /// Create a gameobject as a child for each bone declared in the animator.
        /// </summary>
        /// This implies a flat hierarchy.
        /// <param name="animator"></param>
        protected void ExtractRigsFromAnimator(Animator animator)
        {
            UMI3DSkeletonHierarchy hierarchy = personalSkeletonService.PersonalSkeleton.SkeletonHierarchy;
            var transformHierarchy = hierarchy.Generate(animator.transform);

            static string RemoveWhiteSpaces(string s)
            {
                return string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
            }

            // unity name -> mecanim name / rigname
            var humanBoneRigRelations = animator.avatar.humanDescription.human.ToDictionary(x => RemoveWhiteSpaces(x.humanName).ToLower(), x => x.boneName);

            // rig name in animator -> local transform infos
            var animatorBoneInfos = animator.avatar.humanDescription.skeleton.ToDictionary(x => x.name, x => (x.position, x.rotation, x.scale));

            ExtractRig(BoneType.Hips);

            void ExtractRig(uint boneType)
            {
                Transform node = transformHierarchy[boneType];

                //  proper hierarchy has all the required bones
                if (!transformHierarchy.ContainsKey(boneType))
                    return;

                Transform boneTransform = transformHierarchy[boneType];

                string unityBoneName = BoneTypeConvertingExtensions.ConvertToBoneType(boneType).ToString();
                string animatorRigNames = humanBoneRigRelations.ContainsKey(unityBoneName.ToLower()) ? humanBoneRigRelations[unityBoneName.ToLower()] : string.Empty;

                if (animatorBoneInfos.ContainsKey(animatorRigNames)) // case where bone can get the info from animator
                {
                    var (position, rotation, scale) = animatorBoneInfos[animatorRigNames];

                    boneTransform.name = animatorRigNames;
                    boneTransform.localPosition = position;
                    boneTransform.localRotation = rotation;
                    boneTransform.localScale = scale;
                }
                else // case that occurs if the umi3d bone is not found in animator hierarchy, lift children up and delete parent.
                {
                    foreach (var child in hierarchy.Relations[boneType].children)
                    {
                        Transform liftedNode = transformHierarchy[child.boneType];
                        liftedNode.SetParent(node.transform.parent);
                    }
                    UnityEngine.Object.Destroy(node.gameObject);
                }

                foreach (var child in hierarchy.Relations[boneType].children)
                {
                    ExtractRig(child.boneType);
                }
            }
        }

        #endregion SkeletonMapping

        /// <summary>
        /// Attach an animated subskeleton to a skeleton
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="animatedSubskeleton"></param>
        protected virtual void AttachToSkeleton(ISkeleton parentSkeleton, AnimatedSubskeleton animatedSubskeleton)
        {
            // add animated skeleton to subskeleton list and re-order it by descending priority
            parentSkeleton.AddSubskeleton(animatedSubskeleton);;

            // if it is the browser, register that it is required to delete animated skeleton on leaving
            if (!isRegisteredForPersonalSkeletonCleanup)
            {
                isRegisteredForPersonalSkeletonCleanup = true;

                clientServer.OnLeavingEnvironment.AddListener(() => RemoveSkeletons((IPersonalSkeleton)parentSkeleton));
            }
        }

        private void RemoveSkeletons()
        {
            var skeleton = personalSkeletonService.PersonalSkeleton;

            RemoveSkeletons(skeleton);
        }

        private void RemoveSkeletons(IPersonalSkeleton skeleton)
        {
            foreach (var subskeleton in skeleton.Subskeletons.ToList())
            {
                if (subskeleton is IAnimatedSubskeleton animatedSubskeleton)
                    skeleton.RemoveSubskeleton(animatedSubskeleton);
            }
            clientServer.OnLeavingEnvironment.RemoveListener(() => RemoveSkeletons(skeleton));
            isRegisteredForPersonalSkeletonCleanup = false;
        }
    }
}