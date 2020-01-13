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
using System.Collections.Generic;

namespace umi3d.common
{
    /// <summary>
    /// Class to describe a Manipulation using degrees of freedom
    /// </summary>
    [System.Serializable]
    public class ManipulationDto : AbstractInteractionDto
    {
        /// <summary>
        /// Id of the gameobject describing the space in which the manipulation is applied.
        /// </summary>
        public string frameOfReference;

        /// <summary>
        /// List of different description of the manipulation.
        /// </summary>
        public List<DofGroupOptionDto> dofSeparationOptions = new List<DofGroupOptionDto>();

        public ManipulationDto() : base() { }
    }

    /// <summary>
    /// degrees of freedom combination
    /// </summary>
    public enum DofGroupEnum
    {
        ALL,
        X, Y, Z, XY, XZ, YZ, XYZ,
        RX, RY, RZ, RX_RY, RX_RZ, RY_RZ, RX_RY_RZ,
        X_RX, Y_RY, Z_RZ
    }

    [System.Serializable]
    public class DofGroupDto : UMI3DDto
    {
        /// <summary>
        /// name of the degree of freedom group
        /// </summary>
        public string name;
        /// <summary>
        /// degree of freedom combination used by this group
        /// </summary>
        public DofGroupEnum dofs;

        public DofGroupDto() : base()
        {
        }
    }

    [System.Serializable]
    public class DofGroupOptionDto : UMI3DDto
    {
        /// <summary>
        /// Name of the degree of freedom group option
        /// </summary>
        public string name;
        /// <summary>
        /// List of all separations
        /// </summary>
        public List<DofGroupDto> separations = new List<DofGroupDto>();

        public DofGroupOptionDto() : base()
        {
        }
    }
}