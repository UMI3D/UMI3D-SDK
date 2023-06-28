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
    [System.Serializable]
    public class DurationDto : UMI3DDto
    {
        public DurationDto() { }

        public DurationDto(ulong duration, ulong? min, ulong? max)
        {
            this.duration = duration;
            this.min = min; 
            this.max = max;
        }

        public ulong duration { get; set; }
        public ulong? min { get; set; } 
        public ulong? max { get; set;}
    }
}

