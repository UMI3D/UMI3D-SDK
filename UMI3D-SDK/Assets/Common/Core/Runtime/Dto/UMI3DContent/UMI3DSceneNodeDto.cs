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

namespace umi3d.common
{
    [System.Serializable]
    public class UMI3DSceneNodeDto : UMI3DAbstractNodeDto
    {
        public SerializableVector3 position;
        public SerializableVector4 rotation;
        public SerializableVector3 scale;
        public List<UMI3DAbstractAnimationDto> animations = new List<UMI3DAbstractAnimationDto>();
        public List<IEntity> otherEntities;
        public List<string> LibrariesId;
    }
}
