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

using System;

namespace umi3d.common
{
    public class UMI3DSerializerStringModules : UMI3DSerializerModule
    {
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(string):
                    result = default(T);
                    if (container.length == 0) return false;
                    uint s;
                    string r = "";
                    if (UMI3DSerializer.TryRead<uint>(container, out s))
                    {
                        for (uint i = 0; i < s; i++)
                        {
                            if (UMI3DSerializer.TryRead<char>(container, out char c))
                            {
                                r += c;
                            }
                            else
                            {
                                readable = false;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        readable = false;
                        return true;
                    }

                    result = (T)Convert.ChangeType(r, typeof(T));
                    return true;

            }

            result = default(T);
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable)
        {
            Func<byte[], int, int, (int, int)> f;

            if (value is string || typeof(T) == typeof(string))
            {
                if (value == null)
                {
                    bytable = UMI3DSerializer.Write((uint)0);
                }
                else
                {
                    var str = value as string;
                    bytable = UMI3DSerializer.Write((uint)str.Length);
                    foreach (char ch in str)
                    {
                        bytable += UMI3DSerializer.Write(ch);
                    }
                }
                return true;
            }
            bytable = null;
            return false;
        }
    }
}