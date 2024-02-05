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
using umi3d.common;
using UnityEngine;

namespace EditMode_Tests
{
    public class UMI3DListSerializer_Test
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

        public void WriteRead_T<T>(List<T> value, bool expectedIsCountable)
        {
            Assert.IsTrue(UMI3DSerializer.IsCountable<T>() == expectedIsCountable, $"Expected Is Countable is {expectedIsCountable} while method return {!expectedIsCountable}");
            var bytable = UMI3DSerializer.Write(value);
            var result = UMI3DSerializer.ReadList<T>(new ByteContainer(0, bytable.ToBytes()));
            Assert.IsTrue(result.Count == value.Count, "Object deserialization failed.");
            for (int i = 0; i < value.Count; i++)
            {
                Assert.AreEqual(value[i], result[i], $"values does not match {value} => {result} at [{i}]");
            }
        }

        [Test]
        public void WriteReadCountableList()
        {
            List<int> value = new List<int>() { 1, 3, 1000, 3994, 555};
            WriteRead_T(value,true);
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
            var result = UMI3DSerializer.ReadList<int>(new ByteContainer(0, bytable.ToBytes()));
            Assert.IsTrue(result.Count == value.Count, "Object deserialization failed.");
            for (int i = 0; i < value.Count; i++)
            {
                Assert.AreEqual(value[i], result[i], $"values does not match {value} => {result} at [{i}]");
            }
        }

        //[Test]
        //public void WriteReadCountableList2()
        //{
        //    List<ulong> value = new List<ulong>() { 1, 3, 1000, 3994, 555 };
        //    WriteRead_T(value, true);
        //}

        //public static bool IsGenericIEnumerable(Type type)
        //{
        //    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        //    {
        //        return true;
        //    }

        //    var interfaces = type.GetInterfaces();
        //    return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        //}

        //public static Type GetGenericTypeOfIEnumerable(Type type)
        //{
        //    if (IsGenericIEnumerable(type))
        //    {
        //        return type.GetGenericArguments()[0];
        //    }

        //    var enumerableInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        //    return enumerableInterface?.GetGenericArguments()[0];
        //}

        //[Test]
        //public void Test()
        //{
        //    var listType = typeof(List<int>);
        //    var dictType = typeof(Dictionary<string, int>);

        //    UnityEngine.Debug.Log($"Is {listType} a generic IEnumerable? {IsGenericIEnumerable(listType)}");
        //    UnityEngine.Debug.Log($"Is {dictType} a generic IEnumerable? {IsGenericIEnumerable(dictType)}");

        //    UnityEngine.Debug.Log($"Generic type of {listType}: {GetGenericTypeOfIEnumerable(listType)}");
        //    UnityEngine.Debug.Log($"Generic type of {dictType}: {GetGenericTypeOfIEnumerable(dictType)}");

        //}

        [Test]
        public void WriteReadUnCountableList()
        {
            List<string> strings = new List<string>() { "", "hello  world", "this is fine" };
            WriteRead_T(strings, false);
        }
    }
}