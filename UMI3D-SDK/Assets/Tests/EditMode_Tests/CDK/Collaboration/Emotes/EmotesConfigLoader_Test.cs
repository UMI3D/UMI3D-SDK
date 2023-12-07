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
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.collaboration.emotes;
using umi3d.common;
using umi3d.common.collaboration.dto.emotes;

namespace EditMode_Tests.Collaboration.Emotes.CDK
{
    [TestFixture, TestOf(typeof(UMI3DEmotesConfigLoader))]
    public class EmotesConfigLoader_Test
    {
        private UMI3DEmotesConfigLoader emotesConfigLoader;
        private Mock<IEmoteService> emoteManagerMock;
        private Mock<IEnvironmentManager> environmentManagerMock;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            emoteManagerMock = new Mock<IEmoteService>();
            environmentManagerMock = new Mock<IEnvironmentManager>();
            emotesConfigLoader = new UMI3DEmotesConfigLoader(emoteManagerMock.Object, environmentManagerMock.Object);
        }

        #endregion Test SetUp

        #region CanReadUMI3DExtension

        [Test]
        public void CanReadUMI3DExtension_IsEmotesConfigDto()
        {
            // GIVEN
            var emotesConfigDto = new UMI3DEmotesConfigDto();

            var data = new ReadUMI3DExtensionData(0, emotesConfigDto);

            // WHEN
            var canRead = emotesConfigLoader.CanReadUMI3DExtension(data);

            // THEN
            Assert.IsTrue(canRead);
        }

        [Test]
        public void CanReadUMI3DExtension_IsNotEmoteDto()
        {
            // GIVEN
            var dto = new UMI3DDto();

            var data = new ReadUMI3DExtensionData(0, dto);

            // WHEN
            var canRead = emotesConfigLoader.CanReadUMI3DExtension(data);

            // THEN
            Assert.IsFalse(canRead);
        }

        #endregion CanReadUMI3DExtension

        #region ReadUMI3DExtension

        [Test]
        public async void ReadUMI3DExtension_EmoteConfig(
            [Range(1000uL, 2000ul, 500ul)] ulong id)
        {
            // GIVEN
            var dto = new UMI3DEmotesConfigDto()
            {
                id = id,
                allAvailableByDefault = false,
                emotes = new()
            };

            var entityInstance = new UMI3DEntityInstance(0, () => { }, 0)
            {
                dto = dto
            };

            emoteManagerMock.Setup(x => x.UpdateEmoteConfig(dto));
            environmentManagerMock.Setup(x => x.RegisterEntity(0, dto.id, dto, null, null)).Returns(entityInstance);

            var data = new ReadUMI3DExtensionData(0, dto);

            // WHEN
            await emotesConfigLoader.ReadUMI3DExtension(data);

            // THEN
            emoteManagerMock.Verify(x => x.UpdateEmoteConfig(dto), Times.Once());
        }

        #endregion ReadUMI3DExtension

        #region SetUMI3DProperty

        [Test]
        public async void SetUMI3DProperty_NotEmotesConfigDto()
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
            var success = await emotesConfigLoader.SetUMI3DProperty(data);

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public async void SetUMI3DProperty_ChangeEmoteConfig()
        {
            // GIVEN
            var setEntityDto = new SetEntityPropertyDto()
            {
                property = UMI3DPropertyKeys.ChangeEmoteConfig,
                value = new UMI3DEmotesConfigDto()
            };

            var entityInstance = new UMI3DEntityInstance(0, () => { }, 0)
            {
                dto = setEntityDto.value as UMI3DEmotesConfigDto
            };

            var data = new SetUMI3DPropertyData(0, setEntityDto, entityInstance);

            // WHEN
            var success = await emotesConfigLoader.SetUMI3DProperty(data);

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmoteConfig(It.IsAny<UMI3DEmotesConfigDto>()), Times.Once());
        }

        #endregion SetUMI3DProperty

        #region ReadUMI3DProperty

        [Test]
        public async void ReadUMI3DProperty_IsNotEmotesConfig()
        {
            // set up
            var serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();
            UMI3DSerializer.AddModule(serializationModules);

            // GIVEN
            var dto = new UMI3DEmoteDto();

            Bytable data = UMI3DSerializer.Write(dto);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            var readUMI3DPropertyData = new ReadUMI3DPropertyData(0,
                                                                propertyKey: UMI3DPropertyKeys.ActiveEmote,
                                                                container: byteContainer);

            // WHEN
            var result = await emotesConfigLoader.ReadUMI3DProperty(readUMI3DPropertyData);

            // THEN
            Assert.IsFalse(result);
            emoteManagerMock.Verify(x => x.UpdateEmoteConfig(It.IsAny<UMI3DEmotesConfigDto>()), Times.Never());

            // teardown
            UMI3DSerializer.RemoveModule(serializationModules);
        }

        [Test]
        public async void ReadUMI3DProperty()
        {
            // set up
            var serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();
            UMI3DSerializer.AddModule(serializationModules);

            // GIVEN
            var dto = new UMI3DEmotesConfigDto();

            Bytable data = UMI3DSerializer.Write(dto);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            var readUMI3DPropertyData = new ReadUMI3DPropertyData(0,
                                                                propertyKey: UMI3DPropertyKeys.ChangeEmoteConfig,
                                                                container: byteContainer);

            // WHEN
            bool success = await emotesConfigLoader.ReadUMI3DProperty(readUMI3DPropertyData);

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmoteConfig(It.IsAny<UMI3DEmotesConfigDto>()), Times.Once());

            // teardown
            UMI3DSerializer.RemoveModule(serializationModules);
        }

        #endregion ReadUMI3DProperty
    }
}