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
using umi3d.common.userCapture;
using umi3d.edk;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    [RequireComponent(typeof(umi3d.edk.UMI3DNodeAnimation))]
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class HandAnimation : MonoBehaviour
    {
        //public UMI3DNode AnimatedNode;
        public UMI3DNodeAnimation NodeAnimation;
        public bool ActivePose = false;
        public UMI3DHandPose HandPose;


        // Start is called before the first frame update
        void Start()
        {
            if (NodeAnimation != null && HandPose != null)
            {
                if (HandPose.isRelativeToNode)
                    HandPose.RelativeNodeId = this.GetComponent<UMI3DNode>().Id();
                else
                    HandPose.RelativeNodeId = null;

                HashSet<UMI3DUser> users = new HashSet<UMI3DUser>(UMI3DEnvironment.GetEntities<UMI3DUser>());

                List<UMI3DNodeAnimation.OperationChain> op = new List<UMI3DNodeAnimation.OperationChain>();

                SetEntityProperty operation = new SetEntityProperty()
                {
                    users = users,
                    entityId = HandPose.Id(),
                    property = UMI3DPropertyKeys.ActiveHandPose,
                    value = ActivePose
                };

                op.Add(
                    new UMI3DNodeAnimation.OperationChain()
                    {
                        Operation = operation,
                        });

                NodeAnimation.ObjectAnimationChain.SetValue(op);
            }
        }
    }
}