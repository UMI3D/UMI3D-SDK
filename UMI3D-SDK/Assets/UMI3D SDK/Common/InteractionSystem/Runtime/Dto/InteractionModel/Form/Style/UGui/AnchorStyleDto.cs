/*
Copyright 2019 - 2024 Inetum

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

namespace umi3d.common.interaction.form.ugui
{
    public class AnchorStyleDto : UGUIStyleItemDto
    {
        public float minX {  get; set; }
        public float minY { get; set; }
        public float maxX { get; set; }
        public float maxY { get; set; }
        public float pivotX { get; set; }
        public float pivotY { get; set; }
    }
}
