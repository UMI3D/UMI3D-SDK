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

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using umi3d.edk.userCapture.tracking;
using umi3d.edk.collaboration;

namespace PlayMode_Tests.UserCapture.Tracking.EDK
{
    [TestFixture, TestOf(typeof(UMI3DTrackingManager))]
    public class UMI3DTrackingManager_Test
    {
        protected UMI3DTrackingManager trackingManager;
        protected GameObject serverGo;

        protected float newFPSTracking = 18f;

        #region Test Setup

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.TEST_SCENE_EDK_BASE);
        }

        [SetUp]
        public void Setup()
        {
            trackingManager = UMI3DTrackingManager.Instance;
            _ = UMI3DCollaborationEnvironment.Instance;
            serverGo = new GameObject();
            UnityEngine.Object.Instantiate(serverGo);
            serverGo.AddComponent<UMI3DCollaborationServer>();
            //var server = UMI3DCollaborationServer.Instance; 

            for (int i = 0; i < UnityEngine.Random.Range(4,15); i++)
            {
                UMI3DCollaborationServer.Collaboration.CreateUser(new umi3d.common.collaboration.RegisterIdentityDto() { guid = Guid.NewGuid().ToString(), login = i.ToString() }, (a, b) => { });
            }
        }

        [TearDown] 
        public void TearDown()
        {
            UnityEngine.Object.Destroy(serverGo);
            UMI3DTrackingManager.Destroy();
        }

        #endregion Test Setup

        #region SyncFPSTracking

        [Test]
        public void SyncFPSTracking_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncFPS.Add(users[i].Id(), newFPSTracking);
            }

            // WHEN
            trackingManager.SyncFPSTracking();

            // THEN
            Assert.AreNotEqual(users.Count, trackingManager.asyncFPS.Count);
            Assert.IsTrue(trackingManager.asyncFPS.Count == 0);
        }

        [Test]
        public void SyncFPSTracking_Async_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncFPS.Add(users[i].Id(), newFPSTracking);
            }

            var randIndex = UnityEngine.Random.Range(0, users.Count);

            // WHEN
            trackingManager.SyncFPSTracking(users[randIndex]);

            // THEN
            Assert.AreEqual(users.Count - 1, trackingManager.asyncFPS.Count);
            Assert.IsTrue(!trackingManager.asyncFPS.ContainsKey((ulong)randIndex));
        }

        #endregion SyncFPSTracking

        #region UpdateFPSTracking

        [Test]
        public void UpdateFPSTracking_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            // WHEN
            trackingManager.UpdateFPSTracking(newFPSTracking);

            // THEN
            Assert.AreEqual(users.Count, trackingManager.asyncFPS.Count);
            Assert.IsTrue(trackingManager.asyncFPS.Count != 0);
        }

        [Test]
        public void UpdateFPSTracking_Async_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randIndex = UnityEngine.Random.Range(0, users.Count);

            // WHEN
            trackingManager.UpdateFPSTracking(users[randIndex], newFPSTracking);

            // THEN
            Assert.IsTrue(!trackingManager.asyncFPS.ContainsKey((ulong)randIndex));
            Assert.IsTrue(trackingManager.asyncFPS.Count == 1);
        }

        #endregion UpdateFPSTracking

        #region UpdateBoneFPS

        [Test]
        public void UpdateBoneFPS_NewFPS_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randBonetype = (uint)UnityEngine.Random.Range(1, 59);

            // WHEN
            trackingManager.UpdateBoneFPS(newFPSTracking, randBonetype);

            // THEN
            Assert.AreEqual(users.Count, trackingManager.asyncBonesFPS.Count);
            Assert.IsTrue(trackingManager.asyncBonesFPS.Count != 0);
            foreach (var item in trackingManager.asyncBonesFPS)
            {
                Assert.IsTrue(item.Value.Count == 1);
                Assert.IsTrue(item.Value.ContainsKey(randBonetype));
            }
        }

        [Test]
        public void UpdateBoneFPS_NewFPS_Async_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randIndex = UnityEngine.Random.Range(0, users.Count);
            var randBonetype = (uint)UnityEngine.Random.Range(1, 59);

            // WHEN
            trackingManager.UpdateBoneFPS(users[randIndex], newFPSTracking, randBonetype);

            // THEN
            Assert.IsTrue(trackingManager.asyncBonesFPS.ContainsKey(users[randIndex].Id()));
            Assert.IsTrue(trackingManager.asyncBonesFPS.Count == 1);
            Assert.IsTrue(trackingManager.asyncBonesFPS[users[randIndex].Id()].Count == 1);
            Assert.IsTrue(trackingManager.asyncBonesFPS[users[randIndex].Id()].ContainsKey(randBonetype));
        }

        [Test]
        public void UpdateBoneFPS_TrackingFPS_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randBonetype = (uint)UnityEngine.Random.Range(1, 59);

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncBonesFPS.Add(users[i].Id(), new() { { randBonetype, newFPSTracking } });
            }

            // WHEN
            trackingManager.UpdateBoneFPS(trackingManager.FPSTrackingTarget, randBonetype);

            // THEN
            Assert.AreEqual(users.Count, trackingManager.asyncBonesFPS.Count);
            Assert.IsTrue(trackingManager.asyncBonesFPS.Count != 0);
            foreach (var item in trackingManager.asyncBonesFPS)
            {
                Assert.IsTrue(item.Value.Count == 0);
            }
        }

        [Test]
        public void UpdateBoneFPS_TrackingFPS_Async_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randIndex = UnityEngine.Random.Range(0, users.Count);
            var randBonetype = (uint)UnityEngine.Random.Range(1, 59);

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncBonesFPS.Add(users[i].Id(), new() { { randBonetype, newFPSTracking } });
            }

            // WHEN
            trackingManager.UpdateBoneFPS(users[randIndex], trackingManager.FPSTrackingTarget, randBonetype);

            // THEN
            Assert.AreEqual(users.Count, trackingManager.asyncBonesFPS.Count);
            Assert.IsTrue(trackingManager.asyncBonesFPS.Count != 0);
            Assert.IsTrue(trackingManager.asyncBonesFPS.ContainsKey(users[randIndex].Id()));
            Assert.IsTrue(trackingManager.asyncBonesFPS[users[randIndex].Id()].Count == 0);
        }

        #endregion UpdateBoneFPS

        #region SyncBoneFPS

        [Test]
        public void SyncBoneFPS_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randBonetype = (uint)UnityEngine.Random.Range(minInclusive: 1, 59);

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncBonesFPS.Add(users[i].Id(), new() { { randBonetype, newFPSTracking} });
            }

            // WHEN
            trackingManager.SyncBoneFPS(randBonetype);

            // THEN
            Assert.AreEqual(users.Count, trackingManager.asyncBonesFPS.Count);
            foreach (var item in trackingManager.asyncBonesFPS)
            {
                Assert.IsTrue(item.Value.Count == 0);
            }
        }

        [Test]
        public void SyncBoneFPS_Async_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randIndex = UnityEngine.Random.Range(0, users.Count);
            var randBonetype = (uint)UnityEngine.Random.Range(1, 59);

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncBonesFPS.Add(users[i].Id(), new() { { randBonetype, newFPSTracking } });
            }

            // WHEN
            trackingManager.SyncBoneFPS(users[randIndex], randBonetype);

            // THEN
            Assert.AreEqual(users.Count, trackingManager.asyncBonesFPS.Count);
            Assert.IsTrue(trackingManager.asyncBonesFPS.Count != 0);
            Assert.IsTrue(trackingManager.asyncBonesFPS[users[randIndex].Id()].Count == 0);
        }

        #endregion SyncBoneFPS

        #region SyncAllBones

        [Test]
        public void SyncAllBones_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randBonetype = (uint)UnityEngine.Random.Range(minInclusive: 1, 59);

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncBonesFPS.Add(users[i].Id(), new() { { randBonetype, newFPSTracking } });
            }

            // WHEN
            trackingManager.SyncAllBones();

            // THEN
            Assert.IsTrue(trackingManager.asyncBonesFPS.Count == 0);
        }

        [Test]
        public void SyncAllBones_Async_Test()
        {
            // GIVEN
            List<UMI3DCollaborationUser> users = UMI3DCollaborationServer.Collaboration.Users.ToList();

            var randIndex = UnityEngine.Random.Range(0, users.Count);
            var randBonetype = (uint)UnityEngine.Random.Range(1, 59);

            for (int i = 0; i < users.Count(); i++)
            {
                trackingManager.asyncBonesFPS.Add(users[i].Id(), new() { { randBonetype, newFPSTracking } });
            }

            // WHEN
            trackingManager.SyncAllBones(users[randIndex]);

            // THEN
            Assert.AreEqual(users.Count - 1, trackingManager.asyncBonesFPS.Count);
            Assert.IsTrue(!trackingManager.asyncBonesFPS.ContainsKey((ulong)randIndex));
        }

        #endregion SyncAllBones
    }
}
