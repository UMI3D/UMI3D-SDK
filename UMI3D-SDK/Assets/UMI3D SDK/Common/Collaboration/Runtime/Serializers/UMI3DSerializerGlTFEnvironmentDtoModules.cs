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

namespace umi3d.common
{
    [UMI3DSerializerOrder(1000)]
    public class UMI3DSerializerGlTFEnvironmentDtoModules : UMI3DSerializerModule<GlTFEnvironmentDto>
    {
        public bool IsCountable()
        {
            return true;
        }

        public bool Read(ByteContainer container, out bool readable, out GlTFEnvironmentDto result)
        {
            var b = new GlTFEnvironmentDto();
            readable = true;
            result = b;
            return true;
        }

        public bool Write(GlTFEnvironmentDto value, out Bytable bytable, params object[] parameters)
        {
            bytable = new();
            return true;
        }
    }
}