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

using System.Collections.Generic;

namespace umi3d.common.userCapture
{
    public class UMI3DBodyPoseDto : AbstractEntityDto, IEntity
    {
        public string Name;

        public bool IsActive;
        public bool IsRelativeToNode;
        public bool AllowOverriding;

        public SerializableVector3 BodyPosition;
        public SerializableVector3 BodyEulerRotation;

        //public Dictionary<uint, SerializableVector3> JointRotations = new Dictionary<uint, SerializableVector3>();

        public Dictionary<uint, KeyValuePair<SerializableVector3, SerializableVector3>> TargetTransforms = new Dictionary<uint, KeyValuePair<SerializableVector3, SerializableVector3>>();
    }
}