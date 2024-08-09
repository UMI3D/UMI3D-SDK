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
        [TestCase("\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\"\r\nSection 1.10.32 du \"De Finibus Bonorum et Malorum\" de Ciceron (45 av. J.-C.)\r\n\r\n\"Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?\"\r\nTraduction de H. Rackham (1914)\r\n\r\n\"But I must explain to you how all this mistaken idea of denouncing pleasure and praising pain was born and I will give you a complete account of the system, and expound the actual teachings of the great explorer of the truth, the master-builder of human happiness. No one rejects, dislikes, or avoids pleasure itself, because it is pleasure, but because those who do not know how to pursue pleasure rationally encounter consequences that are extremely painful. Nor again is there anyone who loves or pursues or desires to obtain pain of itself, because it is pain, but because occasionally circumstances occur in which toil and pain can procure him some great pleasure. To take a trivial example, which of us ever undertakes laborious physical exercise, except to obtain some advantage from it? But who has any right to find fault with a man who chooses to enjoy a pleasure that has no annoying consequences, or one who avoids a pain that produces no resultant pleasure?\"\r\nSection 1.10.33 du \"De Finibus Bonorum et Malorum\" de Ciceron (45 av. J.-C.)\r\n\r\n\"At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat.\"\r\nTraduction de H. Rackham (1914)\r\n\r\n\"On the other hand, we denounce with righteous indignation and dislike men who are so beguiled and demoralized by the charms of pleasure of the moment, so blinded by desire, that they cannot foresee the pain and trouble that are bound to ensue; and equal blame belongs to those who fail in their duty through weakness of will, which is the same as saying through shrinking from toil and pain. These cases are perfectly simple and easy to distinguish. In a free hour, when our power of choice is untrammelled and when nothing prevents our being able to do what we like best, every pleasure is to be welcomed and every pain avoided. But in certain circumstances and owing to the claims of duty or the obligations of business it will frequently occur that pleasures have to be repudiated and annoyances accepted. The wise man therefore always holds in these matters to this principle of selection: he rejects pleasures to secure other greater pleasures, or else he endures pains to avoid worse pains.\"")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi vulputate consectetur diam vel gravida. Maecenas ac metus eleifend, vehicula turpis eget, mattis velit. Sed pretium libero justo, ac pharetra turpis maximus nec. Sed lacinia bibendum nunc, sed feugiat nunc maximus ut. In hac habitasse platea dictumst. In sollicitudin sem enim, a feugiat ipsum iaculis a. Donec scelerisque consectetur auctor. Pellentesque consequat aliquam facilisis. Cras non felis eros. Nullam in consequat ipsum. Nullam auctor interdum nisl. Sed vitae commodo felis, et placerat nunc. Lorem ipsum dolor sit amet, consectetur adipiscing elit.\r\nEtiam ac lorem non elit vulputate pharetra. Duis massa velit, facilisis nec purus tristique, semper aliquet lorem. Sed consequat sagittis urna, ut auctor magna vehicula id. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Phasellus vel molestie felis, eget molestie nulla. Aenean eu pharetra mauris. Pellentesque at cursus eros, eu faucibus mi. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Suspendisse potenti. In hac habitasse platea dictumst. Mauris felis lacus, viverra sit amet augue facilisis, congue tincidunt libero. Nunc euismod, sapien vel porttitor sollicitudin, nisi augue blandit erat, id tempor ipsum libero non felis.\r\nAliquam pellentesque ultrices tortor, vel convallis nisi sollicitudin fermentum. Praesent ullamcorper ante sapien, nec laoreet ligula vehicula tincidunt. Fusce malesuada id magna non aliquam. Proin pharetra varius blandit. Sed sodales cursus interdum. Maecenas at ex rhoncus, condimentum erat quis, lobortis dolor. Vivamus gravida rutrum turpis, sed faucibus erat porttitor ut. Suspendisse semper nisl in varius placerat. Fusce malesuada ipsum at lacus lobortis placerat. Aenean ut sollicitudin nunc. Ut quam enim, bibendum in metus eu, fringilla tempor purus. Donec sapien massa, dapibus pulvinar augue ut, lobortis ultricies ipsum. Suspendisse potenti. In finibus purus sed varius faucibus.")]
        public void WriteRead_String( string value)
        {
            WriteRead_T(value);
        }

        [Test]
        public void WriteMaxString()
        {
            string s = "";
            for(int i = 0; i < 50000; i++)
            {
                s += "a";
            }
            WriteRead_T(s);
        }

        [TestCase(null)]
        public void WriteRead_StringNull(string value)
        {
            WriteRead_T(value,string.Empty);
        }

    }
}