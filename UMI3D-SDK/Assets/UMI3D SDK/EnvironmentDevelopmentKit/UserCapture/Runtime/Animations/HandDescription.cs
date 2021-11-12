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
    [System.Serializable]
    public struct SpatialDataInfo
    {
        public Vector3 Pos;
        public Vector3 Rot;

        public SpatialDataInfo(Vector3 pos, Vector3 rot)
        {
            Pos = pos;
            Rot = rot;
        }
    }

    public class HandDescription : ScriptableObject
    {
        public Vector3 RightHandPosition = Vector3.zero;
        public Vector3 RightHandEulerRotation = Vector3.zero;

        public Vector3 LeftHandPosition = Vector3.zero;
        public Vector3 LeftHandEulerRotation = Vector3.zero;

        [System.Serializable]
        public class PhalanxData
        {
            public string PhalanxName;
            public SpatialDataInfo Data;
        }

        public List<PhalanxData> PhalangesData = new List<PhalanxData>();

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

        public void Set(string phalanx, SpatialDataInfo data)
        {
            int index;
            if ((index = PhalangesData.FindIndex(d => d.PhalanxName.Equals(phalanx))) != -1)
            {
                PhalangesData[index].Data = data;
            }
        }

        public void SetRotation(string phalanx, Vector3 rotation)
        {
            int index;
            if ((index = PhalangesData.FindIndex(d => d.PhalanxName.Equals(phalanx))) != -1)
            {
                PhalangesData[index].Data.Rot = rotation;
            }
        }

        public SpatialDataInfo Get(string phalanx)
        {
            return PhalangesData.Find(d => d.PhalanxName.Equals(phalanx)).Data;
        }
    }
}