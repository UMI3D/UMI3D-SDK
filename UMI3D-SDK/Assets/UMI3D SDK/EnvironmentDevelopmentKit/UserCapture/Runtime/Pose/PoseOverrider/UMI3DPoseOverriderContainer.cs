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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// A class responsible for packaging a collection of pose overriders per node it refers to
    /// </summary>
    [Serializable]
    public class UMI3DPoseOverriderContainer : UMI3DLoadableEntity
    {
        /// <summary>
        /// Scriptable objects to load
        /// </summary>
        [SerializeField] private List<UMI3DPoseOveridder_so> poseOverriders = new List<UMI3DPoseOveridder_so>();

        [SerializeField] private bool isStart;
        public bool IsStart => isStart;

        public ulong nodeID { get; set; }

        public void SetNodeId(ulong nodeId)
        {
            this.nodeID = nodeId;

            poseOverriders.ForEach(po =>
            {
                PoseOverriderDto poDto = po.ToDto(po.pose.poseRef);
                poDto.poseConditions.ForEach(pc =>
                {
                    switch (pc)
                    {
                        case MagnitudeConditionDto magnitudeConditionDto:
                            magnitudeConditionDto.TargetNodeId = (uint)nodeID;
                            break;
                    }
                });
                poseOverridersDtos.Add(poDto);
            });
        }

        public UMI3DPoseOverriderContainer(List<UMI3DPoseOveridder_so> poseOveridder_Sos)
        {
            this.poseOverriders = poseOveridder_Sos;
        }

        /// <summary>
        /// list of all the pose dto of this meta class
        /// </summary>
        private List<PoseOverriderDto> poseOverridersDtos = new List<PoseOverriderDto>();

        /// <summary>
        /// Async lis of all the pose overider
        /// </summary>
        private UMI3DAsyncListProperty<PoseOverriderDto> poseOverriderDtoAsyncList;

        /// <summary>
        /// ID of this entty
        /// </summary>
        private ulong overriderID;

        /// <summary>
        /// Check if the UMI3DPoseOverriderMetaClass has been registered to the Environnement and do it if not
        /// </summary>
        /// <returns>Return a LoadEntity</returns>
        public virtual LoadEntity Register()
        {
            if (overriderID == 0 && UMI3DEnvironment.Exists)
            {
                overriderID = UMI3DEnvironment.Register(this);
                InitDefinition(overriderID);
            }
            return GetLoadEntity();
        }

        /// <summary>
        /// Load the scriptable objects as dtos and inits the assync list
        /// </summary>
        /// <param name="id"></param>
        public void InitDefinition(ulong id)
        {
            if (poseOverridersDtos == null) poseOverridersDtos = new List<PoseOverriderDto>();
            poseOverridersDtos.Clear();

            poseOverriderDtoAsyncList = new UMI3DAsyncListProperty<PoseOverriderDto>(id, UMI3DPropertyKeys.ActivePoseOverrider, poseOverridersDtos);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user);
        }

        /// <summary>
        /// Converts this to a DTO entity
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private UMI3DPoseOverriderContainerDto ToDto(UMI3DUser user)
        {
            UMI3DPoseOverriderContainerDto uMI3DOverriderMetaClassDto = new UMI3DPoseOverriderContainerDto()
            {
                poseOverriderDtos = poseOverriderDtoAsyncList.GetValue(user).ToArray(),
                relatedNodeId = nodeID
            };

            return uMI3DOverriderMetaClassDto;
        }

        public UMI3DPoseOverriderContainerDto ToDto()
        {
            return new UMI3DPoseOverriderContainerDto()
            {
                id = Id(),
                poseOverriderDtos = poseOverridersDtos.ToArray(),
                relatedNodeId = nodeID
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(Id())
                + UMI3DSerializer.WriteCollection(poseOverriderDtoAsyncList.GetValue(user));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ulong Id()
        {
            Register();
            return overriderID;
        }

        #region filter

        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        /// <inheritdoc/>
        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        /// <inheritdoc/>
        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        /// <inheritdoc/>
        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }

        #endregion filter
    }
}