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
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.collaboration.emotes;
using umi3d.common;
using umi3d.common.collaboration.dto.emotes;

namespace EditMode_Tests.Collaboration.Emotes.CDK
{
    [TestFixture]
    public class EmoteManager_Test
    {
        private EmoteManager emoteManagerService;

        private Mock<ILoadingManager> loadingManagerMock;
        private Mock<IEnvironmentManager> environmentManagerMock;

        private Mock<IUMI3DCollaborationClientServer> collaborationClientServerMock;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (EmoteManager.Exists)
                EmoteManager.Destroy();

            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();
        }

        [SetUp]
        public void SetUp()
        {
            loadingManagerMock = new Mock<ILoadingManager>();
            environmentManagerMock = new Mock<IEnvironmentManager>();
            collaborationClientServerMock = new Mock<IUMI3DCollaborationClientServer>();

            emoteManagerService = new EmoteManager(loadingManagerMock.Object,
                                                   environmentManagerMock.Object,
                                                   collaborationClientServerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (EmoteManager.Exists)
                EmoteManager.Destroy();

            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();
        }

        #endregion Test SetUp

        #region UpdateEmote

        [Test]
        public void UpdateEmote()
        {
            // GIVEN
            UMI3DEmoteDto emoteDto = new()
            {
                id = 0,
                animationId = 0,
                label = ""
            };

            Emote emote = new(emoteDto, null);

            bool wasEmoteUpdatedTriggered = false;
            emoteManagerService.EmoteUpdated += delegate { wasEmoteUpdatedTriggered = true; };

            // WHEN
            emoteManagerService.UpdateEmote(emote);

            // THEN
            Assert.IsTrue(wasEmoteUpdatedTriggered);
        }

        #endregion UpdateEmote

        #region AddEmoteConfig

        [Test]
        public void AddEmoteConfig_Empty()
        {
            // GIVEN
            UMI3DEmotesConfigDto dto = new()
            {
                id = 1,
                emotes = new(),
                allAvailableByDefault = false
            };

            EmotesConfig emotesConfig = new EmotesConfig(dto, new List<Emote>());

            bool wasEmotesLoaded = false;
            emoteManagerService.EmotesLoaded += delegate { wasEmotesLoaded = true; };

            loadingManagerMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());

            // WHEN
            emoteManagerService.AddEmoteConfig(emotesConfig);
            loadingManagerMock.Object.onEnvironmentLoaded.Invoke();

            // THEN
            Assert.IsTrue(wasEmotesLoaded);
            Assert.IsEmpty(emoteManagerService.Emotes);
        }

        [Test]
        public void AddEmoteConfig_SeveralEmotes()
        {
            // GIVEN

            List<UMI3DEmoteDto> emoteDtos = new List<UMI3DEmoteDto>()
            {
                new UMI3DEmoteDto()
                {
                    id = 2,
                    available = true
                },
                new UMI3DEmoteDto()
                {
                    id = 3,
                    available = false
                }
            };

            UMI3DEmotesConfigDto dto = new()
            {
                id = 1,
                emotes = emoteDtos,
                allAvailableByDefault = false
            };

            EmotesConfig emotesConfig = new EmotesConfig(dto, emoteDtos.Select(d => new Emote(d, null)).ToList());

            bool wasEmotesLoaded = false;
            emoteManagerService.EmotesLoaded += delegate { wasEmotesLoaded = true; };

            loadingManagerMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());

            // WHEN
            emoteManagerService.AddEmoteConfig(emotesConfig);
            loadingManagerMock.Object.onEnvironmentLoaded.Invoke();

