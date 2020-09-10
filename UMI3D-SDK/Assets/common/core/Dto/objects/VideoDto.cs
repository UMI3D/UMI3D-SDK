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

namespace umi3d.common
{
    [System.Serializable]
    public class VideoDto : UMI3DDto
    {
        public bool PlayOnAwake = false;
        public string AudioSource = null;
        public bool Playing = false;
        public bool Paused = false;
        public bool Stoped = false;
        public double Progress = 0;
        public bool Loop = false;

        public string StartTimeInMS = "";

        public ResourceDto VideoResource;

        public VideoDto() : base() { }
    }
}
