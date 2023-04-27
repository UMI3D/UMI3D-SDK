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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using System;
using umi3d.cdk.utils.extrapolation;
using System.Threading.Tasks;
using System.Collections;
using umi3d.cdk;
using inetum.unityUtils;
using UnityEngine.UIElements;

namespace umi3d.cdk.userCapture
{
    public interface ISkeleton
    {
        protected const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;
        public Dictionary<uint, s_Transform> Bones { get; }
        List<ISubSkeleton> Skeletons { get; set; }

        Dictionary<uint, (uint, Vector3)> SkeletonHierarchy { get; set; }

        protected Transform HipsAnchor { get; set; }
        
        #region Data struture
        public class s_Transform
        {
            public Vector3 s_Position;
            public Vector4 s_Rotation;
        }
        #endregion

        public abstract void UpdateFrame(UserTrackingFrameDto frame);

        /// <summary>
        /// Only Call once
        /// </summary>
        public void Init()
        {
            UMI3DSkeletonHierarchy.SetSkeletonHierarchy.AddListener(() => this.SkeletonHierarchy = UMI3DSkeletonHierarchy.SkeletonHierarchy);

            if (Skeletons == null) Skeletons = new List<ISubSkeleton>();
            if (savedTransforms == null) savedTransforms = new Dictionary<ulong, SavedTransform>();
        }

        #region Compute current skeleton
        public ISkeleton Compute()
        {
            if (CheckNulls())
            {
                return this;
            }

            foreach (ISubWritableSkeleton skeleton in Skeletons.OfType<ISubWritableSkeleton>().Reverse())
            {
                List<BoneDto> bones = new List<BoneDto>();

                try
                {
                    bones = skeleton.GetPose().bones.ToList();
                }
                catch (Exception e)
                {
                    Debug.Log($"<color=red> _{e} </color>");
                    return this;
                }

                bones.ForEach(b =>
                {
                    if (b.rotation != null)
                    {
                        Bones.TryGetValue(b.boneType, out var pose);
                        if (pose != null)
                        {
                            Bones[b.boneType].s_Rotation = b.rotation;
                        }
                        else
                        {
                            Bones.TryAdd(b.boneType, new s_Transform()
                            {
                                s_Rotation = b.rotation
                            });
                        }
                    }
                });

            }

            //very naïve
            Bones.Add(BoneType.Hips, new s_Transform());
            Bones[BoneType.Hips].s_Position = HipsAnchor != null ? HipsAnchor.position : Vector3.zero;

            foreach (uint boneType in Bones.Keys)
            {
                ComputeBonePosition(boneType);
            }

            return this;
        }

        private void ComputeBonePosition(uint boneType)
        {
            if (Bones[boneType].s_Position == null && SkeletonHierarchy.TryGetValue(boneType, out var pose))
            {
                if (Bones[pose.Item1].s_Position == null)
                    ComputeBonePosition(pose.Item1);

                Bones[boneType].s_Position = Bones[pose.Item1].s_Position + Bones[pose.Item1].s_Rotation.ToQuaternion() * pose.Item2;
            }
        }

        private bool CheckNulls()
        {
            if (Skeletons == null || Skeletons.Count == 0)
            {
                return true;
            }

            return false;
        }
        #endregion

        /// <summary>
        /// User's registered id
        /// </summary>
        public ulong userId { get; protected set; }

        /// <summary>
        /// Extrapolator for the avatar position.
        /// </summary>
        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator { get; set; }

        /// <summary>
        /// Extrapolator for the avatar rotation.
        /// </summary>
        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator { get; set; }
        
        ///// <summary>
        ///// List of currently applied <see cref="BindingDto"/> to the user's skeleton.
        ///// </summary>
        //public List<BindingDto> userBindings { get; protected set; }
        /// <summary>
        /// Saves of the transform of objects before they had been bound to a user's bone.
        /// </summary>
        protected Dictionary<ulong, SavedTransform> savedTransforms { get; set; }
        public Dictionary<ulong, SavedTransform> SavedTransforms { get => savedTransforms; }

        #region Data Structure
        public struct SavedTransform
        {
            public Transform obj;
            public Vector3 savedPosition;
            public Quaternion savedRotation;
            public Vector3 savedLocalScale;
            public Vector3 savedLossyScale;
        }
        #endregion
    }
}