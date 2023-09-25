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
    /// Loader called to load <see cref="SkeletonAnimationNodeDto"/>.
    /// </summary>
    /// It is based upon the <see cref="UMI3DMeshNodeDto"/> as the animation ressources are packaged in a bundle just like in a model.
    public class SkeletonAnimationNodeLoader : UMI3DMeshNodeLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        private bool isRegisteredForPersonalSkeletonCleanup;

        #region Dependency Injection

        protected readonly ISkeletonManager personnalSkeletonService;
        protected readonly IUMI3DClientServer clientServer;

        public SkeletonAnimationNodeLoader() : base()
        {
            personnalSkeletonService = PersonalSkeletonManager.Instance;
            clientServer = UMI3DClientServer.Instance;
        }

        public SkeletonAnimationNodeLoader(IEnvironmentManager environmentManager,
                                           ILoadingManager loadingManager,
                                           IUMI3DResourcesManager resourcesManager,
                                           ICoroutineService coroutineManager,
                                           ISkeletonManager personnalSkeletonService,
                                           IUMI3DClientServer clientServer)
            : base(environmentManager, loadingManager, resourcesManager, coroutineManager)
        {
            this.personnalSkeletonService = personnalSkeletonService;
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

            await Load(data.dto as SkeletonAnimationNodeDto);
        }

        public async Task Load(SkeletonAnimationNodeDto skeletonNodeDto)
        {
            UMI3DNodeInstance nodeInstance = environmentManager.GetNodeInstance(skeletonNodeDto.id);  //node exists because of base call of ReadUMI3DExtensiun

            // a skeleton node should contain an animator
            Animator animator = nodeInstance.gameObject.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                UMI3DLogger.LogWarning($"Cannot load skeleton animation {skeletonNodeDto.id}. No animator was found on node for user {skeletonNodeDto.userId}. ", DEBUG_SCOPE);
                return;
            }

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // required for applying movements on your body when you're not looking at it

            // get skeleton mapper from model or create one
            ISkeletonMapper skeletonMapper = GetSkeletonMapper(skeletonNodeDto, animator);
            if (skeletonMapper == null) // failed infinding/adding skeletonMapper
            {
                UMI3DLogger.LogWarning($"No skeleton mapper was provided for skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId} and cannot auto-extract from animator failed.", DEBUG_SCOPE);
                return;
            }

            var modelTracker = nodeInstance.gameObject.GetOrAddComponent<ModelTracker>();
            modelTracker.animatorsToRebind.Add(animator);

            // hide the model if it has any renderers
            foreach (var renderer in nodeInstance.gameObject.GetComponentsInChildren<Renderer>())
                renderer.gameObject.layer = LayerMask.NameToLayer("Invisible");

            _ = Task.Run(async () => // task is required to load asynchronously while not blocking the loading process
            {
                // get animation related to the skeleton node
                Queue<UMI3DAnimatorAnimation> animations = new(skeletonNodeDto.relatedAnimationsId.Length);
                foreach (var id in skeletonNodeDto.relatedAnimationsId)
                {
                    var instance = await loadingManager.WaitUntilEntityLoaded(id, null);
                    animations.Enqueue(instance.Object as UMI3DAnimatorAnimation);
                }

                // create subSkeleton and add it to a skeleton
                AnimatedSubskeleton animationSubskeleton = new(skeletonMapper, animations.ToArray(), skeletonNodeDto.priority, skeletonNodeDto.animatorSelfTrackedParameters);
                AttachToSkeleton(skeletonNodeDto.userId, animationSubskeleton);
            });
            nodeInstance.Delete = () => Delete(skeletonNodeDto.userId);

            await Task.CompletedTask;
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
        protected ISkeletonMapper GetSkeletonMapper(SkeletonAnimationNodeDto skeletonNodeDto, Animator animator)
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

                    skeletonMapper.BoneAnchor = new BonePoseDto() { bone = BoneType.Hips, position = root?.position.Dto(), rotation = root?.rotation.Dto() };
                }
                else
                {
                    UMI3DLogger.LogWarning($"SkeletonMapper found on skeleton node {skeletonNodeDto.id} for user {skeletonNodeDto.userId}, but no mapping could be retrieved.", DEBUG_SCOPE);
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
            skeletonMapper.BoneAnchor = new BonePoseDto() { bone = BoneType.Hips, position = animator.rootPosition.Dto(), rotation = animator.rootRotation.Dto() };

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
            skeletonMapper.Mappings = (from bone in boneUnityMapping
                                       where bone.transform != null
                                       select new SkeletonMapping(bone.umi3dBoneType, new GameNodeLink(bone.transform))).ToArray();

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
                    let relationUnity = (umi3dBoneType: relationUMI3D.Bonetype, unityBoneContainer: BoneTypeConvertingExtensions.ConvertToBoneType(relationUMI3D.Bonetype))
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
            var newHierachy = personnalSkeletonService.PersonalSkeleton.SkeletonHierarchy.Generate(animator.transform);

            var quickAccessHierarchy = newHierachy.Where(kv => kv.umi3dBoneType != BoneType.Viewpoint).ToDictionary(x => x.boneTransform.name, x => x);

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
                if (!quickAccessHierarchy.ContainsKey(node.name))
                    return;

                var (umi3dBoneType, boneTransform) = quickAccessHierarchy[node.name];

                var unityBoneName = BoneTypeConvertingExtensions.ConvertToBoneType(umi3dBoneType).ToString();
                var rigNameInAnimator = humanBoneRigRelations.ContainsKey(unityBoneName.ToLower()) ? humanBoneRigRelations[unityBoneName.ToLower()] : string.Empty;

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
                else // case that occurs if the bone is not found in hierarchy, lift children up and delete parent.
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

        #endregion SkeletonMapping

        /// <summary>
        /// Attach an animated subskeleton to a skeleton
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="animatedSubskeleton"></param>
        protected virtual void AttachToSkeleton(ulong userId, AnimatedSubskeleton animatedSubskeleton)
        {
            var skeleton = personnalSkeletonService.PersonalSkeleton;

            // add animated skeleton to subskeleton list and re-order it by descending priority
            skeleton.AddSubskeleton(animatedSubskeleton);

            // if it is the browser, register that it is required to delete animated skeleton on leaving
            if (!isRegisteredForPersonalSkeletonCleanup)
            {
                isRegisteredForPersonalSkeletonCleanup = true;

                clientServer.OnLeavingEnvironment.AddListener(() => RemoveSkeletons(skeleton));
            }
        }

        private void RemoveSkeletons()
        {
            var skeleton = personnalSkeletonService.PersonalSkeleton;

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