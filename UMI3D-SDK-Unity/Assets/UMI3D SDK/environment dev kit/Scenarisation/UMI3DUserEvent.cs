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
using System;
using umi3d.common;
using UnityEngine.Events;


namespace umi3d.edk
{
    /// <summary>
    /// Event raising an UMI3DUser instance.
    /// </summary>
    [Serializable]
    public class UMI3DUserEvent : UnityEvent<UMI3DUser> { }

    /// <summary>
    /// Event rising an UMI3DUser and an avatar bone.
    /// </summary>
    [Serializable]
    public class UMI3DUserBoneEvent : UnityEvent<UMI3DUser, string> { }
}