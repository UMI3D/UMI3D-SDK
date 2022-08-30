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

using System.Collections.Generic;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Description of an body pose.
    /// </summary>
    /// A hand pose description is composed of the position and rotation of limbs 
    /// and the position and rotation of every bone joints of the body. 
    public class BodyDescription : ScriptableObject
    {
        /// <summary>
        /// Position of the body reference point.
        /// </summary>
        [Tooltip("Position of the body reference point.")]
        public Vector3 BodyPosition = Vector3.zero;
        /// <summary>
        /// Rotation of the body reference point.
        /// </summary>
        [Tooltip("Rotation of the body reference point.")]
        public Vector3 BodyEulerRotation = Vector3.zero;

        /// <summary>
        /// Position of the right hand reference point.
        /// </summary>
        [Tooltip("Position of the right hand reference point.")]
        public Vector3 RightHandPosition = Vector3.zero;
        /// <summary>
        /// Rotation of the right hand reference point.
        /// </summary>
        [Tooltip("Rotation of the right hand reference point.")]
        public Vector3 RightHandEulerRotation = Vector3.zero;

        /// <summary>
        /// Position of the left hand reference point.
        /// </summary>
        [Tooltip("Position of the left hand reference point.")]
        public Vector3 LeftHandPosition = Vector3.zero;
        /// <summary>
        /// Rotatoin of the left hand reference point.
        /// </summary>
        [Tooltip("Rotation of the left hand reference point.")]
        public Vector3 LeftHandEulerRotation = Vector3.zero;

        /// <summary>
        /// Position of the right ankle reference point.
        /// </summary>
        [Tooltip("Position of the right ankle reference point.")]
        public Vector3 RightAnklePosition = Vector3.zero;
        /// <summary>
        /// Rotation of the right ankle reference point.
        /// </summary>
        [Tooltip("Rotation of the right ankle reference point.")]
        public Vector3 RightAnkleEulerRotation = Vector3.zero;

        /// <summary>
        /// Position of the left ankle reference point.
        /// </summary>
        [Tooltip("Position of the left ankle reference point.")]
        public Vector3 LeftAnklePosition = Vector3.zero;
        /// <summary>
        /// Rotation of the left ankle reference point.
        /// </summary>
        [Tooltip("Rotation of the left ankle reference point.")]
        public Vector3 LeftAnkleEulerRotation = Vector3.zero;

        /// <summary>
        /// Position and rotation for every body joint.
        /// </summary
        [System.Serializable]
        public class JointData
        {
            /// <summary>
            /// Joint name.
            /// </summary>
            /// This name could be get with nameof(BoneType.type)
            [Tooltip("Use the standard joint name.")]
            public string JointName;
            /// <summary>
            /// Position and rotation.
            /// </summary>
            [Tooltip("Spatial info on the joint.")]
            public SpatialDataInfo Data;
        }

        /// <summary>
        /// List of joints data describing the position and rotation of bones.
        /// </summary>
        /// Behaves like a dictionnary.
        [Tooltip("List of joints position/rotation describing the position of body's bones.")]
        public List<JointData> JointsData = new List<JointData>();

        /// <summary>
        /// Add a position/rotation couple for a joint to the list of joints position/rotation, if not already included.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <param name="data"></param>
        public void Add(string phalanx, SpatialDataInfo data)
        {
            if (JointsData.Find(d => d.JointName.Equals(phalanx)) == null)
            {
                JointsData.Add(new JointData()
                {
                    JointName = phalanx,
                    Data = data
                });
            }
        }

        /// <summary>
        /// Set a position/rotation couple for a joint to the list of joints position/rotation, if already included.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <param name="data"></param>
        public void Set(string phalanx, SpatialDataInfo data)
        {
            int index;
            if ((index = JointsData.FindIndex(d => d.JointName.Equals(phalanx))) != -1)
            {
                JointsData[index].Data = data;
            }
        }

        /// <summary>
        /// Set a rotation for a joint to the list of joints position/rotation, if already included.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <param name="rotation"></param>
        public void SetRotation(string phalanx, Vector3 rotation)
        {
            int index;
            if ((index = JointsData.FindIndex(d => d.JointName.Equals(phalanx))) != -1)
            {
                JointsData[index].Data.Rot = rotation;
            }
        }

        /// <summary>
        /// Get a position/rotation info of a joint.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <returns></returns>
        public SpatialDataInfo Get(string phalanx)
        {
            return JointsData.Find(d => d.JointName.Equals(phalanx)).Data;
        }
    }
}
