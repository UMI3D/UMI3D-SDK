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
using umi3d.cdk;
using umi3d.cdk.collaboration.emotes;
using umi3d.common;
using umi3d.common.collaboration.emotes;

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
            emoteManagerMock = new Mock<IEmoteService>();
            environmentManagerMock = new Mock<IEnvironmentManager>();
            emoteLoader = new UMI3DEmoteLoader(emoteManagerMock.Object, environmentManagerMock.Object);
        }

        #endregion Test SetUp

        #region ReadUMI3DExtension

        [Test, Pairwise]
        public async void ReadUMI3DExtension_Emote(
            [Range(1000uL, 2000ul, 500ul)] ulong id,
            [Values(true, false)] bool available,
            [Range(1001uL, 2001ul, 500ul)] ulong animId,
            [Values("Label", "")] string label)
        {
            // GIVEN
            emoteManagerMock.Setup(x => x.UpdateEmote(It.IsAny<UMI3DEmoteDto>()));

            var emoteDto = new UMI3DEmoteDto()
            {
                id = id,
                available = available,
                animationId = animId,
                label = label,
                iconResource = new umi3d.common.ResourceDto()
            };

            var data = new ReadUMI3DExtensionData(emoteDto);

            environmentManagerMock.Setup(x => x.RegisterEntity(It.IsAny<ulong>(), It.IsAny<UMI3DEmoteDto>(), null, null)).Returns(new UMI3DNodeInstance(() => { }));

            // WHEN
            await emoteLoader.ReadUMI3DExtension(data);

            // THEN
            emoteManagerMock.Verify(x => x.UpdateEmote(emoteDto));
        }

        #endregion ReadUMI3DExtension

        #region CanReadUMI3DExtension

        [Test]
        public void CanReadUMI3DExtension_IsEmoteDto()
        {
            // GIVEN
            var emoteDto = new UMI3DEmoteDto();

            var data = new ReadUMI3DExtensionData(emoteDto);

            // WHEN
            var canRead = emoteLoader.CanReadUMI3DExtension(data);

            // THEN
            Assert.IsTrue(canRead);
        }

        [Test]
        public void CanReadUMI3DExtension_IsNotEmoteDto()
        {
            // GIVEN
            var dto = new UMI3DDto();

            var data = new ReadUMI3DExtensionData(dto);

            // WHEN
            var canRead = emoteLoader.CanReadUMI3DExtension(data);

            // THEN
            Assert.IsFalse(canRead);
        }

        #endregion CanReadUMI3DExtension

        #region SetUMI3DProperty

        [Test]
        public async void SetUMI3DProperty_NotEmoteDto()
        {
            // GIVEN
            var setEntityDto = new SetEntityPropertyDto()
            {
                property = UMI3DPropertyKeys.ActiveEmote,
                value = true
            };

            var entityInstance = new UMI3DEntityInstance(() => { })
            {
                dto = new UMI3DDto()
            };

            var data = new SetUMI3DPropertyData(setEntityDto, entityInstance);

            // WHEN
            var success = await emoteLoader.SetUMI3DProperty(data);

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public async void SetUMI3DProperty_ActiveEmote([Values(true, false)] bool value)
        {
            // GIVEN
            var setEntityDto = new SetEntityPropertyDto()
            {
                property = UMI3DPropertyKeys.ActiveEmote,
                value = value
            };

            var entityInstance = new UMI3DEntityInstance(() => { })
            {
                dto = new UMI3DEmoteDto()
            };

            var data = new SetUMI3DPropertyData(setEntityDto, entityInstance);

            // WHEN
            var success = await emoteLoader.SetUMI3DProperty(data);

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmote(It.IsAny<UMI3DEmoteDto>()));
        }

        [Test]
        public async void SetUMI3DProperty_AnimationEmote([Range(0ul, 2000uL, 500uL)] ulong value)
        {
            // GIVEN
            var setEntityDto = new SetEntityPropertyDto()
            {
                property = UMI3DPropertyKeys.AnimationEmote,
                value = value
            };

            var entityInstance = new UMI3DEntityInstance(() => { })
            {
                dto = new UMI3DEmoteDto()
            };

            var data = new SetUMI3DPropertyData(setEntityDto, entityInstance);

            // WHEN
            var success = await emoteLoader.SetUMI3DProperty(data);

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmote(It.IsAny<UMI3DEmoteDto>()));
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

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            var readUMI3DPropertyData = new ReadUMI3DPropertyData(
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
        public async void ReadUMI3DProperty(uint propertyKey)
        {
            // set up
            var serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();
            UMI3DSerializer.AddModule(serializationModules);

            // GIVEN
            var dto = new UMI3DEmoteDto();

            Bytable data = UMI3DSerializer.Write(dto);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            var readUMI3DPropertyData = new ReadUMI3DPropertyData(
                                                                propertyKey: propertyKey,
                                                                container: byteContainer);

            // WHEN
            bool success = await emoteLoader.ReadUMI3DProperty(readUMI3DPropertyData);

            // THEN
            Assert.IsTrue(success);
            emoteManagerMock.Verify(x => x.UpdateEmote(It.IsAny<UMI3DEmoteDto>()));

            // teardown
            UMI3DSerializer.RemoveModule(serializationModules);
        }

        #endregion ReadUMI3DProperty
    }
}