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
    public class HeaderContentSerializer : UMI3DSerializerModule<HeaderContent>
    {
        public bool IsCountable()
        {
            return true;
        }

        public bool Read(ByteContainer container, out bool readable, out HeaderContent result)
        {
            readable = UMI3DSerializer.TryRead(container, out string header);
            readable &= UMI3DSerializer.TryRead(container, out string content);

            result = readable ? new HeaderContent() { header = header, content = content } : null;
            return readable;
        }

        public bool Write(HeaderContent value, out Bytable bytable, params object[] parameters)
        {
            bytable = UMI3DSerializer.Write(value.header) + UMI3DSerializer.Write(value.content);
            return true;
        }
    }
}