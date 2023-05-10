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


using System;
using System.Collections.Generic;
using System.Linq;

using umi3d.cdk.utils.extrapolation;
using umi3d.common;
using umi3d.common.userCapture;

using UnityEditor;

using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public abstract class AbstractSkeleton : MonoBehaviour, ISkeleton
    {
        protected const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        public virtual Dictionary<uint, ISkeleton.s_Transform> Bones { get; protected set; } = new();
        public virtual List<ISubSkeleton> Skeletons { get; protected set; } = new();

        [SerializeField]
        protected UMI3DSkeletonHierarchy serializedSkeletonHierarchy;
        public virtual Dictionary<uint, (uint boneTypeParent, Vector3 relativePosition)> SkeletonHierarchy => serializedSkeletonHierarchy.SkeletonHierarchy;
        
        public virtual Transform HipsAnchor { get => hipsAnchor; }
        public virtual ulong userId { get; protected set; }

        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator = new ();

        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator = new();

        public TrackedSkeleton TrackedSkeleton;
        public PoseSkeleton poseSkeleton = new PoseSkeleton();

        [SerializeField]
        protected Transform hipsAnchor;


        public ISkeleton Compute()
        {
            if (Skeletons == null || Skeletons.Count == 0)
            {
                return this;
            }

            foreach (ISubWritableSkeleton skeleton in Skeletons.OfType<ISubWritableSkeleton>())
            {
                List<BoneDto> bones = new List<BoneDto>();

                try
                {
                    bones = skeleton.GetPose().bones;
                }
                catch (Exception e)
                {
                    Debug.Log(skeleton.GetType().ToString());
                    Debug.Log($"<color=red> _{e} </color>");
                    return this;
                }

                foreach (var b in bones)
                {
                    {
                        Bones.TryGetValue(b.boneType, out var pose);
                        if (pose is not null)
                        {
                            Bones[b.boneType].s_Rotation = b.rotation;
                        }
                        else
                        {
                            Bones.TryAdd(b.boneType, new ISkeleton.s_Transform()
                            {
                                s_Rotation = b.rotation
                            });
                        }
                    }
                }

            }

            //very naïve
            if (!Bones.ContainsKey(BoneType.Hips))
            {
                Bones.Add(BoneType.Hips, new ISkeleton.s_Transform());
                Bones[BoneType.Hips].s_Position = HipsAnchor != null ? HipsAnchor.position : Vector3.zero;
            }

            foreach (uint boneType in alreadyComputedBonesCache.Keys.ToArray())
                alreadyComputedBonesCache[boneType] = false;

            foreach (uint boneType in Bones.Keys)
            {
                if (!alreadyComputedBonesCache.ContainsKey(boneType))
                    alreadyComputedBonesCache[boneType] = false;

                if (!alreadyComputedBonesCache[boneType])
                    ComputeBonePosition(boneType);
            }

            return this;
        }

        private Dictionary<uint, bool> alreadyComputedBonesCache = new();

        private void ComputeBonePosition(uint boneType)
        {
            if (!alreadyComputedBonesCache[boneType] && SkeletonHierarchy.TryGetValue(boneType, out var pose))
            {
                if (pose.boneTypeParent != BoneType.None)
                {
                    if (!alreadyComputedBonesCache[pose.boneTypeParent])
                        ComputeBonePosition(pose.boneTypeParent);
                    Bones[boneType].s_Position = Bones[pose.boneTypeParent].s_Position + Bones[pose.boneTypeParent].s_Rotation * pose.relativePosition;
                }
                else
                {
                    Bones[boneType].s_Position = pose.relativePosition;
                }
                alreadyComputedBonesCache[boneType] = true;
            }
        }

        public abstract void UpdateFrame(UserTrackingFrameDto frame);
    }
}
