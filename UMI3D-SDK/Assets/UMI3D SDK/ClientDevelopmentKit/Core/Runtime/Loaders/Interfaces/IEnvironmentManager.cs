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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public interface IEnvironmentManager
    {
        GameObject gameObject { get; }
        bool loaded { get; }

        Transform transform { get; }

        T GetEntityObject<T>(ulong id) where T : class;

        UMI3DEntityInstance GetEntityInstance(ulong id);

        UMI3DNodeInstance GetNodeInstance(ulong id);

        UMI3DEntityInstance RegisterEntity(ulong id, UMI3DDto dto, object objectInstance, Action delete = null);

        UMI3DEntityInstance TryGetEntityInstance(ulong id);
    }
}