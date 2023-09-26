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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture.pose;
using umi3d.edk.core;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// A class responsible for packaging a collection of pose overriders per node it refers to
    /// </summary>
    public class UMI3DPoseOverriderContainer : AbstractLoadableEntity
    {
        /// <summary>
        /// Scriptable objects to load
        /// </summary>
        private List<IUMI3DPoseOverriderData> poseOverriders = new();

        /// <summary>
        /// Async list of all the pose overrider
        /// </summary>
        public UMI3DAsyncListProperty<PoseOverriderDto> PoseOverriders;

        public UMI3DPoseOverriderContainer(ulong nodeId, IEnumerable<IUMI3DPoseOverriderData> poseOverriders, bool isStart = false)
            : base()
        {
            this.isStart = isStart;
            this.poseOverriders = poseOverriders?.ToList() ?? new();
            this.NodeId = nodeId;

            PoseOverriders = new UMI3DAsyncListProperty<PoseOverriderDto>(nodeId, UMI3DPropertyKeys.ActivePoseOverrider, new());
        }

        /// <summary>
        /// Should the pose be applied?
        /// </summary
        public bool IsStarted => isStart;

        private bool isStart;

        public ulong NodeId { get; private set; }

        public void Init(ulong nodeId = 0)
        {
            UpdateOverridersDtos(nodeId);
        }

        private void UpdateOverridersDtos(ulong nodeId)
        {
            NodeId = nodeId;
            PoseOverriders.SetValue(new());
            poseOverriders.ForEach(po =>
            {
                PoseOverriderDto poDto = po.ToDto(po.Pose.Index);
                poDto.poseConditions.ForEach(pc =>
                {
                    switch (pc)
                    {
                        case MagnitudeConditionDto magnitudeConditionDto:
                            magnitudeConditionDto.TargetNodeId = (uint)NodeId;
                            break;
                    }
                });

                PoseOverriders.Add(poDto);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public override IEntity ToEntityDto(UMI3DUser user = null)
        {
            return new UMI3DPoseOverridersContainerDto()
            {
                id = Id(),
                poseOverriderDtos = PoseOverriders.GetValue(user).ToArray(),
                relatedNodeId = NodeId
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(Id())
                + UMI3DSerializer.Write(NodeId)
                + UMI3DSerializer.WriteCollection(PoseOverriders.GetValue(user));
        }
    }
}