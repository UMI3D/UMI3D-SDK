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
using System.Linq;
using System.Text.RegularExpressions;
using umi3d.common;
using UnityEngine;
using System;

namespace umi3d.common
{
    public class OperationReader : MonoBehaviour
    {
        public bool ignorePos;
        [TextArea]
        public string container;
        public List<ByteTester> testers;


        public void Decrypt()
        {
            testers = substring(container).Select(b => new ByteTester(b)).ToList();
        }


        List<ByteContainer> substring(string containerString)
        {
            Regex regex = new Regex(@"\[((?:[0-9]+;)+[0-9]+)][^\[]]*\[([0-9]+) : ([0-9]+)]");

            var m = regex.Match(containerString);
            var result = new List<ByteContainer>();

            while (m.Success && m.Groups.Count >= 3)
            {
                var bss = m.Groups[1].Captures.Cast<Capture>().Select(s => s.Value).Select(s => s.Split(';').Select(sb => byte.Parse(sb)).ToArray());
                var pss = m.Groups[2].Captures.Cast<Capture>().Select(s => s.Value).Select(s => int.Parse(s));
                var lss = m.Groups[3].Captures.Cast<Capture>().Select(s => s.Value).Select(s => int.Parse(s));

                if (ignorePos)
                    result.AddRange(pss.Zip(lss, (pos, len) => (pos, len)).Zip(bss, (a, b) => new ByteContainer(0, b)));
                else
                    result.AddRange(pss.Zip(lss, (pos, len) => (pos, len)).Zip(bss, (a, b) => new ByteContainer(0, b) { position = a.pos, length = a.len }));
                m = m.NextMatch();
            }
            return result;
        }



    }

    [Serializable]
    public class ByteTester
    {
        public ByteContainer container;
        public List<Result> results;
        public TypeToSerialize type;
        public float height;

        public class Result
        {
            public string type;
            public string name;
            public object value;
            public TypeToEmum convertType;

            public Result(string type, string name, object value)
            {
                this.type = type;
                this.name = name;
                this.value = value;
            }
        }


        public enum TypeToSerialize { Byte, Short, UShort, Int, Uint, Long, Ulong, Char, String }
        public enum TypeToEmum { PropertyKey, OperationKey, BoneType }

        public ByteTester(ByteContainer container)
        {
            this.container = container;
            results = new();
        }

        public void Read(TypeToSerialize type)
        {
            switch (type)
            {
                case TypeToSerialize.Int:
                    Read<int>();
                    break;
                case TypeToSerialize.Uint:
                    Read<uint>();
                    break;
                case TypeToSerialize.Long:
                    Read<long>();
                    break;
                case TypeToSerialize.Ulong:
                    Read<ulong>();
                    break;
                case TypeToSerialize.Byte:
                    Read<byte>();
                    break;
                case TypeToSerialize.Short:
                    Read<short>();
                    break;
                case TypeToSerialize.UShort:
                    Read<ushort>();
                    break;
                case TypeToSerialize.Char:
                    Read<char>();
                    break;
                case TypeToSerialize.String:
                    Read<string>();
                    break;
                default:
                    throw new Exception("Missing Case " + type);
            }
        }

        public void Read<T>()
        {

            UMI3DSerializer.AddModule(new UMI3DSerializerBasicModules());
            UMI3DSerializer.AddModule(new UMI3DSerializerStringModules());
            UMI3DSerializer.AddModule(new UMI3DSerializerVectorModules());
            UMI3DSerializer.AddModule(new UMI3DSerializerAnimationModules());
            UMI3DSerializer.AddModule(new UMI3DSerializerShaderModules());

            T result;
            if (UMI3DSerializer.TryRead<T>(container, out result))
            {
                this.results.Add(new(typeof(T).Name, result.ToString(), result));
            }
        }
    }
}