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
using umi3d.common.userCapture.animation;
using UnityEngine;

namespace umi3d.edk.userCapture.animation
{
    /// <summary>
    ///  A Skeleton node is a subskeleton with a Unity Animator
    /// that is packaged in a bundle. It is loaded the same way as a Mesh.
    /// </summary>
    public class UMI3DSkeletonAnimationNode : UMI3DModel
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.EDK | DebugScope.UserCapture | DebugScope.Animation;

        /// <summary>
        /// List of states names in the embedded animator.
        /// </summary>
        [Header("Skeleton Animation Node")]
        [Tooltip("List of states names in the embedded animator."), EditorReadOnly]
        public List<string> animationStates = new();

        /// <summary>
        /// ID of the user owning this node. Their skeleton is the one affected by this node.
        /// </summary>
        [EditorReadOnly]
        public ulong userId;

        /// <summary>
        /// Collection of ID of UMI3D animations associated with this node.
        /// </summary>
        [EditorReadOnly]
        public ulong[] relatedAnimationIds = new ulong[0];

        /// <summary>
        /// Priority for application of skeleton animation.
        /// </summary>
        [EditorReadOnly]
        public uint priority;

        /// <summary>
        /// List of parameters that are updated by the browsers themselves based on skeleton movement.
        /// </summary>
        /// Available parameters are listed in <see cref="SkeletonAnimatorParameterKeys"/>.
        public SkeletonAnimationParameter[] animatorSelfTrackedParameters = new SkeletonAnimationParameter[0];

        public bool AreAnimationsGenerated => relatedAnimationIds.Length > 0;

        public Operation[] GetLoadAnimations(UMI3DUser user = null)
        {
            if (!AreAnimationsGenerated)
            {
                UMI3DLogger.LogWarning($"Cannot load animations, no animations are available. Try to generate them beforehand.", DEBUG_SCOPE);
                return null;
            }

            Queue<Operation> ops = new();
            foreach (var id in relatedAnimationIds)
            {
                var animation = UMI3DEnvironment.Instance._GetEntityInstance<UMI3DAnimatorAnimation>(id);
                ops.Enqueue(animation.GetLoadEntity(user is not null ? new() { user } : null));
            }
            return ops.ToArray();
        }

        public Operation[] GetDeleteAnimations(UMI3DUser user = null)
        {
            Queue<Operation> ops = new();
            foreach (var id in relatedAnimationIds)
            {
                var animation = UMI3DEnvironment.Instance._GetEntityInstance<UMI3DAnimatorAnimation>(id);
                ops.Enqueue(animation.GetDeleteEntity(user is not null ? new() { user } : null));
            }
            return ops.ToArray();
        }

        public Operation[] GenerateAnimations(IEnumerable<string> stateNames = null, bool arePlaying = true, bool areLooping = false, UMI3DNode node = null)
        {
            if (AreAnimationsGenerated)
            {
                UMI3DLogger.LogWarning($"Animations are already generated.", DEBUG_SCOPE);
                return null;
            }

            Queue<Operation> ops = new();
            Queue<ulong> animationsIdList = new();
            foreach (var animationState in stateNames ?? animationStates)
            {
                UMI3DAnimatorAnimation animation = gameObject.AddComponent<UMI3DAnimatorAnimation>();
                animation.Register();
                animation.objectNode.SetValue(node != null ? node : this);
                animation.objectLooping.SetValue(areLooping);
                animation.objectPlaying.SetValue(arePlaying);
                foreach (var parameter in animatorSelfTrackedParameters)
                {
                    animation.objectParameters.Add(((SkeletonAnimatorParameterKeys)parameter.parameterKey).ToString(), 0f);
                }
                animation.objectStateName.SetValue(animationState);

                ops.Enqueue(animation.GetLoadEntity());
                animationsIdList.Enqueue(animation.Id());
            }
            relatedAnimationIds = animationsIdList.ToArray();
            return ops.ToArray();
        }

        /// <inheritdoc/>
        protected override UMI3DNodeDto CreateDto()
        {
            return new SkeletonAnimationNodeDto();
        }

        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var skeletonNodeDto = dto as SkeletonAnimationNodeDto;
            skeletonNodeDto.userId = userId;
            skeletonNodeDto.relatedAnimationsId = relatedAnimationIds;
            skeletonNodeDto.priority = priority;
            skeletonNodeDto.animatorSelfTrackedParameters = animatorSelfTrackedParameters.Select(x=>x.ToDto()).ToArray();
        }

        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                    + UMI3DSerializer.Write(userId)
                    + UMI3DSerializer.Write(priority)
                    + UMI3DSerializer.WriteCollection(relatedAnimationIds)
                    + UMI3DSerializer.WriteCollection(animatorSelfTrackedParameters); //TODO : Add serializer for parameter
        }

        public void OnDestroy()
        {
            foreach (var id in relatedAnimationIds)
            {
                var animation = UMI3DEnvironment.Instance._GetEntityInstance<UMI3DAnimatorAnimation>(id);
                if (animation != null)
                {
                    UMI3DEnvironment.Instance.RemoveEntity(animation.Id());
                    UnityEngine.Object.Destroy(animation);
                }
            }
        }
    }
}