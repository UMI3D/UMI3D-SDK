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
    public class BodyAnimation : MonoBehaviour
    {
        public string AnimationName;
        public UMI3DNodeAnimation NodeAnimation;
        public bool ActivePose = false;
        public bool AllowOverriding = false;
        public UMI3DBodyPose BodyPose;

        // Start is called before the first frame update
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
