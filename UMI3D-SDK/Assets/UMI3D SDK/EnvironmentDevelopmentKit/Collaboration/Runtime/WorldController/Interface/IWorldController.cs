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

using System.Threading.Tasks;
using umi3d.common;

namespace umi3d.worldController
{
    /// <summary>
    /// Behaviour of a 3D media that manages a set of environments within a same world.
    /// </summary>
    public interface IWorldController
    {
        /// <summary>
        /// Retrieve the world.
        /// </summary>
        /// <returns></returns>
        Task<MediaDto> GetMediaDto();
    }
}
