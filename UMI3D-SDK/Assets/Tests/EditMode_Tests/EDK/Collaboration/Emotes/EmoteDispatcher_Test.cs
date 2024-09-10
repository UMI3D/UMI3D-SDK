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
using umi3d.edk;
using umi3d.edk.collaboration.emotes;
using UnityEngine;

namespace EditMode_Tests.Collaboration.Emotes.EDK
{
    [TestFixture, TestOf(nameof(EmoteDispatcher))]
    public class EmoteDispatcher_Test
    {
        private Mock<IUMI3DEnvironmentManager> umi3dEnvironmentMock;
        private Mock<IUMI3DServer> umi3dServerMock;
        private EmoteDispatcher emoteDispatcher;

        private void CleanSingletons()
        {
            if (EmoteDispatcher.Exists)
                EmoteDispatcher.Destroy();
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            CleanSingletons();
        }

        [SetUp]
        public void SetUp()
        {
            umi3dEnvironmentMock = new();
            umi3dServerMock = new();

            umi3dServerMock.Setup(x => x.OnUserActive).Returns(new UMI3DUserEvent());
            umi3dServerMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());
            umi3dServerMock.Setup(x => x.OnUserRefreshed).Returns(new UMI3DUserEvent());
            umi3dServerMock.Setup(x => x.OnUserMissing).Returns(new UMI3DUserEvent());

            emoteDispatcher = new EmoteDispatcher(umi3dEnvironmentMock.Object, umi3dServerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CleanSingletons();
        }

        #region DispatchEmoteTrigger

        /// <summary>
        /// Given no emotes config declared,
        /// When an emote is requested to be triggered,
        /// Then no emote is triggered.
        /// </summary>
        /// <param name="emoteId"></param>
        /// <param name="trigger"></param>
        [TestCase(102uL, true)]
        [TestCase(102uL, false)]
        [Test, TestOf(nameof(EmoteDispatcher.DispatchEmoteTrigger))]
        public void DispatchEmoteTrigger_NoEmoteConfig(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();

            umi3dServerMock.Setup(x => x.DispatchTransaction(It.IsAny<Transaction>())).Verifiable();

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            umi3dServerMock.Verify(x => x.DispatchTransaction(It.IsAny<Transaction>()), Times.Never);
        }

        /// <summary>
        /// Given no emote declared,
        /// When an emote is requested to be triggered,
        /// Then no emote is triggered.
        /// </summary>
        /// <param name="emoteId"></param>
        /// <param name="trigger"></param>
        [TestCase(102uL, true)]
        [TestCase(102uL, false)]
        [Test]
        public void DispatchEmoteTrigger_NoEmotes(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();
            emoteDispatcher.EmotesConfigs.Add(0uL, ScriptableObject.CreateInstance<UMI3DEmotesConfig>());

            umi3dServerMock.Setup(x => x.DispatchTransaction(It.IsAny<Transaction>())).Verifiable();

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            umi3dServerMock.Verify(x => x.DispatchTransaction(It.IsAny<Transaction>()), Times.Never);
        }

        /// <summary>
        /// Given an emotes config with an emote A declared,
        /// When an emote B is requested to be triggered,
        /// Then no emote is triggered.
        /// </summary>
        /// <param name="emoteId"></param>
        /// <param name="trigger"></param>
        [TestCase(102uL, true)]
        [TestCase(102uL, false)]
        [Test]
        public void DispatchEmoteTrigger_InvalidEmote(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();

            UMI3DEmotesConfig emotesConfig = ScriptableObject.CreateInstance<UMI3DEmotesConfig>();
            Mock<UMI3DEmote> mockEmote = new();
            emotesConfig.IncludedEmotes = new()
            {
                mockEmote.Object
            };
            mockEmote.Setup(x => x.Id()).Returns(15uL);

            emoteDispatcher.EmotesConfigs.Add(0uL, emotesConfig);

            umi3dServerMock.Setup(x => x.DispatchTransaction(It.IsAny<Transaction>())).Verifiable();

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            umi3dServerMock.Verify(x => x.DispatchTransaction(It.IsAny<Transaction>()), Times.Never);
        }

        /// <summary>
        /// Given an emotes config with an emote A declared, but not available for the user,
        /// When an emote A is requested to be triggered,
        /// Then no emote is triggered.
        /// </summary>
        /// <param name="emoteId"></param>
        /// <param name="trigger"></param>
        [TestCase(102uL, true)]
        [TestCase(102uL, false)]
        [Test]
        public void DispatchEmoteTrigger_UnavailableEmote(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();

            UMI3DEmotesConfig emotesConfig = ScriptableObject.CreateInstance<UMI3DEmotesConfig>();
            Mock<UMI3DEmote> mockEmote = new();
            emotesConfig.IncludedEmotes = new()
            {
                mockEmote.Object
            };
            mockEmote.Setup(x => x.Id()).Returns(emoteId);
            mockEmote.Setup(x => x.Available).Returns(new UMI3DAsyncProperty<bool>(0, 0, false));

            emoteDispatcher.EmotesConfigs.Add(0uL, emotesConfig);

            umi3dServerMock.Setup(x => x.DispatchTransaction(It.IsAny<Transaction>())).Verifiable();

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            umi3dServerMock.Verify(x => x.DispatchTransaction(It.IsAny<Transaction>()), Times.Never);
        }

