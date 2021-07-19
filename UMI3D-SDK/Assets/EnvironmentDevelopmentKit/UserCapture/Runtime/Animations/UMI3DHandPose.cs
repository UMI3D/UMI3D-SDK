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
        public string PoseId;

        [HideInInspector]
        public bool IsActive = false;

        [HideInInspector]
        public bool isRelativeToNode = true;

        public string Name;

        public bool IsRight = true;

        public string RelativeNodeId;

        public Vector3 HandPosition = Vector3.zero;
        public Vector3 HandEulerRotation = Vector3.zero;

        [Serializable]
        public class PhalanxRotation
        {
            public string Phalanx;
            public Vector3 PhalanxEulerRotation;

            public PhalanxRotation(string BoneType, Vector3 rotation)
            {
                Phalanx = BoneType;
                PhalanxEulerRotation = rotation;
            }
        }

        public List<PhalanxRotation> PhalanxRotations = new List<PhalanxRotation>();

        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entity = this,
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

        protected string GetId()
        {
            if (!registered)
            {
                UMI3DHandPoseDto poseDto = new UMI3DHandPoseDto(){ };
                RegisterPose(poseDto);
            }
            return PoseId;
        }

        protected void RegisterPose(AbstractEntityDto pose)
        {
            if (string.IsNullOrEmpty(pose.id) || UMI3DEnvironment.GetEntity<UMI3DHandPose>(pose.id) == null)
            {
                pose.id = UMI3DEnvironment.Register(this);
                SetId(pose.id);
                //InitDefinition(pose.id);
            }
        }

        public string Id()
        {
            return GetId();
        }

        protected void OnEnable()
        {
            PoseId = null;
            registered = false;
        }


        protected void SetId(string id)
        {
            registered = true;
            PoseId = id;
        }

        //protected void InitDefinition(string id)
        //{

        //}

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        public UMI3DHandPoseDto ToDto()
        {
            return new UMI3DHandPoseDto()
            {
                IsRight = IsRight,
                objectId = RelativeNodeId,
                HandPosition = HandPosition,
                HandEulerRotation = HandEulerRotation,
                PhalanxRotations = PhalanxRotations.ToDictionary(x => x.Phalanx, x => (SerializableVector3)x.PhalanxEulerRotation)
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
