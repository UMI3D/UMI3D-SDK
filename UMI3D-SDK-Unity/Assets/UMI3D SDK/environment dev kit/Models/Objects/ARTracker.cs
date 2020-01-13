/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    [RequireComponent(typeof(GenericObject3D))]
    public class ARTracker : MonoBehaviour
    {
        [SerializeField]
        string _TrackerID;
        [SerializeField]
        Vector3 _PositionOffset;
        [SerializeField]
        Vector3 _RotationOffset;
        [SerializeField]
        Vector3 _ScaleOffset = Vector3.one;
        public UMI3DAsyncProperty<string> objectTrackerID;
        public UMI3DAsyncProperty<Vector3> objectPositionOffset;
        public UMI3DAsyncProperty<Vector3> objectRotationOffset;
        public UMI3DAsyncProperty<Vector3> objectScaleOffset;
        protected bool inited = false;

        public string TrackerID
        {
            get => _TrackerID;
            set
            {
                _TrackerID = value;
                SyncProperties();
            }
        }
        public Vector3 PositionOffset
        {
            get => _PositionOffset;
            set
            {
                _PositionOffset = value;
                SyncProperties();
            }
        }
        public Vector3 RotationOffset
        {
            get => _RotationOffset;
            set
            {
                _RotationOffset = value;
                SyncProperties();
            }
        }
        public Vector3 ScaleOffset
        {
            get => _ScaleOffset;
            set
            {
                _ScaleOffset = value;
                SyncProperties();
            }
        }

        public void initDefinition()
        {
            objectTrackerID = new UMI3DAsyncProperty<string>(GetComponent<GenericObject3D>().PropertiesHandler, TrackerID);
            objectTrackerID.OnValueChanged += (string value) => TrackerID = value;

            objectPositionOffset = new UMI3DAsyncProperty<Vector3>(GetComponent<GenericObject3D>().PropertiesHandler, PositionOffset);
            objectPositionOffset.OnValueChanged += (Vector3 value) => PositionOffset = value;
            objectScaleOffset = new UMI3DAsyncProperty<Vector3>(GetComponent<GenericObject3D>().PropertiesHandler, ScaleOffset);
            objectScaleOffset.OnValueChanged += (Vector3 value) => ScaleOffset = value;
            objectRotationOffset = new UMI3DAsyncProperty<Vector3>(GetComponent<GenericObject3D>().PropertiesHandler, RotationOffset);
            objectRotationOffset.OnValueChanged += (Vector3 value) => RotationOffset = value;

            inited = true;
        }

        public ARTrackerDto ToDto(UMI3DUser user)
        {
            if (!inited) initDefinition();

            var dto = new ARTrackerDto();
            dto.TrackerId = objectTrackerID.GetValue(user);
            dto.PositionOffset = objectPositionOffset.GetValue(user);
            dto.RotationOffset = Quaternion.Euler(objectRotationOffset.GetValue(user));
            dto.ScaleOffset = objectScaleOffset.GetValue(user);
            return dto;
        }

        protected void SyncProperties()
        {
            if (!inited) return;
            objectTrackerID.SetValue(TrackerID);
            objectPositionOffset.SetValue(PositionOffset);
            objectRotationOffset.SetValue(RotationOffset);
            objectScaleOffset.SetValue(ScaleOffset);
        }

        private void OnValidate()
        {
            if (inited)
                SyncProperties();
        }

    }
}