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
using UnityEngine.Events;

namespace umi3d.cdk
{

    [System.Serializable]
    public class OnARTrackerCreated : UnityEvent<ARTrackerObject>
    {

    }

    [System.Serializable]
    public class OnARTrackerDeleted : UnityEvent<ARTrackerObject>
    {

    }

    public class ARTrackerObject : MonoBehaviour
    {

        static OnARTrackerCreated _OnARTrackerCreated;
        static OnARTrackerDeleted _OnARTrackerDeleted;

        static public OnARTrackerCreated OnARTrackerCreated
        {
            get {
                if (_OnARTrackerCreated == null)
                    _OnARTrackerCreated = new OnARTrackerCreated();
                return _OnARTrackerCreated;
            }
        }

        static public OnARTrackerDeleted OnARTrackerDeleted
        {
            get {
                if (_OnARTrackerDeleted == null)
                    _OnARTrackerDeleted = new OnARTrackerDeleted();
                return _OnARTrackerDeleted;
            }
        }

        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        public Vector3 positionOffset;
        public Vector3 scaleOffset;
        public Quaternion rotationOffset;

        public Vector3 targetWorldPosition;
        public Vector3 targetWorldScale;
        public Quaternion targetWorldRotation;

        public string TrackerId;

        /// <summary>
        /// Setup an ARTracker Object using parameter from an other ARTracker Object.
        /// </summary>
        /// <param name="trackerObject"></param>
        public void Set(ARTrackerObject trackerObject)
        {
            TrackerId = trackerObject.TrackerId;
            OnARTrackerCreated.Invoke(this);
        }

        /// <summary>
        /// Update an ARTrackerObject with an ARTrackerDto.
        /// </summary>
        /// <param name="newdto"></param>
        public void UpdateTracker(ARTrackerDto newdto)
        {
            TrackerId = newdto.TrackerId;
            positionOffset = newdto.PositionOffset;
            scaleOffset = newdto.ScaleOffset;
            rotationOffset = newdto.RotationOffset;
            targetWorldPosition = newdto.TargetWorldPosition;
            targetWorldRotation = newdto.TargetWorldRotation;
            targetWorldScale = newdto.TargetWorldScale;
        }

        private void OnDestroy()
        {
            OnARTrackerDeleted.Invoke(this);
        }
    }
}