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
using umi3d.common.collaboration.emotes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlayMode_Tests.Collaboration.Emotes.CDK
{
    public class EmoteManager_Test
    {
        private EmoteManager emoteManagerService;

        private Mock<UMI3DEnvironmentLoader> environmentLoaderServiceMock;

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
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
            GameObject go = new GameObject("CollabServer");
            UnityEngine.Object.Instantiate(go);

            var collaborationClientServer = go.AddComponent<UMI3DCollaborationClientServer>();
            environmentLoaderServiceMock = new Mock<UMI3DEnvironmentLoader>();

            emoteManagerService = new EmoteManager(environmentLoaderServiceMock.Object,
                                                    collaborationClientServer);
        }

        [TearDown]
        public void TearDown()
        {
            if (EmoteManager.Exists)
                EmoteManager.Destroy();

            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();
        }

        [UnityTearDown]
        public void UnityTearDown()
        {
            SceneManager.UnloadSceneAsync(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);

            if (UMI3DClientServer.Exists)
                UMI3DClientServer.Destroy();

            if (UMI3DCollaborationClientServer.Exists)
                UMI3DCollaborationClientServer.Destroy();
        }

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

            // WHEN
            emoteManagerService.UpdateEmoteConfig(dto);
            environmentLoaderServiceMock.Object.onEnvironmentLoaded.Invoke();

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

            // WHEN
            emoteManagerService.UpdateEmoteConfig(dto);
            environmentLoaderServiceMock.Object.onEnvironmentLoaded.Invoke();

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

            // WHEN
            emoteManagerService.UpdateEmoteConfig(dto);
            environmentLoaderServiceMock.Object.onEnvironmentLoaded.Invoke();

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

            Mock<UMI3DCollaborationClientServer> mockServer = new();
            mockServer.Setup(x => x._SendRequest(req, true));

            UMI3DAbstractAnimationDto mockDto = new UMI3DAnimatorAnimationDto()
            {
                nodeId = 3
            };
            Mock<UMI3DAbstractAnimation> mockAnimation = new(mockDto);

            environmentLoaderServiceMock.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(emote.AnimationId)).Returns(mockAnimation.Object);

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
            Mock<UMI3DAbstractAnimation> mockAnimation = new(mockDto);

            environmentLoaderServiceMock.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(playingEmote.AnimationId)).Returns(mockAnimation.Object);

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