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

namespace EditMode_Tests
{
    public class UMI3DDictionarySerializer_Test
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

        public void WriteRead_T<T,L>(Dictionary<T,L> value, bool expectedIsCountable)
        {
            Assert.IsTrue(UMI3DSerializer.IsCountable<T>() == expectedIsCountable, $"Expected Is Countable is {expectedIsCountable} while method return {!expectedIsCountable}");
            var bytable = UMI3DSerializer.Write(value);
            var result = UMI3DSerializer.ReadDictionary<T,L>(new ByteContainer(0, 0, bytable.ToBytes()));
            Assert.IsTrue(result.Count == value.Count, "Object deserialization failed.");
            foreach( var p in result.Zip(value,(a,b) => (a,b)))
            {
                Assert.AreEqual(p.a.Key, p.b.Key, $"values does not match {p.a.Key} => {p.b.Key}");
                Assert.AreEqual(p.a.Value, p.b.Value, $"values does not match {p.a.Value} => {p.b.Value}");
            }
        }

        [Test]
        public void WriteReadDictionnary()
        {
            Dictionary<int, int> value = new Dictionary<int, int>
            {
                { 1, 42 },
                { 13, 785 },
                { -96, 33 },
                { int.MaxValue, 1664 }
            };
            WriteRead_T(value, true);
        }

        [Test]
        public void WriteReadDictionnaryUnCountable()
        {
            Dictionary<int, string> value = new Dictionary<int, string>
            {
                { 1, "fire" },
                { 13, "jumping" },
                { -96, "jack" },
                { int.MaxValue, "Simon" }
            };
            WriteRead_T(value, true);
        }

        //[Test]
        //public void WriteReadUnCountableList()
        //{
        //    List<string> strings = new List<string>() { "", "hello  world", "this is fine" };
        //    WriteRead_T(strings, false);
        //}
    }
}