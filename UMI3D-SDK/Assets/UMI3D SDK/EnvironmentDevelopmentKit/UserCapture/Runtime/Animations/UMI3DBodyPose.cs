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
    [CreateAssetMenu(fileName = "UMI3DBodyPose", menuName = "UMI3D/UMI3D Body Pose")]
    public class UMI3DBodyPose : ScriptableObject, UMI3DLoadableEntity
    {
        [HideInInspector]
        public ulong PoseId;

        [HideInInspector]
        public bool IsActive = false;

        [HideInInspector]
        public bool AllowOverriding = false;

        [HideInInspector]
        public bool isRelativeToNode = false;

        public string PoseName;

        public Vector3 BodyPosition = Vector3.zero;
        public Vector3 BodyEulerRotation = Vector3.zero;

        [Serializable]
        public class JointRotation
        {
            [HideInInspector]
            public uint jointId;
            public string Joint;
            public Vector3 JointEulerRotation;

            public JointRotation(uint id, string boneType, Vector3 rotation)
            {
                jointId = id;
                Joint = boneType;
                JointEulerRotation = rotation;
            }
        }

        [Serializable]
        public class TargetTransform
        {
            [HideInInspector]
            public uint jointId;
            public string Joint;
            public Vector3 relativePosition;
            public Vector3 relativeRotation;

            public TargetTransform(uint id, string boneType, Vector3 position, Vector3 rotation)
            {
                jointId = id;
                Joint = boneType;
                relativePosition = position;
                relativeRotation = rotation;
            }
        }

        public List<JointRotation> JointRotations = new List<JointRotation>();

        public List<TargetTransform> TargetTransforms = new List<TargetTransform>();

        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
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
                + UMI3DNetworkingHelper.Write(isRelativeToNode)
                + UMI3DNetworkingHelper.Write(AllowOverriding)
                + UMI3DNetworkingHelper.Write(BodyPosition)
                + UMI3DNetworkingHelper.Write(BodyEulerRotation)
                //+ UMI3DNetworkingHelper.Write(JointRotations.ToDictionary(x => x.jointId, x => (SerializableVector3)x.JointEulerRotation));
                + UMI3DNetworkingHelper.Write(TargetTransforms.ToDictionary(x => x.jointId, x => new KeyValuePair<Vector3, Vector3>(x.relativePosition, x.relativeRotation)));
        }

        public UMI3DBodyPoseDto ToDto()
        {
            return new UMI3DBodyPoseDto()
            {
                id = PoseId,
                Name = PoseName,
                IsActive = IsActive,
                IsRelativeToNode = isRelativeToNode,
                AllowOverriding = AllowOverriding,
                BodyPosition = BodyPosition,
                BodyEulerRotation = BodyEulerRotation,
                //JointRotations = JointRotations.ToDictionary(x => x.jointId, x => (SerializableVector3)x.JointEulerRotation)
                TargetTransforms = TargetTransforms.ToDictionary(x => x.jointId, x => new KeyValuePair<SerializableVector3, SerializableVector3>(x.relativePosition, x.relativeRotation))
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