            // THEN
            Assert.IsTrue(wasEmotesLoaded);
            Assert.AreEqual(dto.emotes.Count, emoteManagerService.Emotes.Count);
        }

        [Test]
        public void AddEmoteConfig_SeveralEmotes_AllAvailableOverride()
        {
            // GIVEN

            List<UMI3DEmoteDto> emoteDtos = new List<UMI3DEmoteDto>()
            {
                new UMI3DEmoteDto()
                {
                    id = 2,
                    available = true
                },
                new UMI3DEmoteDto()
                {
                    id = 3,
                    available = false
                }
            };

            UMI3DEmotesConfigDto dto = new()
            {
                id = 1,
                emotes = emoteDtos,
                allAvailableByDefault = true
            };

            EmotesConfig emotesConfig = new EmotesConfig(dto, emoteDtos.Select(d => new Emote(d, null)).ToList());

            bool wasEmotesLoaded = false;
            emoteManagerService.EmotesLoaded += delegate { wasEmotesLoaded = true; };

            loadingManagerMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());

            // WHEN
            emoteManagerService.AddEmoteConfig(emotesConfig);
            loadingManagerMock.Object.onEnvironmentLoaded.Invoke();

            // THEN
            Assert.IsTrue(wasEmotesLoaded);
            Assert.AreEqual(dto.emotes.Count, emoteManagerService.Emotes.Count);
            foreach (var emote in emoteManagerService.Emotes)
                Assert.IsTrue(emote.available);
        }

        #endregion AddEmoteConfig

        #region PlayEmote

        [Test]
        public void PlayEmote_Available()
        {
            // GIVEN
            ulong environmentId = 0;
            bool wasEmoteStartedInvoked = false;
            emoteManagerService.EmoteStarted += delegate { wasEmoteStartedInvoked = true; };

            UMI3DEmoteDto dto = new UMI3DEmoteDto()
            {
                id = 1005uL,
                label = "TestEmote",
                animationId = 1516uL,
                available = true
            };

            Emote emote = new Emote(dto, null, environmentId);

            EmoteRequestDto req = new EmoteRequestDto()
            {
                emoteId = emote.Id,
                shouldTrigger = true
            };

            Mock<IUMI3DCollaborationClientServer> mockServer = new();
            mockServer.Setup(x => x._SendRequest(req, true));

            UMI3DAbstractAnimationDto mockDto = new UMI3DAnimatorAnimationDto()
            {
                nodeId = 3
            };
            Mock<UMI3DAbstractAnimation> mockAnimation = new(MockBehavior.Default, environmentId, mockDto);
            UMI3DAbstractAnimation foundAnimation = mockAnimation.Object;
            environmentManagerMock.Setup(x => x.TryGetEntity<UMI3DAbstractAnimation>(environmentId, emote.AnimationId, out foundAnimation)).Returns(true);

            // WHEN
            emoteManagerService.PlayEmote(emote);

            // THEN
            Assert.IsTrue(wasEmoteStartedInvoked);
            Assert.IsTrue(emoteManagerService.IsPlaying);
        }

        [Test]
        public void PlayEmote_Unavailable()
        {
            // GIVEN
            bool wasEmoteStartedInvoked = false;
            emoteManagerService.EmoteStarted += delegate { wasEmoteStartedInvoked = true; };

            UMI3DEmoteDto dto = new UMI3DEmoteDto()
            {
                id = 1005uL,
                label = "TestEmote",
                animationId = 1516uL,
                available = false
            };

            Emote emote = new Emote(dto, null);

            // WHEN
            emoteManagerService.PlayEmote(emote);

            // THEN
            Assert.IsFalse(wasEmoteStartedInvoked);
            Assert.IsFalse(emoteManagerService.IsPlaying);
        }

        #endregion PlayEmote

        #region StopEmote

        [Test]
        public void StopEmote_NoEmotePlaying()
        {
            // GIVEN
            bool wasEmoteStoppedInvoked = false;
            emoteManagerService.EmoteStarted += delegate { wasEmoteStoppedInvoked = true; };

            // WHEN
            emoteManagerService.StopEmote();

            // THEN
            Assert.IsFalse(wasEmoteStoppedInvoked);
            Assert.IsFalse(emoteManagerService.IsPlaying);
        }

        [Test]
        public void StopEmote_EmotePlaying()
        {
            // GIVEN
            ulong environmentId = 0;
            bool wasEmoteStoppedInvoked = false;
            emoteManagerService.EmoteEnded += (emote) => { wasEmoteStoppedInvoked = true; };

            UMI3DEmoteDto dto = new UMI3DEmoteDto()
            {
                id = 1005uL,
                label = "TestEmote",
                animationId = 1516uL,
                available = true
            };

            Emote playingEmote = new Emote(dto, null, environmentId);

            UMI3DAbstractAnimationDto mockDto = new UMI3DAnimatorAnimationDto()
            {
                nodeId = 3
            };
            Mock<UMI3DAbstractAnimation> mockAnimation = new(MockBehavior.Default, environmentId, mockDto);
            UMI3DAbstractAnimation foundAnimation = mockAnimation.Object;
            environmentManagerMock.Setup(x => x.TryGetEntity<UMI3DAbstractAnimation>(environmentId, playingEmote.AnimationId, out foundAnimation)).Returns(true);

            EmoteRequestDto req = new EmoteRequestDto()
            {
                emoteId = playingEmote.Id,
                shouldTrigger = true
            };

            Mock<IUMI3DCollaborationClientServer> mockServer = new();
            mockServer.Setup(x => x._SendRequest(req, true));

            // WHEN
            emoteManagerService.PlayEmote(playingEmote);
            emoteManagerService.StopEmote();

            // THEN
            Assert.IsTrue(wasEmoteStoppedInvoked);
            Assert.IsFalse(emoteManagerService.IsPlaying);
        }

        #endregion StopEmote
    }
}