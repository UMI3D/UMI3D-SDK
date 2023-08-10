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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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
            testers = substring(container).Select(b => new ByteTester(b,this)).ToList();
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
        public OperationReader operationReader;
        public List<Result> results;
        public TypeToSerialize type;
        public float height;
        public bool display;




        [Serializable]
        public class Result
        {
            public string type;
            public string name;
            public object value;

            public Enum? convertType;


            public string actionName;
            public Action action;

            public Result()
            {
                this.type = "";
                this.name = "";
                this.value = "";
                this.action = null;
                this.actionName = null;

            }

            public Result(string type, string name, object value) : this()
            {
                this.type = type;
                this.name = name;
                this.value = value;
                ComputeEnum();
            }

            public Result(string type, string name, object value, string actionName, Action action) : this(type,name,value)
            {
                this.actionName = actionName;
                this.action = action;
            }
            public Result(string actionName, Action action) : this()
            {
                this.actionName = actionName;
                this.action = action;
            }

            public void Do()
            {
                action?.Invoke();
            }

            void ComputeEnum()
            {
                switch (value)
                {
                    case uint _:
                        convertType = TypeToEmum.PropertyKey;
                        break;
                    default:
                        convertType = null;
                        break;
                }
            }


        }


        public enum TypeToSerialize { Byte, Short, UShort, Int, Uint, Long, Ulong, Char, String, List }
        public enum TypeToEmum { PropertyKey, OperationKey, BoneType }




        public ByteTester(ByteContainer container, OperationReader operationReader)
        {
            this.container = container;
            this.operationReader = operationReader;
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
                case TypeToSerialize.List:
                    ReadList();
                    break;
                default:
                    throw new Exception("Missing Case " + type);
            }
        }

        public T Read<T>()
        {
            UMI3DSerializer.AddModule(UMI3DSerializerModuleUtils.GetModules().ToList());

            T result;
            if (UMI3DSerializer.TryRead<T>(container, out result))
            {
                this.results.Add(new(typeof(T).Name, result.ToString(), result));
                return result;
            }
            return default(T);
        }

        void ReadList()
        {
            var t = Read<byte>();
            switch (t)
            {
                case UMI3DObjectKeys.CountArray:
                    ReadCountList();
                    break;
                case UMI3DObjectKeys.IndexesArray:
                     ReadIndexedList();
                    break;
                default:
                    this.results.Add(new("","Na Matching list type",""));
                    break;
            }
        }

        void ReadCountList()
        {
            this.results.Add(new("", "Not implemented", ""));
        }

        void ReadIndexedList()
        {
            int i = 0;
            int indexMaxPos = -1;
            int maxLength = container.bytes.Length;
            int valueIndex = -1;
            while( container.position < indexMaxPos || indexMaxPos == -1)
            {
                int nopIndex = UMI3DSerializer.Read<int>(container);
                
                if (indexMaxPos == -1)
                {
                    this.results.Add(new(typeof(int).Name, nopIndex.ToString(), nopIndex));
                    indexMaxPos = valueIndex = nopIndex;
                    continue;
                }
               
                var SubContainer = new ByteContainer(container.timeStep, container.bytes) { position = valueIndex, length = nopIndex - valueIndex };
                var byteTester = new ByteTester(SubContainer, operationReader);
                this.results.Add(new(typeof(int).Name, nopIndex.ToString(), nopIndex, $"Tester {i++}", () => { operationReader.testers.Add(byteTester); }));
                valueIndex = nopIndex;
            }
            {
                var SubContainer = new ByteContainer(container.timeStep, container.bytes) { position = valueIndex, length = maxLength - valueIndex };
                var byteTester = new ByteTester(SubContainer, operationReader);
                this.results.Add(new($"Tester {i}", () => { operationReader.testers.Add(byteTester); }));
            }
        }




        //public void ReadNextIndexedList()
        //{
        //    var last = results.Last();
        //    int nopIndex = UMI3DSerializer.Read<int>(container);
        //    this.results.Add(new(typeof(int).Name, nopIndex.ToString(), nopIndex));
        //    if (last.value is int valueIndex)
        //    {
        //        var SubContainer = new ByteContainer(container.timeStep, container.bytes) { position = valueIndex, length = nopIndex - valueIndex };
        //        operationReader.testers.Add(new ByteTester(SubContainer, operationReader));
        //    }
        //}
    }
}