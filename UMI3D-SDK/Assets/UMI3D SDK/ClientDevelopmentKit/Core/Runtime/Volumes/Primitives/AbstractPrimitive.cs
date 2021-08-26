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


using UnityEngine;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Base class for volume primitives.
    /// </summary>
    public abstract class AbstractPrimitive : AbstractVolumeCell
    {
        public ulong? id = null;
        public override ulong Id()
        {
            if (id == null)
                id = (ulong) Random.Range(long.MinValue, long.MaxValue);
            return id.Value;
        }

        public abstract void Delete();
    }
}