/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// <inheritdoc/>
    /// A condition to check the direction
    /// </summary>
    [System.Serializable]
    public class DirectionConditionDto : PoseConditionDto
    {
        public DirectionConditionDto() { }

        public DirectionConditionDto(Vector3Dto direction)
        {
            Direction = direction;
        }

        private Vector3Dto direction;
        public Vector3Dto Direction { get => direction; set => direction = value; }
    }
}
