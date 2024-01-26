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
        public void UpdateEmote_NotReceivedEmote()
        {
            // GIVEN
            UMI3DEmoteDto emoteDto = new()
            {
                id = 0,
                animationId = 0,
                label = ""
            };

            bool wasEmoteUpdatedTriggered = false;
            emoteManagerService.EmoteUpdated += delegate { wasEmoteUpdatedTriggered = true; };

            // WHEN
            emoteManagerService.UpdateEmote(emoteDto);

            // THEN
            Assert.IsFalse(wasEmoteUpdatedTriggered);
        }

        #endregion UpdateEmote

        #region LoadEmoteConfig

        // Playmode because of use of UMI3DCollaborationClientServer
        [Test]
        public void Test_LoadEmoteConfig_Empty()
        {
            // GIVEN
            UMI3DEmotesConfigDto dto = new()
            {
                id = 1,
                emotes = new(),
                allAvailableByDefault = false
            };

            bool wasEmotesLoaded = false;
            emoteManagerService.EmotesLoaded += delegate { wasEmotesLoaded = true; };

            loadingManagerMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());

            // WHEN
            emoteManagerService.UpdateEmoteConfig(dto);
            loadingManagerMock.Object.onEnvironmentLoaded.Invoke();

            // THEN
            Assert.IsTrue(wasEmotesLoaded);
            Assert.IsEmpty(emoteManagerService.Emotes);
        }

        [Test]
        public void Test_LoadEmoteConfig_SeveralEmotes()
        {
            // GIVEN
            UMI3DEmotesConfigDto dto = new()
            {
                id = 1,
                emotes = new()
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
                },
                allAvailableByDefault = false
            };

            bool wasEmotesLoaded = false;
            emoteManagerService.EmotesLoaded += delegate { wasEmotesLoaded = true; };

            loadingManagerMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());

            // WHEN
            emoteManagerService.UpdateEmoteConfig(dto);
            loadingManagerMock.Object.onEnvironmentLoaded.Invoke();

            // THEN
            Assert.IsTrue(wasEmotesLoaded);
            Assert.AreEqual(dto.emotes.Count, emoteManagerService.Emotes.Count);
        }

        [Test]
        public void Test_LoadEmoteConfig_SeveralEmotes_AllAvailableOverride()
        {
            // GIVEN
            UMI3DEmotesConfigDto dto = new()
            {
                id = 1,
                emotes = new()
                {
                    new UMI3DEmoteDto()
                    {
                        id = 2,
                        available = false
                    },
                    new UMI3DEmoteDto()
                    {
                        id = 3,
                        available = false
                    }
                },
                allAvailableByDefault = true
            };

            bool wasEmotesLoaded = false;
            emoteManagerService.EmotesLoaded += delegate { wasEmotesLoaded = true; };

            loadingManagerMock.Setup(x => x.onEnvironmentLoaded).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());
            collaborationClientServerMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());

            // WHEN
            emoteManagerService.UpdateEmoteConfig(dto);
            loadingManagerMock.Object.onEnvironmentLoaded.Invoke();

            // THEN
            Assert.IsTrue(wasEmotesLoaded);
            Assert.AreEqual(dto.emotes.Count, emoteManagerService.Emotes.Count);
            foreach (var emote in emoteManagerService.Emotes)
                Assert.IsTrue(emote.available);
        }

        #endregion LoadEmoteConfig

        #region PlayEmote

        [Test]
        public void PlayEmote_Available()
        {
            // GIVEN
            bool wasEmoteStartedInvoked = false;
            emoteManagerService.EmoteStarted += delegate { wasEmoteStartedInvoked = true; };

            Emote emote = new Emote()
            {
                environmentId = 0L,
                icon = null,
                available = true,
                dto = new UMI3DEmoteDto()
                {
                    id = 1005uL,
                    label = "TestEmote",
                    animationId = 2
                }
            };

            EmoteRequestDto req = new EmoteRequestDto()
            {
                emoteId = emote.dto.id,
                shouldTrigger = true
            };

            Mock<IUMI3DCollaborationClientServer> mockServer = new();
            mockServer.Setup(x => x._SendRequest(req, true));

            UMI3DAbstractAnimationDto mockDto = new UMI3DAnimatorAnimationDto()
            {
                nodeId = 3
            };
            Mock<UMI3DAbstractAnimation> mockAnimation = new(MockBehavior.Default,0UL, mockDto);

            environmentManagerMock.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(0UL, emote.AnimationId)).Returns(mockAnimation.Object);

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

            Emote emote = new Emote()
            {
                icon = null,
                available = false,
                dto = new UMI3DEmoteDto()
                {
                    id = 1005uL,
                    label = "TestEmote",
                    animationId = 0
                }
            };

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
            bool wasEmoteStoppedInvoked = false;
            emoteManagerService.EmoteEnded += (emote) => { wasEmoteStoppedInvoked = true; };

            Emote playingEmote = new Emote()
            {
                icon = null,
                available = true,
                dto = new UMI3DEmoteDto()
                {
                    id = 1,
                    label = "TestEmote",
                    animationId = 2
                }
            };
            Mock<Emote> mockEmote = new Mock<Emote>(playingEmote);

            UMI3DAnimatorAnimationDto mockDto = new UMI3DAnimatorAnimationDto();
            Mock<UMI3DAbstractAnimation> mockAnimation = new(MockBehavior.Default,0UL ,mockDto);

            environmentManagerMock.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(UMI3DGlobalID.EnvironmentId, playingEmote.AnimationId)).Returns(mockAnimation.Object);

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