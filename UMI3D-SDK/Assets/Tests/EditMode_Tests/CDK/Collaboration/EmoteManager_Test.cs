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
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.common.collaboration;

namespace EditMode_Tests
{
    public class EmoteManager_Test
    {
        private EmoteManager emoteManagerService;

        private Mock<UMI3DEnvironmentLoader> environmentLoaderServiceMock;

        [SetUp]
        public void SetUp()
        {
            environmentLoaderServiceMock = new Mock<UMI3DEnvironmentLoader>();

            emoteManagerService = new EmoteManager(environmentLoaderServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (EmoteManager.Exists)
                EmoteManager.Destroy();

            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();
        }

        #region UpdateEmote

        [Test]
        public void Test_UpdateEmote_NotReceivedEmote()
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

        #region PlayEmote

        [Test]
        public void Test_PlayEmote_Available()
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
                    id = 1,
                    label = "TestEmote",
                    animationId = 2
                }
            };

            EmoteRequest req = new EmoteRequest()
            {
                emoteId = emote.dto.id,
                shouldTrigger = true
            };

            Mock<UMI3DClientServer> mockServer = new();
            mockServer.Setup(x => x.SendRequest(req, true));

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
        public void Test_PlayEmote_Unavailable()
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
                    id = 0,
                    label = "TestEmote",
                    animationId = 0
                }
            };

            // WHEN
            Action toFail = () => emoteManagerService.PlayEmote(emote);

            // THEN
            Assert.Throws<Umi3dException>(() => toFail());
            Assert.IsFalse(wasEmoteStartedInvoked);
            Assert.IsFalse(emoteManagerService.IsPlaying);
        }

        #endregion PlayEmote

        #region StopEmote

        [Test]
        public void Test_StopEmote_NoEmotePlaying()
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
        public void Test_StopEmote_EmotePlaying()
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