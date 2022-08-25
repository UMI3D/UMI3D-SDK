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
    [RequireComponent(typeof(umi3d.edk.UMI3DNodeAnimation))]
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class HandAnimation : MonoBehaviour
    {
        private UMI3DNodeAnimation nodeAnimation;
        private bool activePose = false;
        private bool hoverPose = false;
        private UMI3DHandPose handPose;

        public UMI3DNodeAnimation NodeAnimation
        {
            get => nodeAnimation; set
            {
                nodeAnimation = value;
                Start();
            }
        }
        public UMI3DHandPose HandPose
        {
            get => handPose; set
            {
                handPose = value;
                Start();
            }
        }
        public bool ActivePose
        {
            get => activePose; set
            {
                activePose = value;
                Start();
            }
        }
        public bool HoverPose
        {
            get => hoverPose; set
            {
                hoverPose = value;
                Start();
            }
        }

        // Start is called before the first frame update
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