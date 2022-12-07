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
using UnityEngine;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Relay that allows to transmit data from one user to another.
    /// </summary>
    [CreateAssetMenu(fileName = "UMI3DRelayDescription", menuName = "UMI3D/UMI3D_Relay_Description")]
    public class RelayDescription : ScriptableObject
    {
        /// <summary>
        /// Way to handle data sending.
        /// </summary>
        public enum SendingStrategy { AlwaysSend, Fixed, Proximity };

        [Serializable]
        public struct Strategy
        {
            /// <summary>
            /// Is data transmission allowed?
            /// </summary>
            [Tooltip("Is data transmission allowed?")]
            public bool sendData;

            /// <summary>
            /// Way to handle data sending.
            /// </summary>
            [Tooltip("Way to handle data sending.\n" +
                "Always Send: Send every data whatever the sending frequency is.\n" +
                "Fixed: Send data with a constant frequency.\n" +
                "Proximity: Decrease data transmission frequency with distance from the relay.")]
            public SendingStrategy sendingStrategy;

            /// <summary>
            /// Data sending frequency on Fixed strategy.
            /// </summary>
            [Min(0), Tooltip("Data sending frequency on Fixed strategy.")]
            public float constantFPS;

            /// <summary>
            /// Minimum data sending frequency on Proximity strategy.
            /// </summary>
            [Min(0), Tooltip("Minimum data sending frequency on Proximity strategy.")]
            public float minProximityFPS;

            /// <summary>
            /// Maximum data sending frequency on Proximity strategy.
            /// </summary>
            [Min(0), Tooltip("Maximum data sending frequency on Proximity strategy.")]
            public float maxProximityFPS;

            /// <summary>
            /// Distance at which the proximty strategy starts to be applied.
            /// </summary>
            [Min(0), Tooltip("Distance at which the proximty strategy starts to be applied.")]
            public float startingProximityDistance;

            /// <summary>
            /// Distance at which the proximty strategy stops to be applied.
            /// </summary>
            [Min(0), Tooltip("Distance at which the proximty strategy stops to be applied.")]
            public float stoppingProximityDistance;
        }

        /// <summary>
        /// Strategy within the relay volume
        /// </summary>
        public Strategy InsideVolume;

        /// <summary>
        /// Strategy outside of the relay volume
        /// </summary>
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