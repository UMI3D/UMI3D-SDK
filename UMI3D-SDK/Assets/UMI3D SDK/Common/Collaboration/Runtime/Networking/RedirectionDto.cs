﻿/*
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

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Tell a user to disconnect from an environment and connect to an other.
    /// </summary>
    public class RedirectionDto : UMI3DDto
    {
        /// <summary>
        /// MediaDto of the media to connect to.
        /// </summary>
        public MediaDto media;

        /// <summary>
        /// Gate data to help the new media where to redirect the user.
        /// </summary>
        public GateDto gate;
    }
}