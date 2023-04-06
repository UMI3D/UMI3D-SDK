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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Hand pose animation component.
    /// </summary>
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class UMI3DPoseAnimation : MonoBehaviour
    {
        /// <summary>
        /// Animation associated with the pose.
        /// </summary>
        protected List<UMI3DNodeAnimation> nodeAnimations;
        /// <summary>
        /// Should the pose be active?
        /// </summary>
        protected bool isStartPose = false;

        /// <summary>
        /// Hand pose associated with the animation.
        /// </summary>
        protected List<UMI3DPoseOverriderContainer> poseOverriderContainers;

        private void Awake()
        {
            
        }

        public void Init(bool isStartpose, List<UMI3DPoseOverriderContainer> poseOverriderContainer)
        {
            this.isStartPose = isStartpose;
            this.poseOverriderContainers = poseOverriderContainer;
        }

        /// <summary>
        /// Animation associated with the hand pose.
        /// </summary>
        public List<UMI3DNodeAnimation> NodeAnimations
        {
            get => nodeAnimations; set
            {
                nodeAnimations = value;
                Start();
            }
        }
        /// <summary>
        /// Hand pose associated with the animation.
        /// </summary>
        public List<UMI3DPoseOverriderContainer> PoseOverriderContainers
        {
            get => poseOverriderContainers; set
            {
                poseOverriderContainers = value;
                Start();
            }
        }
        /// <summary>
        /// Should the pose be active?
        /// </summary
        public bool IsStartPose
        {
            get => isStartPose; set
            {
                isStartPose = value;
                Start();
            }
        }

        private void Start()
        {
            SetUp();
        }

        public void SetUp()
        {
            if (NodeAnimations == null)
                NodeAnimations = GetComponents<UMI3DNodeAnimation>().ToList();

            if (NodeAnimations != null && PoseOverriderContainers != null)
            {
                for (int i =0; i < PoseOverriderContainers.Count; i++)
                {

                    var op = new List<UMI3DNodeAnimation.OperationChain>();

                    var operation = new SetEntityProperty()
                    {
                        users = UMI3DServer.Instance.UserSetWhenHasJoined(),
                        entityId = PoseOverriderContainers[i].Id(),
                        property = UMI3DPropertyKeys.ActivePoseOverrider,
                        value = IsStartPose
                    };

                    op.Add(
                        new UMI3DNodeAnimation.OperationChain()
                        {
                            Operation = operation,
                            progress = 0f
                        });

                    NodeAnimations[i].ObjectAnimationChain.SetValue(op);
                }
            }       
        }
    }
}
