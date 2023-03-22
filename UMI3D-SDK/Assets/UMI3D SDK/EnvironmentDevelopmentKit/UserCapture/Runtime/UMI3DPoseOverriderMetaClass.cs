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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    [Serializable]
    public class UMI3DPoseOverriderMetaClass : UMI3DLoadableEntity
    {
        [SerializeField] List<UMI3DPoseOveridder_so> poseOverriders = new List<UMI3DPoseOveridder_so>();

        List<PoseOverriderDto> poseOverridersDtos = new List<PoseOverriderDto>();
        UMI3DAsyncListProperty<PoseOverriderDto> poseOverriderDtoAsyncList;

        ulong overriderID;

        public void Init()
        {
            poseOverridersDtos.Clear();
            poseOverriders.ForEach(po =>
            {
                po.pose.onPoseReferencedAndIndexSetted += (indexInPoseManager) =>
                {
                    poseOverridersDtos.Add(po.ToDto(indexInPoseManager));
                };
            });
        }

        public void AddPoseOveriderDtos(List<PoseOverriderDto> poseOverriderDtos)
        {
            this.poseOverridersDtos.AddRange(poseOverriderDtos);
        }

        public LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            throw new NotImplementedException();
        }

        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            throw new NotImplementedException();
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        private UMI3DOverriderMetaClassDto ToDto()
        {
            UMI3DOverriderMetaClassDto uMI3DOverriderMetaClassDto = new UMI3DOverriderMetaClassDto()
            {
                poseOverriderDtos = poseOverridersDtos.ToArray(),
            };

            return uMI3DOverriderMetaClassDto;
        }

        public Bytable ToBytes(UMI3DUser user)
        {
            throw new NotImplementedException();
        }

        public bool LoadOnConnection(UMI3DUser user)
        {
            throw new NotImplementedException();
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            throw new NotImplementedException();
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            throw new NotImplementedException();
        }

        public ulong Id()
        {
            Register();
            return overriderID;
        }

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
    }
}