        /// <summary>
        /// Given an emotes config with an emote A declared, but the animation cannot be found,
        /// When an emote A is requested to be triggered,
        /// Then no emote is triggered.
        /// </summary>
        /// <param name="emoteId"></param>
        /// <param name="trigger"></param>
        [TestCase(102uL, true)]
        [TestCase(102uL, false)]
        [Test]
        public void DispatchEmoteTrigger_AnimationNotFound(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();

            UMI3DEmotesConfig emotesConfig = ScriptableObject.CreateInstance<UMI3DEmotesConfig>();
            Mock<UMI3DEmote> mockEmote = new();
            emotesConfig.IncludedEmotes = new()
            {
                mockEmote.Object
            };

            ulong animationId = 10023uL;

            mockEmote.Setup(x => x.Id()).Returns(emoteId);
            mockEmote.Setup(x => x.Available).Returns(new UMI3DAsyncProperty<bool>(0, 0, true));
            mockEmote.Setup(x => x.AnimationId).Returns(new UMI3DAsyncProperty<ulong>(0, 0, animationId));

            emoteDispatcher.EmotesConfigs.Add(0uL, emotesConfig);

            umi3dServerMock.Setup(x => x.DispatchTransaction(It.IsAny<Transaction>())).Verifiable();
            UMI3DAbstractAnimation animation = null;
            umi3dEnvironmentMock.Setup(x => x._GetEntityInstance<UMI3DAbstractAnimation>(animationId)).Returns(animation).Verifiable();

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            umi3dServerMock.Verify(x => x.DispatchTransaction(It.IsAny<Transaction>()), Times.Never);
        }

        /// <summary>
        /// Given an emotes config with an emote A declared, but the animation cannot be found,
        /// When an emote A is requested to be triggered,
        /// Then no emote is triggered.
        /// </summary>
        /// <param name="emoteId"></param>
        /// <param name="trigger"></param>
        [TestCase(102uL, true)]
        [TestCase(102uL, false)]
        [Test]
        public void DispatchEmoteTrigger(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();

            UMI3DEmotesConfig emotesConfig = ScriptableObject.CreateInstance<UMI3DEmotesConfig>();
            Mock<UMI3DEmote> mockEmote = new();
            emotesConfig.IncludedEmotes = new()
            {
                mockEmote.Object
            };

            ulong animationId = 10023uL;

            mockEmote.Setup(x => x.Id()).Returns(emoteId);
            mockEmote.Setup(x => x.Available).Returns(new UMI3DAsyncProperty<bool>(0uL, 0u, true));
            mockEmote.Setup(x => x.AnimationId).Returns(new UMI3DAsyncProperty<ulong>(0uL, 0u, animationId));

            emoteDispatcher.EmotesConfigs.Add(0uL, emotesConfig);

            umi3dServerMock.Setup(x => x.DispatchTransaction(It.IsAny<Transaction>())).Verifiable();

            Mock<IAnimation> mockAnimation = new();
            var mockPropertyChanged = new Mock<UMI3DAsyncProperty<bool>>(MockBehavior.Loose, 0uL, 0u, !trigger, null, null);
            mockPropertyChanged.Setup(x => x.SetValue(trigger, false)).Returns(new SetEntityProperty());
            mockAnimation.Setup(x => x.objectPlaying).Returns(mockPropertyChanged.Object);

            // impossible to mock monobehaviour
            umi3dEnvironmentMock.Setup(x => x._GetEntityInstance<IAnimation>(animationId)).Returns(mockAnimation.Object);

            emoteDispatcher.EmoteTriggered += CallbackToCall;

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            umi3dServerMock.Verify(x => x.DispatchTransaction(It.IsAny<Transaction>()), Times.Once);

            void CallbackToCall((UMI3DUser sendingUser, ulong emoteId, bool isTrigger) emoteTrigggeredInfo)
            {
                Assert.AreEqual(user.Id(), emoteTrigggeredInfo.sendingUser.Id());
                Assert.AreEqual(emoteId, emoteTrigggeredInfo.emoteId);
                Assert.AreEqual(trigger, emoteTrigggeredInfo.isTrigger);
            }
        }

        #endregion DispatchEmoteTrigger
    }
}