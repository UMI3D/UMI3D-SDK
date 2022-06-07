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
    public class BodyDescription : ScriptableObject
    {
        public Vector3 BodyPosition = Vector3.zero;
        public Vector3 BodyEulerRotation = Vector3.zero;

        public Vector3 RightHandPosition = Vector3.zero;
        public Vector3 RightHandEulerRotation = Vector3.zero;

        public Vector3 LeftHandPosition = Vector3.zero;
        public Vector3 LeftHandEulerRotation = Vector3.zero;

        public Vector3 RightAnklePosition = Vector3.zero;
        public Vector3 RightAnkleEulerRotation = Vector3.zero;

        public Vector3 LeftAnklePosition = Vector3.zero;
        public Vector3 LeftAnkleEulerRotation = Vector3.zero;

        [System.Serializable]
        public class JointData
        {
            public string JointName;
            public SpatialDataInfo Data;
        }

        public List<JointData> JointsData = new List<JointData>();

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

        public void Set(string phalanx, SpatialDataInfo data)
        {
            int index;
            if ((index = JointsData.FindIndex(d => d.JointName.Equals(phalanx))) != -1)
            {
                JointsData[index].Data = data;
            }
        }

        public void SetRotation(string phalanx, Vector3 rotation)
        {
            int index;
            if ((index = JointsData.FindIndex(d => d.JointName.Equals(phalanx))) != -1)
            {
                JointsData[index].Data.Rot = rotation;
            }
        }

        public SpatialDataInfo Get(string phalanx)
        {
            return JointsData.Find(d => d.JointName.Equals(phalanx)).Data;
        }
    }
}
