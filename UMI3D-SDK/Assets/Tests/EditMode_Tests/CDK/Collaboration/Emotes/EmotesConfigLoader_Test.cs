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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private Mock<ILoader<UMI3DEmoteDto, Emote>> emoteLoaderMock;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            emoteManagerMock = new ();
            environmentManagerMock = new ();
            emoteLoaderMock = new ();
            emotesConfigLoader = new UMI3DEmotesConfigLoader(emoteManagerMock.Object, environmentManagerMock.Object, emoteLoaderMock.Object);
        }

        #endregion Test SetUp

        #region Load

        [Test, TestOf(nameof(UMI3DEmotesConfigLoader.Load))]
        public void Load([Values(true, false)] bool allAvailableByDefault)
        {
            // GIVEN
            ulong environmentId = 0;
            var dto = new UMI3DEmotesConfigDto()
            {
                id = 1025uL,
                allAvailableByDefault = allAvailableByDefault,
                emotes = new() { new(), new()}
            };

            var entityInstance = new UMI3DEntityInstance(environmentId, () => { }, dto.id)
            {
                dto = dto,
            };

            emoteLoaderMock.Setup(x=>x.Load(environmentId, It.IsAny<UMI3DEmoteDto>())).Returns((ulong a, UMI3DEmoteDto e) => Task.FromResult(new Emote(e, null)));
            emoteManagerMock.Setup(x => x.AddEmoteConfig(It.IsAny<EmotesConfig>()));
            environmentManagerMock.Setup(x => x.RegisterEntity(environmentId, dto.id, dto, It.IsAny<EmotesConfig>(), null)).Returns(entityInstance);

            // WHEN
            Task<EmotesConfig> task = emotesConfigLoader.Load(environmentId, dto);
            task.Wait();
            EmotesConfig emotesConfig = task.Result;

            // THEN
            emoteManagerMock.Verify(x => x.AddEmoteConfig(It.IsAny<EmotesConfig>()), Times.Once());
            Assert.IsNotNull(emotesConfig);
            Assert.AreEqual(dto.id, emotesConfig.Id);
            Assert.AreEqual(dto.allAvailableByDefault, emotesConfig.AllAvailableByDefault);
            Assert.AreEqual(dto.emotes.Count, emotesConfig.Emotes.Count);
        }

        #endregion Load
    }
}