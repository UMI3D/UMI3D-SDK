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

namespace umi3d.cdk.interaction.selection.intentdetector.method
{
    /// <summary>
    /// Serialized parameters for IntenSelect detectors
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "IntenSelectParameters", menuName = "UMI3D/Selection/Detector parameters/IntenSelect")]
    public class IntenSelectParameters : AbstractMethodParameters
    {
        /// <summary>
        /// Corrective term according to de Haan et al. 2005
        /// </summary>
        [SerializeField]
        public float corrective_k = 4 / 5;

        /// <summary>
        /// Cone angle in degrees, correspond to the half of the full angle at its apex
        /// </summary>
        [SerializeField]
        public float coneAngle = 15;

        /// <summary>
        /// Rate of decay of the score at each step
        /// </summary>
        [SerializeField]
        public float stickinessRate = 0.5f;

        /// <summary>
        /// Rate of increase of the score at each step
        /// </summary>
        [SerializeField]
        public float snappinessRate = 0.5f;

        /// <summary>
        /// Maximum score before provoking a reset of the detector
        /// </summary>
        [Header("Score boundaries")]
        [SerializeField]
        public float scoreMax = 70;

        /// <summary>
        /// Minimum score for an object to remain considered
        /// </summary>
        [SerializeField]
        public float scoreMin = -10;
    }
}
