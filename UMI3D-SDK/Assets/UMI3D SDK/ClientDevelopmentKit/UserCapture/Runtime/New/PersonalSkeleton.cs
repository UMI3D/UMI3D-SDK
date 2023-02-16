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
using umi3d.cdk.utils.extrapolation;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace umi3d.cdk.userCapture
{

    public class PersonalSkeleton : Singleton<PersonalSkeleton>, ISkeleton
    {
        protected const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;
        #region fields
        #region interface Fields
        Dictionary<uint, ISkeleton.s_Transform> ISkeleton.Bones { get => bones; set => bones = value; }
        List<ISubSkeleton> ISkeleton.Skeletons { get => skeletons; set => skeletons = value; }
        bool ISkeleton.activeUserBindings { get => activeUserBindings; set => activeUserBindings = value; }
        ulong ISkeleton.userId { get => userId; set => userId = value; }
        Vector3LinearDelayedExtrapolator ISkeleton.nodePositionExtrapolator { get => nodePositionExtrapolator; set => nodePositionExtrapolator = value; }
        QuaternionLinearDelayedExtrapolator ISkeleton.nodeRotationExtrapolator { get => nodeRotationExtrapolator; set => nodeRotationExtrapolator = value; }
        List<ISkeleton.Bound> ISkeleton.bounds { get => bounds; set => bounds = value; }
        List<Transform> ISkeleton.boundRigs { get => boundRigs; set => boundRigs = value; }
        List<BindingDto> ISkeleton.userBindings { get => userBindings; set => userBindings = value; }
        Dictionary<ulong, ISkeleton.SavedTransform> ISkeleton.savedTransforms { get => savedTransforms; set => savedTransforms = value; }

        #endregion
        protected Dictionary<uint, ISkeleton.s_Transform> bones = new Dictionary<uint, ISkeleton.s_Transform>();
        public List<ISubSkeleton> skeletons = new List<ISubSkeleton>();
        protected bool activeUserBindings;
        protected ulong userId;
        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator;
        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator;
        protected List<ISkeleton.Bound> bounds = new List<ISkeleton.Bound>();
        protected List<Transform> boundRigs = new List<Transform>();
        protected List<BindingDto> userBindings = new List<BindingDto>();
        protected Dictionary<ulong, ISkeleton.SavedTransform> savedTransforms = new Dictionary<ulong, ISkeleton.SavedTransform>();

        #endregion
        public TrackedSkeleton TrackedSkeleton;

        public float skeletonHighOffset = 0;

        public Vector3 worldSize => TrackedSkeleton.transform.lossyScale;



        public void Init()
        {
            skeletons = new List<ISubSkeleton>();
            skeletons[0] = TrackedSkeleton;
        }

        public UserTrackingFrameDto GetFrame(TrackingOption option) {
            var frame = new UserTrackingFrameDto()
            {
                position = PoseManager.Instance.transform.position,
                rotation = PoseManager.Instance.transform.rotation,
                skeletonHighOffset = skeletonHighOffset,
            };

            foreach (var skeleton in skeletons)
                skeleton.WriteTrackingFrame(frame, option);

            return frame;
        }

        public UserCameraPropertiesDto GetCameraProperty()
        {
            foreach(var skeleton in skeletons)
            {
                var c = skeleton.GetCameraDto();
                if(c != null)
                    return c;
            }
            return null;
        }

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            UMI3DLogger.LogWarning("The personal ISkeleton should not receive frame", scope);
        }
    }
}