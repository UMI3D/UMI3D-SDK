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

namespace umi3d.cdk.userCapture.tracking
{
    public interface ISimulatedTracker
    {
        uint BoneType { get; }

        Vector3 PositionOffset { get; }

        Quaternion RotationOffset { get; }

        IController Controller { get; }
        GameObject GameObject { get; }

        event Action<ISimulatedTracker> Destroyed;

        (Vector3 position, Quaternion rotation) SimulatePosition();
    }
}
