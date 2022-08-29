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
    /// Essential spatial info with position and rotation.
    /// </summary>
    [System.Serializable]
    public struct SpatialDataInfo
    {
        /// <summary>
        /// Position in Cartesian coordinate system.
        /// </summary>
        public Vector3 Pos;
        /// <summary>
        /// Rotation in Euler's coordinates.
        /// </summary>
        public Vector3 Rot;

        public SpatialDataInfo(Vector3 pos, Vector3 rot)
        {
            Pos = pos;
            Rot = rot;
        }
    }

    /// <summary>
    /// Description of an hand pose.
    /// </summary>
    /// A hand pose description is composed of the position and rotation of hand 
    /// and the position and rotation of every phalanx of the hand. 
    public class HandDescription : ScriptableObject
    {
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
        /// Rotation of the right hand reference point.
        /// </summary>
        [Tooltip("Rotation of the left hand reference point.")]
        public Vector3 LeftHandEulerRotation = Vector3.zero;

        /// <summary>
        /// Position and rotation for every phalanx.
        /// </summary>
        [System.Serializable]
        public class PhalanxData
        {
            /// <summary>
            /// Name of the phalanx.
            /// </summary>
            /// This name could be get with nameof(BoneType.type)
            [Tooltip("Use the standard phalanx name.")]
            public string PhalanxName;
            /// <summary>
            /// Position and rotation.
            /// </summary>
            [Tooltip("Spatial info on the phalanx.")]
            public SpatialDataInfo Data;
        }

        /// <summary>
        /// List of phalanx data describing the position and rotation of fingers.
        /// </summary>
        /// Behaves like a dictionnary.
        [Tooltip("List of phalanx position/rotation describing the position of fingers.")]
        public List<PhalanxData> PhalangesData = new List<PhalanxData>();

        /// <summary>
        /// Add a position/rotation couple for a phalanx to the list of phalanx position/rotation, if not already included.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <param name="data"></param>
        public void Add(string phalanx, SpatialDataInfo data)
        {
            if (PhalangesData.Find(d => d.PhalanxName.Equals(phalanx)) == null)
            {
                PhalangesData.Add(new PhalanxData()
                {
                    PhalanxName = phalanx,
                    Data = data
                });
            }
        }

        /// <summary>
        /// Set a position/rotation couple for a phalanx to the list of phalanx position/rotation, if already included.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <param name="data"></param>
        public void Set(string phalanx, SpatialDataInfo data)
        {
            int index;
            if ((index = PhalangesData.FindIndex(d => d.PhalanxName.Equals(phalanx))) != -1)
            {
                PhalangesData[index].Data = data;
            }
        }

        /// <summary>
        /// Set a rotation for a phalanx to the list of phalanx position/rotation, if already included.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <param name="rotation"></param>
        public void SetRotation(string phalanx, Vector3 rotation)
        {
            int index;
            if ((index = PhalangesData.FindIndex(d => d.PhalanxName.Equals(phalanx))) != -1)
            {
                PhalangesData[index].Data.Rot = rotation;
            }
        }

        /// <summary>
        /// Get a position/rotation info of a phalanx.
        /// </summary>
        /// <param name="phalanx"></param>
        /// <returns></returns>
        public SpatialDataInfo Get(string phalanx)
        {
            return PhalangesData.Find(d => d.PhalanxName.Equals(phalanx)).Data;
        }
    }
}