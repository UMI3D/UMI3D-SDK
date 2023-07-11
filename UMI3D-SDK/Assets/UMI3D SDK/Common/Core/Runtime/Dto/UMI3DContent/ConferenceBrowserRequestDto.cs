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

namespace umi3d.common
{
    /// <summary>
    /// Request to update conference value such as microphone state.
    /// </summary>
    public class ConferenceBrowserRequestDto : AbstractBrowserRequestDto
    {
        /// <summary>
        /// conference operation key tu update  
        /// </summary>
        public uint operation { get; set; }

        /// <summary>
        /// value to update to
        /// </summary>
        public bool value { get; set; }

        /// <summary>
        /// id of the user
        /// </summary>
        public ulong id { get; set; }
    }
}