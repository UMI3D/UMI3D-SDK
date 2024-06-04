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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d;
using umi3d.common;
using UnityEngine;

namespace EditMode_Tests
{
    public class UMI3DSerializerModule_Test
    {
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();
            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        (bool readable, T result) WriteRead<T>(T value)
        {
            var bytable = UMI3DSerializer.Write(value);
            bool readable = UMI3DSerializer.TryRead<T>(new ByteContainer(0,0,bytable.ToBytes(), UMI3DVersion.ComputedVersion), out T result);
            return (readable, result);
        }

        public void WriteRead_T<T>(T value)
        {
            (bool readable, T result) = WriteRead(value);
            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(value, result, $"values does not match {value} => {result}");
        }

        public void WriteRead_T<T>(T value, T expected)
        {
            (bool readable, T result) = WriteRead(value);
            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(expected, result, $"values does not match {value} => {result} != {expected}");
        }

        #region test SerializerModule
        [Test]
        public void WriteReadTest()
        {
            WriteRead_T(new TestToSerialize() { hello = 42, world = 0.59f });
            WriteRead_T(new TestToSerialize2() { hello = "this is the end", world = 320 });
            WriteRead_T(new TestToSerialize3() { hello = 1452, world = 1515 });
        }

        class TestToSerialize : IEquatable<TestToSerialize>
        {
            public int hello { get; set; }
            public float world { get; set; }

            public bool Equals(TestToSerialize other)
            {
                return hello == other.hello && world == other.world;
            }
        }

        class TestToSerialize2 : IEquatable<TestToSerialize2>
        {
            public string hello { get; set; }
            public ulong world { get; set; }

            public bool Equals(TestToSerialize2 other)
            {
                return hello == other.hello && world == other.world;
            }
        }

        class TestToSerialize3 : IEquatable<TestToSerialize3>
        {
            public long hello { get; set; }
            public ushort world { get; set; }

            public bool Equals(TestToSerialize3 other)
            {
                return hello == other.hello && world == other.world;
            }
        }

        class TestToSerializeSerializer : UMI3DSerializerModule<TestToSerialize2>, UMI3DSerializerModule<TestToSerialize>, UMI3DSerializerModule
        {
            bool UMI3DSerializerModule<TestToSerialize2>.IsCountable()
            {
                return true;
            }

            bool UMI3DSerializerModule<TestToSerialize>.IsCountable()
            {
                return true;
            }

            bool? UMI3DSerializerModule.IsCountable<T>()
            {
                return typeof(T) == typeof(TestToSerialize3) ? true : null;
            }

            bool UMI3DSerializerModule<TestToSerialize2>.Read(ByteContainer container, out bool readable, out TestToSerialize2 result)
            {
                if(UMI3DSerializer.TryRead(container,out string hello)
                    && UMI3DSerializer.TryRead(container, out ulong world))
                {
                    readable = true;
                    result = new TestToSerialize2() { hello = hello, world = world };
                    return true;
                }
                readable = false;
                result = null;
                return false;
            }

            bool UMI3DSerializerModule<TestToSerialize>.Read(ByteContainer container, out bool readable, out TestToSerialize result)
            {
                if (UMI3DSerializer.TryRead(container, out int hello)
                    && UMI3DSerializer.TryRead(container, out float world))
                {
                    readable = true;
                    result = new TestToSerialize() { hello = hello, world = world };
                    return true;
                }
                readable = false;
                result = null;
                return false;
            }

            bool UMI3DSerializerModule.Read<T>(ByteContainer container, out bool readable, out T result)
            {
                if (typeof(T) == typeof(TestToSerialize3)
                   && UMI3DSerializer.TryRead(container, out long hello)
                   && UMI3DSerializer.TryRead(container, out ushort world))
                {
                    readable = true;
                    result = (T)(object)new TestToSerialize3() { hello = hello, world = world };
                    return true;
                }
                readable = false;
                result = default;
                return false;
            }

            bool UMI3DSerializerModule<TestToSerialize2>.Write(TestToSerialize2 value, out Bytable bytable, params object[] parameters)
            {
                bytable = UMI3DSerializer.Write(value.hello) + UMI3DSerializer.Write(value.world);
                return true;
            }

            bool UMI3DSerializerModule<TestToSerialize>.Write(TestToSerialize value, out Bytable bytable, params object[] parameters)
            {
                bytable = UMI3DSerializer.Write(value.hello) + UMI3DSerializer.Write(value.world);
                return true;
            }

            bool UMI3DSerializerModule.Write<T>(T value, out Bytable bytable, params object[] parameters)
            {
                if (value is TestToSerialize3 test)
                {
                    bytable = UMI3DSerializer.Write(test.hello) + UMI3DSerializer.Write(test.world);
                    return true;
                }
                bytable = default;
                return false;
            }
        }
        #endregion

        [TestCase('\0')]
        [TestCase('\uFFFF')]
        [Test]
        public void WriteRead_char([Random('\0', '\uFFFF', 100)] char value)
        {
            WriteRead_T(value);
        }

        [Test]
        public void WriteRead_bool([Values]bool value)
        {
            WriteRead_T(value);
        }

        [TestCase(byte.MinValue)]
        [TestCase(byte.MaxValue)]
        [Test]
        public void WriteRead_byte([Random(byte.MinValue, byte.MaxValue, 100)]byte value)
        {
            WriteRead_T(value);
        }

        [TestCase(short.MinValue)]
        [TestCase(short.MaxValue)]
        [Test]
        public void WriteRead_short([Random(short.MinValue, short.MaxValue, 100)] short value)
        {
            WriteRead_T(value);
        }

        [TestCase(ushort.MinValue)]
        [TestCase(ushort.MaxValue)]
        [Test]
        public void WriteRead_ushort([Random(ushort.MinValue, ushort.MaxValue, 100)] ushort value)
        {
            WriteRead_T(value);
        }

        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        [Test]
        public void WriteRead_int([Random(int.MinValue, int.MaxValue, 100)] int value)
        {
            WriteRead_T(value);
        }

        [TestCase(uint.MinValue)]
        [TestCase(uint.MaxValue)]
        [Test]
        public void WriteRead_uint([Random(uint.MinValue, uint.MaxValue, 100)] uint value)
        {
            WriteRead_T(value);
        }

        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [Test]
        public void WriteRead_float([Random(float.MinValue, float.MaxValue, 100)] float value)
        {
            WriteRead_T(value);
        }

        [TestCase(long.MinValue)]
        [TestCase(long.MaxValue)]
        [Test]
        public void WriteRead_long([Random(long.MinValue, long.MaxValue, 100)] long value)
        {
            WriteRead_T(value);
        }

        [TestCase(ulong.MinValue)]
        [TestCase(ulong.MaxValue)]
        [Test]
        public void WriteRead_ulong([Random(ulong.MinValue, ulong.MaxValue, 100)] ulong value)
        {
            WriteRead_T(value);
        }

        [TestCase("")]
        [TestCase("Portez ce vieux whisky au juge blond qui fume.")]
        public void WriteRead_String( string value)
        {
            WriteRead_T(value);
        }

        [TestCase(null)]
        public void WriteRead_StringNull(string value)
        {
            WriteRead_T(value,string.Empty);
        }

    }
}