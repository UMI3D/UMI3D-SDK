﻿/*
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
    [RequireComponent(typeof(umi3d.edk.UMI3DNodeAnimation))]
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class HandAnimation : MonoBehaviour
    {
        /// <summary>
        /// Animation associated with the hand pose.
        /// </summary>
        private UMI3DNodeAnimation nodeAnimation;
        /// <summary>
        /// Should the pose be active?
        /// </summary>
        private bool activePose = false;
        /// <summary>
        /// Should the animation be triggered on hover/pointing?
        /// </summary>
        private bool hoverPose = false;
        /// <summary>
        /// Hand pose associated with the animation.
        /// </summary>
        private UMI3DHandPose handPose;
      
        /// <summary>
        /// Animation associated with the hand pose.
        /// </summary>
        [Tooltip("Animation associated with the hand pose.")]
        public UMI3DNodeAnimation NodeAnimation
        {
            get => nodeAnimation; set
            {
                nodeAnimation = value;
                Start();
            }
        }
        /// <summary>
        /// Hand pose associated with the animation.
        /// </summary>
        [Tooltip("Hand pose associated with the animation.")]
        public UMI3DHandPose HandPose
        {
            get => handPose; set
            {
                handPose = value;
                Start();
            }
        }
        /// <summary>
        /// Should the pose be active?
        /// </summary
        [Tooltip("Should the animation be active?")]
        public bool ActivePose
        {
            get => activePose; set
            {
                activePose = value;
                Start();
            }
        }
        /// <summary>
        /// Should the animation be triggered on hover/pointing?
        /// </summary>
        [Tooltip("Should the animation be triggered on hover/pointing?")]
        public bool HoverPose
        {
            get => hoverPose; set
            {
                hoverPose = value;
                Start();
            }
        }


        private void Start()
        {
            if (NodeAnimation != null && HandPose != null)
            {
                HandPose.HoverAnimation = HoverPose;

                var op = new List<UMI3DNodeAnimation.OperationChain>();

                var operation = new SetEntityProperty()
                {
                    users = UMI3DServer.Instance.UserSetWhenHasJoined(),
                    entityId = HandPose.Id(),
                    property = UMI3DPropertyKeys.ActiveHandPose,
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