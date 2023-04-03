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
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    public class PoseSkeleton : ISubWritableSkeleton
    {
        public List<PoseDto> CurrentlyActivatedPoses = new List<PoseDto>();

        public void SetPose(bool isOveriding, List<PoseDto> poseToAdd)
        {
            if (isOveriding)
            {
                CurrentlyActivatedPoses.Clear();
            }

            CurrentlyActivatedPoses.AddRange(poseToAdd);
        }

        public void StopPose(bool areAll = true, List<PoseDto> posesToStop = null)
        {
            if (areAll)
            {
                CurrentlyActivatedPoses.Clear();
            }
            else
            {
                if (posesToStop != null)
                {
                    posesToStop.ForEach(pts =>
                    {
                        CurrentlyActivatedPoses.Remove(pts);
                    });
                }
            }
        }

        public PoseDto GetPose()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateFrame(UserTrackingFrameDto trackingFrame)
        {
            List<PoseDto> posesFromTrackingFrame = new List<PoseDto>();

            trackingFrame.playerUserPoses.ForEach(pup =>
            {
                posesFromTrackingFrame.Add(PoseManager.Instance.GetPose(trackingFrame.userId, pup));
            });

            trackingFrame.playerServerPoses.ForEach(psp =>
            {
                posesFromTrackingFrame.Add(PoseManager.Instance.GetPose(0, psp));
            });
        }

        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            throw new System.NotImplementedException();
        }

        public UserCameraPropertiesDto GetCameraDto()
        {
            return null;
        }
    }

}