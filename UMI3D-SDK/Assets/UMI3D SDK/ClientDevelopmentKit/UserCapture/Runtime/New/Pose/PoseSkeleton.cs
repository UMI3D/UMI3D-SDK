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

namespace umi3d.cdk.userCapture
{
    public class PoseSkeleton : ISubWritableSkeleton
    {
        public List<PoseDto> localCurrentlyActivatedPoses = new List<PoseDto>();
        public List<PoseDto> serverCurrentlyActivatedPoses = new List<PoseDto>();

        public void SetPose(bool isOveriding, List<PoseDto> poseToAdd, bool isServerPose)
        {
            if (isOveriding)
            {
                serverCurrentlyActivatedPoses.Clear();
                localCurrentlyActivatedPoses.Clear();
            }

            if (isServerPose)
            {
                serverCurrentlyActivatedPoses.AddRange(poseToAdd);
            }
            else
            {
                localCurrentlyActivatedPoses.AddRange(poseToAdd);
            }
        }

        public void StopPose(bool areAll = true, List<PoseDto> posesToStop = null, bool isServerPose = false)
        {
            if (areAll)
            {
                localCurrentlyActivatedPoses.Clear();
            }
            else
            {
                if (posesToStop != null)
                {
                    posesToStop.ForEach(pts =>
                    {
                        localCurrentlyActivatedPoses.Remove(pts);
                    });
                }
            }
        }

        public PoseDto GetPose()
        {
            PoseDto poseDto = new PoseDto();

            for (int i = 0; i < localCurrentlyActivatedPoses?.Count; i++)
            {
                for (int j = 0; j < localCurrentlyActivatedPoses[i].bones?.Count; j++)
                {
                    int indexOf = poseDto.bones.IndexOf(localCurrentlyActivatedPoses[i].bones[j]);
                    if (indexOf != -1)
                    {
                        poseDto.bones[indexOf] = localCurrentlyActivatedPoses[i].bones[j];
                    }
                    else
                    {
                        poseDto.bones.Add(localCurrentlyActivatedPoses[i].bones[j]);
                    }
                }
            }

            for (int i = 0; i < serverCurrentlyActivatedPoses?.Count; i++)
            {
                for (int j = 0; j < serverCurrentlyActivatedPoses[i].bones?.Count; j++)
                {
                    int indexOf = poseDto.bones.IndexOf(serverCurrentlyActivatedPoses[i].bones[j]);
                    if (indexOf != -1)
                    {
                        poseDto.bones[indexOf] = serverCurrentlyActivatedPoses[i].bones[j];
                    }
                    else
                    {
                        poseDto.bones.Add(serverCurrentlyActivatedPoses[i].bones[j]);
                    }
                }
            }

            return poseDto;
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