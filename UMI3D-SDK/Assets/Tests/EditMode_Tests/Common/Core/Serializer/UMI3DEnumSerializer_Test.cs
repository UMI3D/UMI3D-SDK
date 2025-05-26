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

using AutoFixture;
using inetum.unityUtils.autoFixture;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d;
using umi3d.common;

namespace EditMode_Tests.Serializer
{
    public class UMI3DEnumSerializer_Test
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
            bool readable = UMI3DSerializer.TryRead<T>(new ByteContainer(0, 0, bytable.ToBytes(), UMI3DVersion.ComputedVersion), out T result);
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

        #region Test Enum detection
        public void Test<T>(bool expectedIsEnum = true)
        {
            Test<T, int>(expectedIsEnum);
        }

        public void Test<T, UT>(bool expectedIsEnum = true)
        {
            bool result = UMI3DSerializerBasicEnumModules.EDITOR_ONLY_IsEnumType(typeof(T), out Type underlyingType);

            Assert.AreEqual(expectedIsEnum, result);
            Assert.AreEqual(expectedIsEnum ? typeof(UT) : null, underlyingType);
        }

        public void Test_obj(object obj, bool expectedIsEnum = true)
        {
            Test_obj<int>(obj, expectedIsEnum);
        }

        public void Test_obj<UT>(object obj, bool expectedIsEnum = true)
        {
            if (obj == null)
                return;

            bool result = UMI3DSerializerBasicEnumModules.EDITOR_ONLY_IsEnumType(obj.GetType(), out Type underlyingType);

            Assert.AreEqual(expectedIsEnum, result);
            Assert.AreEqual(expectedIsEnum ? typeof(UT) : null, underlyingType);
        }

        [Test]
        public void Test_Type_Other() => Test<string>(false);
        [Test]
        public void Test_Obj_Other() => Test_obj("Hello",false);

        public enum A { a, b, c, d }

        [Test]
        public void Test_Type_Enum() => Test<A>();
        [Test]
        public void Test_Obj_Enum() => Test_obj(A.a);

        public enum B : short { a, b, c, d }

        [Test]
        public void Test_Type_Enum_Short() => Test<B, short>();
        [Test]
        public void Test_Obj_Enum_Short() => Test_obj<short>(B.b);
        #endregion Test Enum detection

        [Test]
        public void WriteRead_A()
        {
            Fixture fixture = new();
            fixture.Customizations.Add(new RandomEnumGenerator());
            A a = fixture.Create<A>();

            WriteRead_T(a);
        }

        [Test]
        public void WriteRead_B()
        {
            Fixture fixture = new();
            fixture.Customizations.Add(new RandomEnumGenerator());
            B b = fixture.Create<B>();

            WriteRead_T(b);
        }
    }
}