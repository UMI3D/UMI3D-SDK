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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class TrackedSkeleton : MonoBehaviour, ISubSkeleton
    {
        public IController[] controllers;

        public Camera viewpoint;
        UMI3DClientUserTrackingBone[] trackedBones;


        public void Start()
        {
            controllers = GetComponentsInChildren<IController>();
            trackedBones = GetComponentsInChildren<UMI3DClientUserTrackingBone>();
        }

        public UserCameraPropertiesDto GetCameraDto()
        {
            return new UserCameraPropertiesDto()
            {
                scale = 1f,
                projectionMatrix = viewpoint.projectionMatrix,
                boneType = BoneType.Viewpoint,
            };
        }

        public PoseDto GetPose()
        {
            return new PoseDto()
            {
                bones = trackedBones.Select(tb => (AbstractBonePoseDto)tb.ToBonePose()).ToArray()
            };
        }

        public virtual void Update() { }

        public void Update(UserTrackingFrameDto trackingFrame)
        {
            //CRUD Distant controller in controllers
            throw new System.NotImplementedException();
        }

        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            trackingFrame.bones = trackedBones.Select(tb => tb.ToDto()).Where(b => b != null).ToList();
        }
    }

}