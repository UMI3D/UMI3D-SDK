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
using System.Collections.Generic;
using System.Linq;
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
            serializationModules = UMI3DSerializerModule.GetModules().ToList();

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
            bool readable = UMI3DSerializer.TryRead<T>(new ByteContainer(0,bytable.ToBytes()), out T result);
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