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

using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d;
using umi3d.common;
using UnityEngine;

namespace EditMode_Tests.Serializer
{
    public class UMI3DListSerializer_Test
    {
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();

            UMI3DSerializer.AddModule(serializationModules);
            UMI3DSerializer.AddModule(new UMI3DSerializerUncountableModules());
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        public void WriteRead_T<T>(List<T> value, bool expectedIsCountable)
        {
            Assert.IsTrue(UMI3DSerializer.IsCountable<T>() == expectedIsCountable, $"Expected Is Countable is {expectedIsCountable} while method return {!expectedIsCountable}");
            var bytable = UMI3DSerializer.Write(value);
            var result = UMI3DSerializer.ReadList<T>(new ByteContainer(0, 0, bytable.ToBytes(), UMI3DVersion.ComputedVersion));
            Assert.IsTrue(result.Count == value.Count, "Object deserialization failed.");
            for (int i = 0; i < value.Count; i++)
            {
                Assert.AreEqual(value[i], result[i], $"values does not match {value} => {result} at [{i}]");
            }
        }

        [Test]
        public void WriteReadCountableList()
        {
            List<int> value = new List<int>() { 1, 3, 1000, 3994, 555 };
            WriteRead_T(value, true);
        }

        class TestClass
        {
            public int value;
        }
        [Test]
        public void WriteReadSelectList()
        {
            List<int> value = new List<int>() { 1, 3, 1000, 3994, 555 };
            var v2 = value.Select(i => new TestClass() { value = i }).ToList();


            var v3 = v2.Select(c => c.value);

            var bytable = UMI3DSerializer.Write(v3);
            var result = UMI3DSerializer.ReadList<int>(new ByteContainer(0, 0, bytable.ToBytes(), UMI3DVersion.ComputedVersion));
            Assert.IsTrue(result.Count == value.Count, "Object deserialization failed.");
            for (int i = 0; i < value.Count; i++)
            {
                Assert.AreEqual(value[i], result[i], $"values does not match {value} => {result} at [{i}]");
            }
        }


        public class Uncountable
        {
            public int value;

            public Uncountable(int value)
            {
                this.value = value;
            }

            public override bool Equals(object obj)
            {
                return obj is Uncountable u && value.Equals(u.value);
            }
        }


        public class UMI3DSerializerUncountableModules : UMI3DSerializerModule
        {
            public bool? IsCountable<T>()
            {
                if (typeof(T) == typeof(Uncountable))
                    return false;
                return null;
            }

            public bool Read<T>(ByteContainer container, out bool readable, out T result)
            {
                readable = true;
                switch (true)
                {
                    case true when typeof(T) == typeof(Uncountable):
                        if (UMI3DSerializer.TryRead<int>(container, out int s))
                        {
                            result = (T)(object)new Uncountable(s);
                            readable = true;
                            return true;
                        }

                        result = default(T);
                        readable = false;
                        return true;

                }

                result = default(T);
                readable = false;
                return false;
            }

            public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
            {
                Func<byte[], int, int, (int, int)> f;

                if (value is Uncountable u)
                {

                        var str = value as string;
                        bytable = UMI3DSerializer.Write(u.value);

                    return true;
                }
                bytable = null;
                return false;
            }
        }

        [Test]
        public void WriteReadUnCountableList()
        {
            List<Uncountable> strings = new List<Uncountable>() { new(1), new(2)};
            WriteRead_T(strings, false);
        }

        [Test]
        public void WriteRead_ListNull()
        {
            WriteRead_List<int>(null);
        }

        [Test]
        public void WriteRead_ListEmpty()
        {
            WriteRead_List(new List<int>());
        }

        [Test]
        public void WriteRead_ListInt()
        {
            WriteRead_List(new List<int>() { 1, 2, 3, 4 });
        }

        [Test]
        public void WriteRead_ListString()
        {
            WriteRead_List(new List<string>() { "1", "2", "3", "4" });
        }

        public void WriteRead_List<T>(List<T> value)
        {
            int val = 42;
            var bytable = UMI3DSerializer.WriteCollection(value);
            bytable += UMI3DSerializer.Write(val);

            var byt = new ByteContainer(0, 0, bytable.ToBytes(), UMI3DVersion.ComputedVersion);

            List<T> readable = UMI3DSerializer.ReadList<T>(byt);
            int v = UMI3DSerializer.Read<int>(byt);
            Assert.IsTrue(readable != null, "Object deserialization failed.");
            Assert.AreEqual(val, v, $"test end does not match {value} => {val} != {v}");
            Assert.AreEqual(value?.Count ?? 0, readable?.Count, $"Count does not match {value} => {value?.Count} != {readable?.Count}");
            if (value != null && readable != null)
                foreach ((T a, T b) in value.Zip(readable, (a, b) => (a, b)))
                    Assert.AreEqual(a, b, $"values does not match {value} => {a} != {b}");
        }
    }
}