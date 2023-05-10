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

namespace EditMode_Tests.Collaboration.Emotes.CDK
{
    [TestFixture]
    public class EmoteManager_Test
    {
        private EmoteManager emoteManagerService;

        private Mock<UMI3DEnvironmentLoader> environmentLoaderServiceMock;

        private Mock<UMI3DCollaborationClientServer> collaborationClientServerMock;

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
            environmentLoaderServiceMock = new Mock<UMI3DEnvironmentLoader>();
            collaborationClientServerMock = new Mock<UMI3DCollaborationClientServer>();

            emoteManagerService = new EmoteManager(environmentLoaderServiceMock.Object, null); // monobehaviour are not called during edit mode tests
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


    }
}