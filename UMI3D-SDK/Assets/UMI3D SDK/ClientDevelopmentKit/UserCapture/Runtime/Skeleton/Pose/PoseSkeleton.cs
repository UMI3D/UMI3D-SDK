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
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;

namespace umi3d.cdk.userCapture.pose
{
    public class PoseSkeleton : ISubWritableSkeleton
    {
        private readonly IPoseManager poseManagerService;

        public PoseSkeleton()
        {
            poseManagerService = PoseManager.Instance;
        }

        public PoseSkeleton(IPoseManager poseManager)
        {
            this.poseManagerService = poseManager;
        }

        private List<PoseDto> localCurrentlyActivatedPoses = new List<PoseDto>();
        private List<PoseDto> serverCurrentlyActivatedPoses = new List<PoseDto>();

        public List<PoseDto> GetLocalPoses()
        {
            List<PoseDto> poses = new List<PoseDto>();
            poses.AddRange(localCurrentlyActivatedPoses);
            return poses;
        }

        public List<PoseDto> GetServerPoses()
        {
            List<PoseDto> poses = new List<PoseDto>();
            poses.AddRange(serverCurrentlyActivatedPoses);
            return poses;
        }

        /// <summary>
        /// Set a pose for the calculation of the next tracking frame
        /// </summary>
        /// <param name="isOveriding"></param>
        /// <param name="poseToAdd"></param>
        /// <param name="isServerPose"></param>
        public void SetPose(bool isOveriding, List<PoseDto> poseToAdd, bool isServerPose)
        {
            if (poseToAdd == null) return;
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

        /// <summary>
        /// Stops a specific set of poses
        /// </summary>
        /// <param name="posesToStop"></param>
        /// <param name="isServerPose"></param>
        public void StopPose(List<PoseDto> posesToStop = null, bool isServerPose = false)
        {
            if (posesToStop != null)
            {
                if (isServerPose)
                {
                    posesToStop.ForEach(pts =>
                    {
                        serverCurrentlyActivatedPoses.Remove(pts);
                    });
                }
                else
                {
                    posesToStop.ForEach(pts =>
                    {
                        localCurrentlyActivatedPoses.Remove(pts);
                    });
                }
            }
        }

        /// <summary>
        /// stops all the poses s
        /// </summary>
        public void StopAllPoses()
        {
            localCurrentlyActivatedPoses.Clear();
            serverCurrentlyActivatedPoses.Clear();
        }

        /// <summary>
        ///last in has priority,,, server poses have priority
        /// </summary>
        /// <returns></returns>
        public PoseDto GetPose()
        {
            PoseDto poseDto = new PoseDto() { bones = new List<BoneDto>() };
            for (int i = 0; i < localCurrentlyActivatedPoses?.Count; i++)
            {
                for (int j = 0; j < localCurrentlyActivatedPoses[i].bones?.Count; j++)
                {
                    int indexOf = poseDto.bones.FindIndex(a => a.boneType == localCurrentlyActivatedPoses[i].bones[j].boneType);
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
                    int indexOf = poseDto.bones.FindIndex(a => a.boneType == serverCurrentlyActivatedPoses[i].bones[j].boneType);
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

        /// <summary>
        /// Updates the state of the pose manager using the tracking frame
        /// </summary>
        /// <param name="trackingFrame"></param>
        public void UpdateFrame(UserTrackingFrameDto trackingFrame)
        {
            localCurrentlyActivatedPoses.Clear();
            serverCurrentlyActivatedPoses.Clear();

            trackingFrame.playerUserPoses.ForEach(pup =>
            {
                localCurrentlyActivatedPoses.Add(poseManagerService.GetPose(trackingFrame.userId, pup));
            });

            trackingFrame.playerServerPoses.ForEach(psp =>
            {
                serverCurrentlyActivatedPoses.Add(poseManagerService.GetPose(0, psp));
            });
        }

        /// <summary>
        /// Add the poses to the tracking frame
        /// </summary>
        /// <param name="trackingFrame"></param>
        /// <param name="option"></param>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (trackingFrame.playerUserPoses == null) trackingFrame.playerUserPoses = new();
            if (trackingFrame.playerServerPoses == null) trackingFrame.playerServerPoses = new();

            localCurrentlyActivatedPoses.ForEach((pose) =>
            {
                trackingFrame.playerUserPoses.Add(pose.id);
            });

            serverCurrentlyActivatedPoses.ForEach((pose) =>
            {
                trackingFrame.playerServerPoses.Add(pose.id);
            });
        }

        public UserCameraPropertiesDto GetCameraDto()
        {
            return null;
        }
    }
}