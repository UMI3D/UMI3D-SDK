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
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using System;
using System.Collections;
using umi3d.cdk.utils.extrapolation;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace umi3d.cdk.userCapture
{
    public interface ISkeleton
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;
        public Dictionary<uint, s_Transform> Bones { get; set; }
        public ISubSkeleton[] Skeletons { get; set; }

        #region Data struture
        public class s_Transform
        {
            public Vector3 s_Position;
            public Quaternion s_Rotation;
        }
        #endregion

        public abstract void UpdateFrame(UserTrackingFrameDto frame);

        #region Compute current skeleton
        public ISkeleton Compute()
        {
            if (CheckNulls())
            {
                return this;
            }

            for (int i = Skeletons.Length - 1; i >= 0; i--)
            {
                ISubSkeleton skeleton = Skeletons[i];
                List<BonePoseDto> bones = new List<BonePoseDto>();
                
                try
                {
                    bones = skeleton.GetPose().bones.ToList();
                }
                catch(Exception e)
                {
                    Debug.Log($"<color=red> _{e} </color>");
                    return this;
                }

                bones.ForEach(b =>
                {
                    if (b.rotation != null && b.position != null)
                    {
                        Bones.TryGetValue(b.boneType, out var pose);
                        if (pose != null)
                        {
                            Bones[b.boneType].s_Rotation = b.rotation;
                            Bones[b.boneType].s_Position = b.position;
                        }
                        else
                        {
                            Bones.TryAdd(b.boneType, new s_Transform()
                            {
                                s_Position= b.position,
                                s_Rotation= b.rotation
                            });
                        }
                    }
                });
            }

            return this;
        }

        private bool CheckNulls()
        {
            if (Bones == null)
            {
                Bones = new Dictionary<uint, s_Transform>();
            }

            if (Skeletons == null || Skeletons.Length == 0)
            {
                return true;
            }

            return false;
        }
        #endregion
       
    }
}