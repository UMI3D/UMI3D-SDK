/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Body pose animation component.
    /// </summary>
    [RequireComponent(typeof(umi3d.edk.UMI3DNodeAnimation))]
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class BodyAnimation : MonoBehaviour
    {
        /// <summary>
        /// name of the animation associated with the body pose.
        /// </summary>
        [Tooltip("Name of the animation associated with the body pose.")]
        public string AnimationName;
        /// <summary>
        /// Animation associated with the body pose.
        /// </summary>
        [Tooltip("Animation associated with the body pose.")]
        public UMI3DNodeAnimation NodeAnimation;
        /// <summary>
        /// Should the pose be active?
        /// </summary
        [Tooltip("Should the animation be active?")]
        public bool ActivePose = false;
        public bool AllowOverriding = false;
        /// <summary>
        /// Body pose associated with the animation.
        /// </summary
        [Tooltip("Body pose associated with the animation.")]
        public UMI3DBodyPose BodyPose;

        void Start()
        {
            if (NodeAnimation != null && BodyPose != null)
            {
                BodyPose.AllowOverriding = AllowOverriding;

                var op = new List<UMI3DNodeAnimation.OperationChain>();

                var operation = new SetEntityProperty()
                {
                    users = UMI3DServer.Instance.UserSet(),
                    entityId = BodyPose.Id(),
                    property = UMI3DPropertyKeys.ActiveBodyPose,
                    value = ActivePose
                };

                op.Add(
                    new UMI3DNodeAnimation.OperationChain()
                    {
                        Operation = operation,
                        progress = 0f
                    });

                NodeAnimation.ObjectAnimationChain.SetValue(op);
            }
        }
    }
}
