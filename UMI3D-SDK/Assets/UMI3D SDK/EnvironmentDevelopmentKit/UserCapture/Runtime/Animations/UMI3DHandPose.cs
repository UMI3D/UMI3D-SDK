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
    /// <summary>
    /// UMI3D hand pose.
    /// </summary>
    /// A hand pose description is composed of the position and rotation of hand 
    /// and the position and rotation of every phalanx of the hand. <br/>
    /// See <see cref="HandDescription"/> and <see cref="HandAnimation"/>.
    [CreateAssetMenu(fileName = "UMI3DHandPose", menuName = "UMI3D/UMI3D Hand Pose")]
    public class UMI3DHandPose : ScriptableObject, UMI3DLoadableEntity
    {
        /// <summary>
        /// To make a clone of the current handpose thats registered to the ressouce manager
        /// </summary>
        /// <returns></returns>
        public UMI3DHandPose Clone()
        {
            UMI3DHandPose hp = CreateInstance<UMI3DHandPose>();
            hp.IsActive = this.IsActive;
            hp.isRelativeToNode = this.isRelativeToNode;
            hp.PoseName = this.PoseName;
            hp.RightHandPosition = this.RightHandPosition;
            hp.LeftHandPosition = this.LeftHandPosition;
            hp.RightHandEulerRotation = this.RightHandEulerRotation;
            hp.LeftHandEulerRotation = this.LeftHandEulerRotation;
            hp.PhalanxRotations.AddRange(this.PhalanxRotations);
            hp.registered = false;
            hp.PoseId = hp.Id();
            //UMI3DEmbodimentManager.Instance.AddAnHandPoseRef(hp);
            return hp;
        }

        /// <summary>
        /// To Diregister the HandposeFrom The embodiment manager at run time
        /// </summary>
        public void UnRegistered()
        {
            //UMI3DEmbodimentManager.Instance.RemoveHandPose(this);
        }

        /// <summary>
        /// Entity UMI3D id.
        /// </summary>
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
            public int phalanxId;
            public string Phalanx;
            public Vector3 PhalanxEulerRotation;

            public PhalanxRotation(int id, string boneType, Vector3 rotation)
            {
                phalanxId = id;
                Phalanx = boneType;
                PhalanxEulerRotation = rotation;
            }
        }

        public List<PhalanxRotation> PhalanxRotations = new List<PhalanxRotation>();

        /// <inheritdoc/>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <inheritdoc/>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        private bool registered = false;

        protected ulong GetId()
        {
            if (!registered)
            {
                var poseDto = new UMI3DHandPoseDto() { };
                RegisterPose(poseDto);
            }
            return PoseId;
        }

        protected void RegisterPose(AbstractEntityDto pose)
        {
            if (pose.id != 0 || UMI3DEnvironment.GetEntityInstance<UMI3DHandPose>(pose.id) == null)
            {
                pose.id = UMI3DEnvironment.Register(this);
                SetId(pose.id);
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(PoseId)
                + UMI3DSerializer.Write(PoseName)
                + UMI3DSerializer.Write(IsActive)
                + UMI3DSerializer.Write(HoverAnimation)
                + UMI3DSerializer.Write(isRelativeToNode)
                + UMI3DSerializer.Write(RightHandPosition)
                + UMI3DSerializer.Write(RightHandEulerRotation)
                + UMI3DSerializer.Write(LeftHandPosition)
                + UMI3DSerializer.Write(LeftHandEulerRotation)
                + UMI3DSerializer.Write(PhalanxRotations.ToDictionary(x => x.phalanxId, x => (Vector3Dto)x.PhalanxEulerRotation));
        }

        /// <inheritdoc/>
        public UMI3DHandPoseDto ToDto()
        {
            return new UMI3DHandPoseDto()
            {
                id = PoseId,
                Name = PoseName,
                IsActive = IsActive,
                HoverPose = HoverAnimation,
                isRelativeToNode = isRelativeToNode,
                RightHandPosition = RightHandPosition,
                RightHandEulerRotation = RightHandEulerRotation,
                LeftHandPosition = LeftHandPosition,
                LeftHandEulerRotation = LeftHandEulerRotation,
                PhalanxRotations = PhalanxRotations.ToDictionary(x => (uint)x.phalanxId, x => (Vector3Dto)x.PhalanxEulerRotation)
            };
        }

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

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
