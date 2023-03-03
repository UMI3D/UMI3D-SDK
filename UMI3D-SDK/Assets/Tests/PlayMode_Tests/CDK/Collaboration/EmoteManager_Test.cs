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
using umi3d.common.collaboration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlayMode_Tests
{
    public class EmoteManager_Test
    {
        private const string TEST_SCENE_NAME = "Tests/PlayMode_Tests/TestScenes/TESTSCENE_Empty";

        private EmoteManager emoteManagerService;

        private Mock<UMI3DEnvironmentLoader> environmentLoaderServiceMock;

        [SetUp]
        public void SetUp()
        {
            SceneManager.LoadScene(TEST_SCENE_NAME);
            GameObject go = new GameObject("CollabServer");
            UnityEngine.Object.Instantiate(go);

            UMI3DCollaborationClientServer clientServer = go.AddComponent<UMI3DCollaborationClientServer>();

            Debug.Log(clientServer);

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

        [UnityTearDown]
        public void UnityTearDown()
        {
            SceneManager.UnloadSceneAsync(TEST_SCENE_NAME);

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
            emoteManagerService.LoadEmoteConfig(dto);
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
            emoteManagerService.LoadEmoteConfig(dto);
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
            emoteManagerService.LoadEmoteConfig(dto);
            environmentLoaderServiceMock.Object.onEnvironmentLoaded.Invoke();

            // THEN
            Assert.IsTrue(wasEmotesLoaded);
            Assert.AreEqual(dto.emotes.Count, emoteManagerService.Emotes.Count);
            foreach (var emote in emoteManagerService.Emotes)
                Assert.IsTrue(emote.available);
        }

        #endregion LoadEmoteConfig
    }
}