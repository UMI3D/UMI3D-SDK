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

namespace umi3d.edk.interaction
{
    /// <summary>
    /// Abstract UMI3D interaction.
    /// </summary>
    [System.Serializable]
    public class InteractionDisplay
    {
        /// <summary>
        /// The interaction's name. 
        /// </summary>
        public string name = "new interaction";

        /// <summary>
        /// The interaction's description. 
        /// </summary>
        public string description = "";

        /// <summary>
        /// The interaction's icon 2D. 
        /// </summary>
        public UMI3DResource icon2D = new UMI3DResource();

        /// <summary>
        /// The interaction's icon 3D. 
        /// </summary>
        public UMI3DResource icon3D = new UMI3DResource();
    }
}
