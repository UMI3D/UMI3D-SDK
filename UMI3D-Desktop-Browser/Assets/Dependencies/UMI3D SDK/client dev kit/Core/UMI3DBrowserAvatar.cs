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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DBrowserAvatar : Singleton<UMI3DBrowserAvatar>
    {
        public List<BoneType> BonesToFilter;
        public AvatarDto avatar;

        /// <summary>
        /// Iterate through the bones of the browser's skeleton to create BoneDto
        /// </summary>
        public void BonesIterator(AvatarDto avatar, Transform viewpoint)
        {
            List<BoneDto> bonesList = new List<BoneDto>();
            List<UMI3DBoneType> children = new List<UMI3DBoneType>();

            this.transform.GetComponentsInChildren<UMI3DBoneType>(children);
            foreach (UMI3DBoneType Item in children)
            {
                BoneDto boneInstance = null;

                if (Item.BoneType != BoneType.None)
                {
                    boneInstance = new BoneDto();
                    this.set_bone(boneInstance, Item.BoneType, Item, viewpoint);
                }
                if (boneInstance != null)
                {
                    bonesList.Add(boneInstance);
                }
            }
            avatar.BoneList = bonesList;
        }

        /// <summary>
        /// Set a BoneDto at a Transform object properties
        /// </summary>
        private void set_bone(BoneDto bone, BoneType type, UMI3DBoneType item, Transform viewpoint)
        {
            bone.type = type;
            bone.Position = viewpoint.transform.InverseTransformPoint(item.transform.position);
            bone.Position.X *= viewpoint.lossyScale.x;
            bone.Position.Y *= viewpoint.lossyScale.y;
            bone.Position.Z *= viewpoint.lossyScale.z;
            bone.Rotation = (Quaternion.Inverse(viewpoint.transform.rotation) * item.transform.rotation);
            bone.Scale = item.transform.lossyScale;
        }
    }
}
