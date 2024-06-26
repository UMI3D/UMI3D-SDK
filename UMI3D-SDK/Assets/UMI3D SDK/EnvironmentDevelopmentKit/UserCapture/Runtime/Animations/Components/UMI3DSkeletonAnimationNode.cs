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

        #region Fields

        /// <summary>
        /// List of states names in the embedded animator.
        /// </summary>
        [Header("Skeleton Animation Node")]
        [Tooltip("List of states names in the embedded animator."), EditorReadOnly]
        public List<string> animationStates = new();

        /// <summary>
        /// ID of the user owning this node. Their skeleton is the one affected by this node.
        /// </summary>
        [Tooltip("ID of the user owning this node. Their skeleton is the one affected by this node."), EditorReadOnly]
        public ulong userId;

        /// <summary>
        /// Collection of ID of UMI3D animations associated with this node.
        /// </summary>
        [Tooltip("Collection of ID of UMI3D animations associated with this node."), HideInInspector]
        public ulong[] relatedAnimationIds = new ulong[0];

        /// <summary>
        /// Priority for application of skeleton animation.
        /// </summary>
        [Tooltip("Priority for application of skeleton animation."), EditorReadOnly]
        public int priority;

        /// <summary>
        /// List of parameters that are updated by the browsers themselves based on skeleton movement.
        /// </summary>
        /// Available parameters are listed in <see cref="SkeletonAnimatorParameterKeys"/>.
        public SkeletonAnimationParameter[] animatorSelfTrackedParameters = new SkeletonAnimationParameter[0];

        /// <summary>
        /// True if the animation could be interpolated at start and end.
        /// </summary>
        /// Used for smooth transition to a skeleton animation.
        [Tooltip("True if the animation could be interpolated at start and end."), EditorReadOnly]
        public bool isInterpolable = true;

        #endregion Fields

        #region AnimationManagement

        /// <summary>
        /// True if UMI3D animatons have been generated from the skeleton animation node.
        /// </summary>
        public bool AreAnimationsGenerated => relatedAnimationIds.Length > 0;

        /// <summary>
        /// Get load operations for animations related to skeleton node.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public Operation[] GetLoadAnimations(IEnumerable<UMI3DUser> users = null)
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
                ops.Enqueue(animation.GetLoadEntity(users is not null ? users.ToHashSet() : null));
            }
            return ops.ToArray();
        }

        /// <summary>
        /// Get load operations for animations related to skeleton node.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Operation[] GetLoadAnimations(UMI3DUser user)
        {
            return GetLoadAnimations(user != null ? new UMI3DUser[1] { user } : null);
        }

        /// <summary>
        /// Get delete operations for animations relation to skeleton node.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public Operation[] GetDeleteAnimations(IEnumerable<UMI3DUser> users = null)
        {
            if (!AreAnimationsGenerated)
            {
                UMI3DLogger.LogWarning($"Cannot delete animations, no animations are generated.", DEBUG_SCOPE);
                return null;
            }

            Queue<Operation> ops = new();
            foreach (var id in relatedAnimationIds)
            {
                var animation = UMI3DEnvironment.Instance._GetEntityInstance<UMI3DAnimatorAnimation>(id);
                ops.Enqueue(animation.GetDeleteEntity(users is not null ? users.ToHashSet() : null));
            }
            return ops.ToArray();
        }

        /// <summary>
        /// Get load operations for animations related to skeleton node.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Operation[] GetDeleteAnimations(UMI3DUser user)
        {
            return GetDeleteAnimations(user != null ? new UMI3DUser[1] { user } : null);
        }

        /// <summary>
        /// Create all <see cref="UMI3DAnimatorAnimation"/> related to the node.
        /// </summary>
        /// <param name="stateNames">Name of the states for wich to create animation.</param>
        /// <param name="arePlaying">Are the animations already playing?</param>
        /// <param name="areLooping">Should the animations loop when they finish?</param>
        /// <param name="node">Node on which instantiate the animations.</param>
        /// <returns></returns>
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
                    object defaultValue = ((SkeletonAnimatorParameterKeys)parameter.parameterKey) switch
                    {
                        SkeletonAnimatorParameterKeys.SPEED => 0f,
                        SkeletonAnimatorParameterKeys.SPEED_X => 0f,
                        SkeletonAnimatorParameterKeys.SPEED_ABS_X => 0f,
                        SkeletonAnimatorParameterKeys.SPEED_Y => 0f,
                        SkeletonAnimatorParameterKeys.SPEED_ABS_Y => 0f,
                        SkeletonAnimatorParameterKeys.SPEED_Z => 0f,
                        SkeletonAnimatorParameterKeys.SPEED_ABS_Z => 0f,
                        SkeletonAnimatorParameterKeys.SPEED_X_Z => 0f,
                        SkeletonAnimatorParameterKeys.JUMP => false,
                        SkeletonAnimatorParameterKeys.CROUCH => false,
                        SkeletonAnimatorParameterKeys.GROUNDED => false,
                        _ => 0
                    };
                    animation.objectParameters.Add(parameter.parameterName, defaultValue);
                }
                animation.objectStateName.SetValue(animationState);

                ops.Enqueue(animation.GetLoadEntity());
                animationsIdList.Enqueue(animation.Id());
            }
            relatedAnimationIds = animationsIdList.ToArray();
            return ops.ToArray();
        }

        #endregion AnimationManagement

        #region Serialization

        /// <inheritdoc/>
        protected override UMI3DNodeDto CreateDto()
        {
            return new SkeletonAnimationNodeDto();
        }

        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            SkeletonAnimationNodeDto skeletonNodeDto = (SkeletonAnimationNodeDto)dto;
            skeletonNodeDto.userId = userId;
            skeletonNodeDto.relatedAnimationsId = relatedAnimationIds;
            skeletonNodeDto.priority = priority;
            skeletonNodeDto.animatorSelfTrackedParameters = animatorSelfTrackedParameters.Select(x => x.ToDto()).ToArray();
            skeletonNodeDto.IsInterpolable = isInterpolable;
        }

        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                    + UMI3DSerializer.Write(userId)
                    + UMI3DSerializer.Write(priority)
                    + UMI3DSerializer.WriteCollection(relatedAnimationIds)
                    + UMI3DSerializer.WriteCollection(animatorSelfTrackedParameters)
                    + UMI3DSerializer.Write(isInterpolable); //TODO : Add a complete serializer for parameter. no use here because it s always sent through bson.
        }

        #endregion Serialization

        public void OnDestroy()
        {
            // destroy related animations when destroying the skeleton animation node
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