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

namespace umi3d.cdk.userCapture
{
    public interface ISkeleton
    {
        Dictionary<uint, Transform> Bones { get; set; }
        List<ISubSkeleton> Skeletons { get; set; }

        public abstract void UpdateFrame(UserTrackingFrameDto frame);

        public ISkeleton Compute()
        {
            if (CheckNulls())
            {
                return this;
            }

            for (int i = Skeletons.Count - 1; i > 0; i--)
            {
                ISubSkeleton skeleton = Skeletons[i];
                List<BonePoseDto> bones = new List<BonePoseDto>();
                
                try
                {
                    bones = skeleton.GetPose().bones.ToList();
                }
                catch(Exception e)
                {
                    Debug.Log($"<color=red> {e} </color>");
                    return this;
                }

                bones.ForEach(b =>
                {
                    if (b.rotation != null && b.position != null)
                    {
                        Bones.TryGetValue(b.boneType, out var pose);
                        if (pose != null)
                        {
                            Bones[b.boneType].rotation = b.rotation;
                            Bones[b.boneType].position = b.position;
                        }
                        else
                        {
                            GameObject new_obj = new GameObject();
                            Transform new_trans = new_obj.transform;
                            new_trans.position = b.position;
                            new_trans.rotation = b.rotation;
                            Bones.TryAdd(b.boneType, new_trans);
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
                Bones = new Dictionary<uint, Transform>();
            }

            if (Skeletons == null || Skeletons.Count == 0)
            {
                return true;
            }

            return false;
        }
        public void Bind(BoneBindingDto boneBinding) { }


    }
}