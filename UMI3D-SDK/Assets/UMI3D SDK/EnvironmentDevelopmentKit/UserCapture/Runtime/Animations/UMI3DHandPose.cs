/*
Copyright 2019 - 2021 Inetum

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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    [CreateAssetMenu(fileName = "UMI3DHandPose", menuName = "UMI3D/UMI3D Hand Pose")]
    public class UMI3DHandPose : ScriptableObject, UMI3DLoadableEntity
    {
        [HideInInspector]
        public ulong PoseId;

        [HideInInspector]
        public bool IsActive = false;

        [HideInInspector]
        public bool HoverAnimation = false;

        [HideInInspector]
        public bool isRelativeToNode = false;

        public string PoseName;

        public Vector3 RightHandPosition = Vector3.zero;
        public Vector3 RightHandEulerRotation = Vector3.zero;

        public Vector3 LeftHandPosition = Vector3.zero;
        public Vector3 LeftHandEulerRotation = Vector3.zero;

        [Serializable]
        public class PhalanxRotation
        {
            [HideInInspector]
            public uint phalanxId;
            public string Phalanx;
            public Vector3 PhalanxEulerRotation;

            public PhalanxRotation(uint id, string boneType, Vector3 rotation)
            {
                phalanxId = id;
                Phalanx = boneType;
                PhalanxEulerRotation = rotation;
            }
        }

        public List<PhalanxRotation> PhalanxRotations = new List<PhalanxRotation>();

        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
            };
            return operation;
        }

        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        private bool registered = false;

        protected ulong GetId()
        {
            if (!registered)
            {
                UMI3DHandPoseDto poseDto = new UMI3DHandPoseDto() { };
                RegisterPose(poseDto);
            }
            return PoseId;
        }

        protected void RegisterPose(AbstractEntityDto pose)
        {
            if (pose.id != 0 || UMI3DEnvironment.GetEntity<UMI3DHandPose>(pose.id) == null)
            {
                pose.id = UMI3DEnvironment.Register(this);
                SetId(pose.id);
            }
        }

        public ulong Id()
        {
            return GetId();
        }

        protected void OnEnable()
        {
            PoseId = 0;
            registered = false;
        }


        protected void SetId(ulong id)
        {
            registered = true;
            PoseId = id;
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(PoseId)
                + UMI3DNetworkingHelper.Write(PoseName)
                + UMI3DNetworkingHelper.Write(IsActive)
                + UMI3DNetworkingHelper.Write(HoverAnimation)
                + UMI3DNetworkingHelper.Write(isRelativeToNode)
                + UMI3DNetworkingHelper.Write(RightHandPosition)
                + UMI3DNetworkingHelper.Write(RightHandEulerRotation)
                + UMI3DNetworkingHelper.Write(LeftHandPosition)
                + UMI3DNetworkingHelper.Write(LeftHandEulerRotation)
                + UMI3DNetworkingHelper.Write(PhalanxRotations.ToDictionary(x => x.phalanxId, x => (SerializableVector3)x.PhalanxEulerRotation));
        }

        public UMI3DHandPoseDto ToDto()
        {
            return new UMI3DHandPoseDto()
            {
                id = PoseId,
                Name = PoseName,
                IsActive = IsActive,
                //IsRight = IsRight,
                HoverPose = HoverAnimation,
                isRelativeToNode = isRelativeToNode,
                RightHandPosition = RightHandPosition,
                RightHandEulerRotation = RightHandEulerRotation,
                LeftHandPosition = LeftHandPosition,
                LeftHandEulerRotation = LeftHandEulerRotation,
                PhalanxRotations = PhalanxRotations.ToDictionary(x => x.phalanxId, x => (SerializableVector3)x.PhalanxEulerRotation)
            };
        }

        #region filter
        HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion
    }
}
