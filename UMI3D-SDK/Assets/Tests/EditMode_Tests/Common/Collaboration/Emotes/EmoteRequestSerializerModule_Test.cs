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
    public class EmoteRequestSerializerModule_Test
    {
        protected List<UMI3DSerializerModule> dependenciesSerializationModules = new();

        protected EmoteRequestSerializerModule emoteRequestSerializerModule = new();

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
        [TestCase(10005uL, false)]
        [TestCase(20008uL, true)]
        public void WriteRead(ulong emoteId, bool trigger)
        {
            // GIVEN
            EmoteRequestDto dto = new()
            {
                emoteId = emoteId,
                shouldTrigger = trigger
            };

            // WHEN
            emoteRequestSerializerModule.Write(dto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            UMI3DSerializer.TryRead<uint>(byteContainer, out _); // emote request property id
            emoteRequestSerializerModule.Read(byteContainer, out bool readable, out EmoteRequestDto result);

            // THEN
            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(dto.emoteId, result.emoteId);
            Assert.AreEqual(dto.shouldTrigger, result.shouldTrigger);
        }

        #endregion WriteRead
    }
}