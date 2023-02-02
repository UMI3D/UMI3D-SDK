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
        protected Dictionary<uint, s_Transform> Bones { get; set; }
        public bool activeUserBindings { get; protected set; }
        ISubSkeleton[] Skeletons { get; set; }
        protected class s_Transform
        {
            public Vector3 s_Position;
            public Quaternion s_Rotation;
        }

        public abstract void UpdateFrame(UserTrackingFrameDto frame);

        /// <summary>
        /// List of currently applied <see cref="BoneBindingDto"/> to the user's skeleton.
        /// </summary>
        public List<BoneBindingDto> userBindings { get; protected set; }

        public ISkeleton Compute()
        {
            if (CheckNulls())
            {
                return this;
            }

            for (int i = Skeletons.Length - 1; i > 0; i--)
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

        #region bindings
        public void AddBinding(int index, BoneBindingDto boneBinding) 
        {
            if (index <= userBindings.Count - 1)
            {
                BoneBindingDto dtoAtIndex = userBindings[index];

                if (!boneBinding.bindingId.Contains(dtoAtIndex.bindingId) && !dtoAtIndex.bindingId.Contains(boneBinding.bindingId))
                    AddBinding_(index, boneBinding);
            }
            else
            {
                AddBinding_(index, boneBinding);
            }
        }

        private void AddBinding_(int index, BoneBindingDto boneBinding)
        {
            userBindings.Insert(index, boneBinding);
            if (activeUserBindings && boneBinding.active)
                //UpdateBindingPosition(boneBinding);
        }

        public void UnBind(BoneBindingDto boneBinding)
        {
        
        }
        #endregion
    }
}