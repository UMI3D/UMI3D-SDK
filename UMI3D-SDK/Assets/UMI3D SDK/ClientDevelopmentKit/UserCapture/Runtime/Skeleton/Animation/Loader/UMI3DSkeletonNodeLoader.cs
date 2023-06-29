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
using umi3d.common.userCapture.animation;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.animation
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
        protected readonly UMI3DEnvironmentLoader environmentLoader;

        public UMI3DSkeletonNodeLoader() : base()
        {
            personnalSkeletonService = PersonalSkeletonManager.Instance;
            environmentLoader = UMI3DEnvironmentLoader.Instance;
        }

        public UMI3DSkeletonNodeLoader(ISkeletonManager personnalSkeletonService, UMI3DEnvironmentLoader environmentLoader) : base()
        {
            this.personnalSkeletonService = personnalSkeletonService;
            this.environmentLoader = environmentLoader;
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

            await Load(data.dto as UMI3DSkeletonNodeDto);
        }

        public async Task Load(UMI3DSkeletonNodeDto skeletonNodeDto)
        {
            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(skeletonNodeDto.id);  //node exists because of base call of ReadUMI3DExtensiun

            // a skeleton node should contain an animator
            Animator animator = nodeInstance.gameObject.GetComponentInChildren<Animator>();
            if (animator == null)
                return;

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // required for applying movements on your body when you're not looking at it

            // get skeleton mapper from model or create one
            SkeletonMapper skeletonMapper = GetSkeletonMapper(skeletonNodeDto, animator);
            if (skeletonMapper == null) // failed infinding/adding skeletonMapper
            {
                UMI3DLogger.LogWarning($"No skeleton mapper was provided for skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId} and cannot auto-extract from animator failed", DEBUG_SCOPE);
                return;
            }

            var modelTracker = nodeInstance.gameObject.GetOrAddComponent<ModelTracker>();
            modelTracker.animatorsToRebind.Add(animator);

            // get animation related to the skeleton node
            Queue<UMI3DAnimatorAnimation> animations = new(skeletonNodeDto.relatedAnimationsId.Length);
            foreach (var id in skeletonNodeDto.relatedAnimationsId)
            {
                var instance = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(id, null);
                if (instance.Object is not UMI3DAnimatorAnimation animation)
                {
                    UMI3DLogger.LogWarning($"Unable to get animation {id} for Skeleton Node {skeletonNodeDto.id} for user {skeletonNodeDto.userId}.", DEBUG_SCOPE);
                    continue;
                }
                    
                animations.Enqueue(animation);
            }

            // create subSkeleton and add it to a skeleton
            AnimatedSkeleton animationSubskeleton = new(skeletonMapper, animations.ToArray(), skeletonNodeDto.priority, skeletonNodeDto.animatorSelfTrackedParameters);
            AttachToSkeleton(skeletonNodeDto.userId, animationSubskeleton);

            // hide the model if it has any renderers
            foreach (var renderer in nodeInstance.gameObject.GetComponentsInChildren<Renderer>())
                renderer.gameObject.layer = LayerMask.NameToLayer("Invisible");
        }

        /// <summary>
        /// Extract skeleton mapper from skeleton or create one
        /// </summary>
        /// <param name="skeletonNodeDto"></param>
        /// <param name="animator"></param>
        /// <returns></returns>
        protected SkeletonMapper GetSkeletonMapper(UMI3DSkeletonNodeDto skeletonNodeDto, Animator animator)
        {
            // if the designer added a skeleton mapper, uses its links
            if (animator.gameObject.TryGetComponent(out SkeletonMapper skeletonMapper))
            {
                skeletonMapper.Mappings = skeletonMapper.GetComponentsInChildren<SkeletonMappingLinkMarker>()
                                                            .Select(x => x.ToSkeletonMapping())
                                                            .ToArray();
                if (skeletonMapper.Mappings.Length > 0)
                {
                    var root = skeletonMapper.Mappings.FirstOrDefault(x => x.BoneType == BoneType.Hips)?.Link.Compute();

                    skeletonMapper.BoneAnchor = new BonePoseDto() { Bone = BoneType.Hips, Position = root?.position.Dto(), Rotation = root?.rotation.Dto() };
                }
                else
                {
                    UMI3DLogger.LogWarning($"SkeletonMapper found on skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId}, but no mapping could be retrieved.", DEBUG_SCOPE);
                    return null;
                }
            }
            else if (animator.avatar.isHuman)// if null, we try to adapt the unity avatar (rigs) by ourselves assuming it is close to the UMI3D standard one
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
        protected SkeletonMapper AutoMapAnimatorSkeleton(Animator animator, UMI3DSkeletonNodeDto skeletonNodeDto)
        {
            SkeletonMapper skeletonMapper = animator.gameObject.AddComponent<SkeletonMapper>();

            // umi3d default anchor is hips
            skeletonMapper.BoneAnchor = new BonePoseDto() { Bone = BoneType.Hips, Position = animator.rootPosition.Dto(), Rotation = animator.rootRotation.Dto() };

            // map animator unity bones to umi3d ones
            var boneUnityMapping = FindBonesTransform(animator);

            // if no bone can be mapped, then extract from animator
            if (boneUnityMapping.All(x => x.transform == null))
            {
                ExtractRigsFromAnimator(animator);
                animator.Rebind();
                boneUnityMapping = FindBonesTransform(animator);

                // if still no bones can be retrieved, the avatar mask in the animator cannot be adapted
                if (boneUnityMapping.All(x => x.transform == null))
                {
                    UMI3DLogger.LogWarning($"No skeleton mapper was provided for skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId} and attempt to auto-extract from animator failed", DEBUG_SCOPE);
                    return null;
                }
            }

            // create link for each rig. May be improved with distance analysis for more complex links
            skeletonMapper.Mappings = boneUnityMapping.Select(b => new SkeletonMapping(b.umi3dBoneType, new GameNodeLink(b.transform))).ToArray();

            return skeletonMapper;
        }

        /// <summary>
        /// Look for bones contained in an animator and their associated transform.
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        protected (uint umi3dBoneType, Transform transform)[] FindBonesTransform(Animator animator)
        {
            return (UMI3DEnvironmentLoader.Parameters as UMI3DUserCaptureLoadingParameters).SkeletonHierarchyDefinition.BoneRelations
                            .Select(x => (umi3dBoneType: x.Bonetype, unityBoneContainer: BoneTypeConvertingExtensions.ConvertToBoneType(x.Bonetype)))
                            .Where(x => x.unityBoneContainer.HasValue)
                            .Select(x => (x.umi3dBoneType, transform: animator.GetBoneTransform(x.unityBoneContainer.Value)))
                            .ToArray();
        }

        /// <summary>
        /// Create a gameobject as a child for each bone declared in the animator.
        /// </summary>
        /// This implies a flat hierarchy.
        /// <param name="animator"></param>
        protected void ExtractRigsFromAnimator(Animator animator)
        {
            var newHierachy = personnalSkeletonService.personalSkeleton.SkeletonHierarchy.Generate(animator.transform);

            var quickAccessHierarchy = newHierachy.ToDictionary(x => x.boneTransform.name, x => x);

            static string RemoveWhiteSpaces(string s)
            {
                return string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
            }

            // unity name -> mecanim name / rigname
            var humanBoneRigRelations = animator.avatar.humanDescription.human.ToDictionary(x => RemoveWhiteSpaces(x.humanName).ToLower(), x => x.boneName);

            // rig name in animator -> local transform infos
            var boneInfoInAnimator = animator.avatar.humanDescription.skeleton.ToDictionary(x => x.name, x => (x.position, x.rotation, x.scale));

            var root = newHierachy.First(x => x.umi3dBoneType == BoneType.Hips).boneTransform;

            Compute(root);

            void Compute(Transform node)
            {
                var (umi3dBoneType, boneTransform) = quickAccessHierarchy[node.name];

                var unityBoneName = BoneTypeConvertingExtensions.ConvertToBoneType(umi3dBoneType).ToString();
                var rigNameInAnimator = humanBoneRigRelations[unityBoneName.ToLower()];

                if (boneInfoInAnimator.ContainsKey(rigNameInAnimator))
                {
                    var (position, rotation, scale) = boneInfoInAnimator[rigNameInAnimator];

                    boneTransform.name = rigNameInAnimator;
                    boneTransform.localPosition = position;
                    boneTransform.localRotation = rotation;
                    boneTransform.localScale = scale;

                    for (int i = 0; i < node.childCount; i++)
                    {
                        Compute(node.GetChild(i));
                    }
                }
                else // case that occurs if the bone is not found in hierarchy
                {
                    var liftedNodes = new List<Transform>();
                    for (int i = 0; i < node.childCount; i++)
                    {
                        liftedNodes.Add(node.GetChild(i));
                    }
                    foreach (var liftedNode in liftedNodes)
                    {
                        node.SetParent(node.transform.parent);
                        Compute(liftedNode);
                    }
                    UnityEngine.Object.Destroy(node.gameObject);
                }
            }
        }

        /// <summary>
        /// Attach an animated subskeleton to a skeleton
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="subskeleton"></param>
        protected virtual void AttachToSkeleton(ulong userId, AnimatedSkeleton subskeleton)
        {
            var skeleton = personnalSkeletonService.personalSkeleton;

            // add animated skeleton to subskeleton list and re-order it by descending priority
            var animatedSkeletons = skeleton.Skeletons
                                        .Where(x => x is AnimatedSkeleton)
                                        .Cast<AnimatedSkeleton>()
                                        .Append(subskeleton)
                                        .OrderByDescending(x => x.Priority).ToList();

            personnalSkeletonService.personalSkeleton.Skeletons.RemoveAll(x => x is AnimatedSkeleton);
            personnalSkeletonService.personalSkeleton.Skeletons.AddRange(animatedSkeletons);

            // if some animator parameters should be updated by the browsers itself, start listening to them
            if (subskeleton.SelfUpdatedAnimatorParameters.Length > 0)
                subskeleton.StartParameterSelfUpdate(skeleton);
        }
    }
}