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

using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.collaboration.emotes;
using umi3d.common;
using umi3d.common.collaboration.dto.emotes;

namespace EditMode_Tests.Collaboration.Emotes.CDK
{
    [TestFixture, TestOf(typeof(UMI3DEmoteLoader))]
    public class EmoteLoader_Test
    {
        private UMI3DEmoteLoader emoteLoader;
        private Mock<IEmoteService> emoteManagerMock;
        private Mock<IEnvironmentManager> environmentManagerMock;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            emoteManagerMock = new ();
            environmentManagerMock = new ();
            emoteLoader = new UMI3DEmoteLoader(emoteManagerMock.Object, environmentManagerMock.Object);
        }

        #endregion Test SetUp

        #region ReadUMI3DExtension

        [Test, Pairwise, TestOf(nameof(UMI3DEmoteLoader.Load))]
        public void Load([Range(1000uL, 2000ul, 500ul)] ulong id,
                                [Values(true, false)] bool available,
                                [Range(1001uL, 2001ul, 500ul)] ulong animId,
                                [Values("Label", "")] string label)

        {
            // GIVEN

            ulong environmentId = 0;

            var dto = new UMI3DEmoteDto()
            {
                id = id,
                available = available,
                animationId = animId,
                label = label,
                iconResource = new umi3d.common.ResourceDto()
            };

            var entityInstance = new UMI3DEntityInstance(environmentId, () => { }, dto.id)
            {
                dto = dto,
            };
            environmentManagerMock.Setup(x => x.RegisterEntity(environmentId, dto.id, dto, It.IsAny<Emote>(), null)).Returns(entityInstance);

            // WHEN
            Task<Emote> task = emoteLoader.Load(environmentId, dto);
            task.Wait();
            Emote emote = task.Result;


            // THEN
            Assert.IsNotNull(emote);
            Assert.AreEqual(dto.id, emote.Id);
            Assert.AreEqual(dto.available, emote.available);
            Assert.AreEqual(dto.animationId, emote.AnimationId);
            Assert.AreEqual(dto.label, emote.Label);
        }

        #endregion Load

        #region SetUMI3DProperty

        [Test]
        public void SetUMI3DProperty_NotEmoteDto()
        {
            // GIVEN
            var setEntityDto = new SetEntityPropertyDto()
            {
                property = UMI3DPropertyKeys.ActiveEmote,
                value = true
            };

            var entityInstance = new UMI3DEntityInstance(0, () => { }, 0)
            {
                dto = new UMI3DDto()
            };

            var data = new SetUMI3DPropertyData(0, setEntityDto, entityInstance);

            // WHEN
            Task<bool> task = emoteLoader.SetUMI3DProperty(data);
            task.Wait();
            bool success = task.Result;

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void SetUMI3DProperty_ActiveEmote([Values(true, false)] bool value)
        {
            // GIVEN
            ulong environmentId = 0;
            var setEntityDto = new SetEntityPropertyDto()
            {
                property = UMI3DPropertyKeys.ActiveEmote,
                value = value
            };

            var dto = new UMI3DEmoteDto() { id = 102uL };
            var entityInstance = new UMI3DEntityInstance(environmentId, () => { }, 0)
            {
                dto = dto
            };

            var data = new SetUMI3DPropertyData(environmentId, setEntityDto, entityInstance);

            Emote emote = new Emote(dto, null);
            environmentManagerMock.Setup(x => x.TryGetEntity<Emote>(environmentId, dto.id, out emote)).Returns(true);

            emoteManagerMock.Setup(x => x.UpdateEmote(It.IsAny<Emote>()));

            // WHEN
            Task<bool> task = emoteLoader.SetUMI3DProperty(data);
            task.Wait();
            bool success = task.Result;

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmote(It.IsAny<Emote>()));
        }

        [Test]
        public void SetUMI3DProperty_AnimationEmote([Range(0ul, 2000uL, 500uL)] ulong value)
        {
            // GIVEN
            ulong environmentId = 0;
            var setEntityDto = new SetEntityPropertyDto()
            {
                property = UMI3DPropertyKeys.AnimationEmote,
                value = value
            };

            var dto = new UMI3DEmoteDto() { id = 102uL };
            var entityInstance = new UMI3DEntityInstance(environmentId, () => { }, 0)
            {
                dto = dto
            };

            var data = new SetUMI3DPropertyData(environmentId, setEntityDto, entityInstance);

            Emote emote = new Emote(dto, null);
            environmentManagerMock.Setup(x => x.TryGetEntity<Emote>(environmentId, dto.id, out emote)).Returns(true);

            emoteManagerMock.Setup(x => x.UpdateEmote(It.IsAny<Emote>()));

            // WHEN
            Task<bool> task = emoteLoader.SetUMI3DProperty(data);
            task.Wait();
            bool success = task.Result;

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmote(It.IsAny<Emote>()));
        }

        #endregion SetUMI3DProperty

        #region ReadUMI3DProperty

        [Test]
        public void ReadUMI3DProperty_IsNotEmote()
        {
            // set up
            var serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();
            UMI3DSerializer.AddModule(serializationModules);

            // GIVEN
            var dto = new UMI3DEmotesConfigDto();

            Bytable data = UMI3DSerializer.Write(dto);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            var readUMI3DPropertyData = new ReadUMI3DPropertyData(0,
                                                                propertyKey: UMI3DPropertyKeys.ActiveEmote,
                                                                container: byteContainer);

            // WHEN
            bool action()
            {
                var task = emoteLoader.ReadUMI3DProperty(readUMI3DPropertyData);
                task.Wait();
                return task.Result;
            }

            // THEN
            Assert.Throws<Exception>(() =>
            {
                try
                {
                    action();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });

            // teardown
            UMI3DSerializer.RemoveModule(serializationModules);
        }

        [Test]
        [TestCase(UMI3DPropertyKeys.ActiveEmote)]
        [TestCase(UMI3DPropertyKeys.AnimationEmote)]
        public void ReadUMI3DProperty(uint propertyKey)
        {
            // set up
            var serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();
            UMI3DSerializer.AddModule(serializationModules);

            // GIVEN
            ulong environmentId = 0;
            var dto = new UMI3DEmoteDto() { id = 102uL };

            Bytable data = UMI3DSerializer.Write(dto);

            ByteContainer byteContainer = new ByteContainer(environmentId, 1, data.ToBytes());

            var readUMI3DPropertyData = new ReadUMI3DPropertyData(environmentId,
                                                                                    propertyKey: propertyKey,
                                                                                    container: byteContainer);

            Emote emote = new Emote(dto, null);
            environmentManagerMock.Setup(x=>x.TryGetEntity<Emote>(environmentId, dto.id, out emote)).Returns(true);

            emoteManagerMock.Setup(x => x.UpdateEmote(It.IsAny<Emote>()));


            // WHEN
            Task<bool> task = emoteLoader.ReadUMI3DProperty(readUMI3DPropertyData);
            task.Wait();
            bool success = task.Result;

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmote(It.IsAny<Emote>()));

            // teardown
            UMI3DSerializer.RemoveModule(serializationModules);
        }

        #endregion ReadUMI3DProperty
    }
}