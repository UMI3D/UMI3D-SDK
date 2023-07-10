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
using umi3d.common.collaboration;
using umi3d.common.collaboration.dto.emotes;
using umi3d.common.collaboration.emotes;

namespace EditMode_Tests.Collaboration.Emotes.Common
{
    public class UMI3DEmotesConfigSerializerModule_Test
    {
        protected List<UMI3DSerializerModule> dependenciesSerializationModules = new();

        protected UMI3DEmotesConfigSerializerModule emotesConfigSerializerModule = new();

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            dependenciesSerializationModules = new()
            {
                 new UMI3DSerializerBasicModules(),
                 new UMI3DSerializerVectorModules(),
                 new UMI3DSerializerStringModules(),
                 UMI3DSerializerModuleUtils.Instanciate(typeof(UMI3DEmoteSerializerModule)).First(),
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
        [TestCase(1005uL, true, 3)]
        [TestCase(1008uL, false, 10)]
        [TestCase(1008uL, false, 0)]
        public void WriteRead(ulong id, bool allAvailable, int nbEmotes)
        {
            // GIVEN
            UMI3DEmotesConfigDto dto = new()
            {
                id = id,
                allAvailableByDefault = allAvailable,
            };
            for (int i = 0; i < nbEmotes; i++)
            {
                dto.emotes.Add(new UMI3DEmoteDto()
                {
                    iconResource = new umi3d.common.ResourceDto() { variants = new() { new() } }
                });
            }

            // WHEN
            emotesConfigSerializerModule.Write(dto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            emotesConfigSerializerModule.Read(byteContainer, out bool readable, out UMI3DEmotesConfigDto result);

            // THEN
            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(dto.id, result.id);
            Assert.AreEqual(dto.allAvailableByDefault, result.allAvailableByDefault);
            Assert.AreEqual(dto.emotes.Count, result.emotes.Count);
            for (int i = 0; i < nbEmotes; i++)
            {
                Assert.AreEqual(dto.emotes[i].id, result.emotes[i].id);
                Assert.AreEqual(dto.emotes[i].available, result.emotes[i].available);
                Assert.AreEqual(dto.emotes[i].animationId, result.emotes[i].animationId);
                Assert.AreEqual(dto.emotes[i].label, result.emotes[i].label);
                Assert.AreEqual(dto.emotes[i].iconResource.variants.Count, result.emotes[i].iconResource.variants.Count);
            }
        }

        #endregion WriteRead
    }
}