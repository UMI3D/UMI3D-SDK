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
using umi3d.cdk.userCapture;
using umi3d.cdk.utils.extrapolation;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class CollaborativeSkeleton : AbstractSkeleton
    {

        public UMI3DUser User;

        public Dictionary<uint, Transform> Bones { get; set; }
        public ISubSkeleton[] Skeletons { get; set; }
        ISubSkeleton[] ISkeleton.Skeletons { get => Skeletons; set { Skeletons = value; } }
        #region Iskeleeton Fields
        /// <summary>
        /// stores the different rotation and position of the bones
        /// </summary>
        Dictionary<uint, ISkeleton.s_Transform> ISkeleton.Bones { get; set; }
        /// <summary>
        /// Saves of the transform of objects before they had been bound to a user's bone.
        /// </summary>
        Dictionary<ISkeleton.BoundObject, ISkeleton.SavedTransform> ISkeleton.savedTransforms { get; set; }

        /// <summary>
        /// Has the user currently active bindings?
        /// </summary>
        bool ISkeleton.activeUserBindings { get; set; }
        /// <summary>
        /// User's registered id
        /// </summary>
        ulong ISkeleton.userId { get; set; }
        List<ISkeleton.Bound> ISkeleton.bounds { get; set; }
        List<Transform> ISkeleton.boundRigs { get; set; }
        List<BoneBindingDto> ISkeleton.userBindings { get; set; }
        /// <summary>
        /// Extrapolator for the avatar position.
        /// </summary>
        Vector3LinearDelayedExtrapolator ISkeleton.nodePositionExtrapolator { get; set; }
        /// <summary>
        /// Extrapolator for the avatar rotation.
        /// </summary>
        QuaternionLinearDelayedExtrapolator ISkeleton.nodeRotationExtrapolator { get; set; }

        #endregion

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            if (Skeletons != null)
                foreach (var skeleton in Skeletons)
                    skeleton.Update(frame);
        }

    }
}