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
    /// Hand pose animation component.
    /// </summary>
    [RequireComponent(typeof(UMI3DNode))]
    public class UMI3DPoseOverriderAnimation : MonoBehaviour
    {
        /// <summary>
        /// Animation associated with the pose.
        /// </summary>
        protected UMI3DNodeAnimation nodeAnimation;

        /// <summary>
        /// Hand pose associated with the animation.
        /// </summary>
        protected UMI3DPoseOverriderContainer poseOverriderContainer;

        public void Init(UMI3DPoseOverriderContainer poseOverriderContainer)
        {
            this.poseOverriderContainer = poseOverriderContainer;
            SetUp();
        }   

        /// <summary>
        /// Animation associated with the hand pose.
        /// </summary>
        public UMI3DNodeAnimation NodeAnimation
        {
            get => nodeAnimation; set
            {
                nodeAnimation = value;
            }
        }
        /// <summary>
        /// Hand pose associated with the animation.
        /// </summary>
        public UMI3DPoseOverriderContainer PoseOverriderContainer
        {
            get => poseOverriderContainer; set
            {
                poseOverriderContainer = value;
            }
        }

        public void SetUp()
        {
            if (NodeAnimation == null && PoseOverriderContainer != null)
            {
                nodeAnimation = (gameObject.AddComponent(typeof(UMI3DNodeAnimation)) as UMI3DNodeAnimation);

                nodeAnimation.Register();

                var op = new List<UMI3DNodeAnimation.OperationChain>();

                var operation = new SetEntityProperty()
                {
                    users = UMI3DServer.Instance.UserSetWhenHasJoined(),
                    entityId = PoseOverriderContainer.Id(),
                    property = UMI3DPropertyKeys.ActivePoseOverrider,
                    value = PoseOverriderContainer.IsStart
                };

                op.Add(
                    new UMI3DNodeAnimation.OperationChain()
                    {
                        Operation = operation,
                        progress = 0f
                    });

                nodeAnimation.ObjectAnimationChain.SetValue(op);       
            }
        }
    }
}
