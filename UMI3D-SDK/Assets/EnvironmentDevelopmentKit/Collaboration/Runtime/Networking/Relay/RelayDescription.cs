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

using UnityEngine;
using System;

namespace umi3d.edk.collaboration
{
    [CreateAssetMenu(fileName = "UMI3DRelayDescription", menuName = "UMI3D/UMI3D_Relay_Description")]
    public class RelayDescription : ScriptableObject
    {
        public enum SendingStrategy { AlwaysSend, Fixed, Proximity };

        [Serializable]
        public struct Strategy
        {
            public bool sendData;
            public SendingStrategy sendingStrategy;

            [Min(0)]
            public float constantFPS;

            [Min(0)]
            public float minProximityFPS;

            [Min(0)]
            public float maxProximityFPS;

            [Min(0)]
            public float startingProximityDistance;

            [Min(0)]
            public float stoppingProximityDistance;
        }

        public Strategy InsideVolume; 

        public Strategy OutsideVolume;


        private void OnValidate()
        {
            if (InsideVolume.minProximityFPS > InsideVolume.maxProximityFPS)
            {
                InsideVolume.minProximityFPS = InsideVolume.maxProximityFPS;
            }

            if (OutsideVolume.minProximityFPS > OutsideVolume.maxProximityFPS)
            {
                OutsideVolume.minProximityFPS = OutsideVolume.maxProximityFPS;
            }

            if (InsideVolume.startingProximityDistance > InsideVolume.stoppingProximityDistance)
            {
                InsideVolume.startingProximityDistance = InsideVolume.stoppingProximityDistance;
            }

            if (OutsideVolume.startingProximityDistance > OutsideVolume.stoppingProximityDistance)
            {
                OutsideVolume.startingProximityDistance = OutsideVolume.stoppingProximityDistance;
            }
        }

    }
}