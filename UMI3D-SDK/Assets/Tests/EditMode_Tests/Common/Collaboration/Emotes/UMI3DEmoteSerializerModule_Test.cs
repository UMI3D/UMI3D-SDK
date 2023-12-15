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
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.collaboration.dto.emotes;
using umi3d.common.collaboration.emotes;

namespace EditMode_Tests.Collaboration.Emotes.Common
{
    public class UMI3DEmoteSerializerModule_Test
    {
        protected List<UMI3DSerializerModule> dependenciesSerializationModules = new();

        protected UMI3DEmoteSerializerModule emotesSerializerModule = new();

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            dependenciesSerializationModules = new()
            {
                 new UMI3DSerializerBasicModules(),
                 new UMI3DSerializerVectorModules(),
                 new UMI3DSerializerStringModules(),
                 new UMI3DCollaborationSerializerModule()
            };

            UMI3DSerializer.AddModule(dependenciesSerializationModules);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            UMI3DSerializer.RemoveModule(dependenciesSerializationModules);
            dependenciesSerializationModules.Clear();
        }

        #endregion Test SetUp

        #region WriteRead

        [Test]
        [TestCase(1005uL, true, 1482uL, "Hello")]
        [TestCase(1008uL, false, 1297uL, "Dancing")]
        public void WriteRead(ulong id, bool available, ulong animationId, string label)
        {
            // GIVEN
            UMI3DEmoteDto dto = new()
            {
                id = id,
                available = available,
                animationId = animationId,
                label = label,
                iconResource = new umi3d.common.ResourceDto() { variants = new() { new() } }
            };

            // WHEN
            emotesSerializerModule.Write(dto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            emotesSerializerModule.Read(byteContainer, out bool readable, out UMI3DEmoteDto result);

            // THEN
            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(dto.id, result.id);
            Assert.AreEqual(dto.available, result.available);
            Assert.AreEqual(dto.animationId, result.animationId);
            Assert.AreEqual(dto.label, result.label);
            Assert.AreEqual(dto.iconResource.variants.Count, result.iconResource.variants.Count);
        }

        #endregion WriteRead
    }
}