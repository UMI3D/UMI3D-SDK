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
using UnityEngine;

namespace umi3d.common
{
    [System.Serializable]
    public class ManipulationParametersDto:UMI3DDto
    {
        public SerializableVector3 Translation = Vector3.zero;
        public SerializableQuaternion Rotation = Quaternion.identity;

        public ManipulationParametersDto() : base() { }
    }
}