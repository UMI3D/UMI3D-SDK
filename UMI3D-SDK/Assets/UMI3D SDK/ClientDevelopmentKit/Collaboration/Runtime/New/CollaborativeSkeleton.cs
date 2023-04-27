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
using umi3d.cdk.userCapture;
using umi3d.cdk.utils.extrapolation;
using umi3d.common.collaboration;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class CollaborativeSkeleton : MonoBehaviour, ISkeleton
    {
        #region fields
        #region interface Fields
        public Dictionary<uint, ISkeleton.s_Transform> Bones { get ; protected set; }
        List<ISubSkeleton> ISkeleton.Skeletons { get => skeletons; set => skeletons = value; }
        ulong ISkeleton.userId { get => userId; set => userId = value; }
        Vector3LinearDelayedExtrapolator ISkeleton.nodePositionExtrapolator { get => nodePositionExtrapolator; set => nodePositionExtrapolator = value; }
        QuaternionLinearDelayedExtrapolator ISkeleton.nodeRotationExtrapolator { get => nodeRotationExtrapolator; set => nodeRotationExtrapolator = value; }
        Dictionary<ulong, ISkeleton.SavedTransform> ISkeleton.savedTransforms { get => savedTransforms; set => savedTransforms = value; }
        Dictionary<uint, (uint, Vector3)> ISkeleton.SkeletonHierarchy { get => skeletonHierarchy; set => skeletonHierarchy = value; }
        Transform ISkeleton.HipsAnchor { get => hipsAnchor; set => hipsAnchor = value; }

        #endregion
        protected Dictionary<uint, ISkeleton.s_Transform> bones = new Dictionary<uint, ISkeleton.s_Transform>();
        protected List<ISubSkeleton> skeletons = new List<ISubSkeleton>();
        protected ulong userId;
        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator;
        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator;
        protected Dictionary<ulong, ISkeleton.SavedTransform> savedTransforms = new Dictionary<ulong, ISkeleton.SavedTransform>();
        protected Dictionary<uint, (uint, Vector3)> skeletonHierarchy = new Dictionary<uint, (uint, Vector3)>();
        protected Transform hipsAnchor;

        #endregion

        public UMI3DUser User;

        public TrackedSkeleton TrackedSkeleton;
        public PoseSkeleton poseSkeleton = new PoseSkeleton();

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            if (skeletons != null)
                foreach (ISubWritableSkeleton skeleton in skeletons.OfType<ISubWritableSkeleton>())
                    skeleton.UpdateFrame(frame);

            this.transform.position = frame.position;
            this.transform.rotation = frame.rotation;
        }

        public void SetSubSkeletons()
        {
            TrackedSkeleton = Instantiate((UMI3DCollaborationEnvironmentLoader.Parameters as UMI3DCollabLoadingParameters).CollabTrackedSkeleton, this.transform).GetComponent<TrackedSkeleton>();
            skeletons.Add(TrackedSkeleton);
            skeletons.Add(poseSkeleton);
            //skeletons.Add(new AnimatedSkeleton());
        }
    }
}