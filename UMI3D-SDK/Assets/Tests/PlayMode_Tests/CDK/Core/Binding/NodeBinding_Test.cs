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
using System.Collections;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.common.binding;
using umi3d.common.dto.binding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlayMode_Tests.Core.Binding.CDK
{
    public class NodeBinding_Test
    {
        protected GameObject parentGo;
        protected GameObject go;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            parentGo = new GameObject("Parent Node");
            Object.Instantiate(parentGo);
            parentGo.transform.position = Vector3.zero;
            parentGo.transform.rotation = Quaternion.identity;
            parentGo.transform.localScale = Vector3.one;

            go = new GameObject("Bound Node");
            Object.Instantiate(go);
            go.transform.position = Vector3.one;
            go.transform.rotation = Quaternion.Euler(90, 90, 90);
            go.transform.localScale = Vector3.one * 0.5f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(parentGo);
            Object.Destroy(go);
        }

        #endregion Test SetUp

        #region Apply

        private static Vector3[] positionOffsets = new Vector3[] { Vector3.zero, Vector3.one, new Vector3(1, -1, 25) };
        private static Quaternion[] rotationOffsets = new Quaternion[] { Quaternion.identity, Quaternion.Euler(10, 10, 10), Quaternion.Euler(-10, 10, -10) };
        private static Vector3[] scaleOffsets = new Vector3[] { Vector3.zero, Vector3.one, new Vector3(1, -1, 25) };

        #region Position
        [Test]
        public void Apply_SyncPosition([ValueSource("positionOffsets")] Vector3 offSetPosition)
        {
            // GIVEN
            var dto = new NodeBindingDataDto()
            {
                parentNodeId = 1008uL,
                syncPosition = true,
                offSetPosition = offSetPosition.Dto()
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            parentNodeMock.Setup(x => x.transform).Returns(parentGo.transform);

            NodeBinding binding = new(dto, go.transform, parentNodeMock.Object);

            var previousRotation = go.transform.rotation;
            var previousScale = go.transform.localScale;

            // WHEN
            binding.Apply(out bool succes);

            // THEN
            Assert.IsTrue(succes);
            Assert.AreEqual(parentGo.transform.position + offSetPosition, go.transform.position);
            Assert.AreEqual(previousRotation, go.transform.rotation);
            Assert.AreEqual(previousScale, go.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Apply_SyncPosition_SeveralFrames([ValueSource("positionOffsets")] Vector3 offSetPosition)
        {
            // GIVEN
            var dto = new NodeBindingDataDto()
            {
                parentNodeId = 1008uL,
                syncPosition = true,
                offSetPosition = offSetPosition.Dto()
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            parentNodeMock.Setup(x => x.transform).Returns(parentGo.transform);

            NodeBinding binding = new(dto, go.transform, parentNodeMock.Object);

            var previousRotation = go.transform.rotation;
            var previousScale = go.transform.localScale;

            // WHEN
            int numberOfFrames = 10;

            for (int i = 0; i < numberOfFrames; i++)
            {
                parentGo.transform.Translate(1, 1, 1);
                parentGo.transform.Rotate(1, 1, 1);

                binding.Apply(out bool succes);

                // THEN
                Assert.IsTrue(succes);
                Assert.AreEqual(parentGo.transform.position + offSetPosition, go.transform.position);
                Assert.AreEqual(previousRotation, go.transform.rotation);
                Assert.AreEqual(previousScale, go.transform.localScale);

                yield return null;
            }
        }

        #endregion

        #region Rotation

        [Test]
        public void Apply_SyncRotation([ValueSource("rotationOffsets")] Quaternion offsetRotation)
        {
            // GIVEN

            var dto = new NodeBindingDataDto()
            {
                parentNodeId = 1008uL,
                syncRotation = true,
                offSetRotation = offsetRotation.Dto()
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            parentNodeMock.Setup(x => x.transform).Returns(parentGo.transform);

            NodeBinding binding = new(dto, go.transform, parentNodeMock.Object);

            var previousPosition = go.transform.position;
            var previousRotation = go.transform.rotation;
            var previousScale = go.transform.localScale;

            // WHEN
            binding.Apply(out bool success);

            // THEN
            Assert.IsTrue(success);
            Assert.AreEqual(previousPosition, go.transform.position);
            Assert.IsTrue(parentGo.transform.rotation * offsetRotation == go.transform.rotation);
            Assert.AreEqual(previousScale, go.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Apply_SyncRotation_SeveralFrames([ValueSource("rotationOffsets")] Quaternion offsetRotation)
        {
            // GIVEN

            var dto = new NodeBindingDataDto()
            {
                parentNodeId = 1008uL,
                syncRotation = true,
                offSetRotation = offsetRotation.Dto()
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            parentNodeMock.Setup(x => x.transform).Returns(parentGo.transform);

            NodeBinding binding = new(dto, go.transform, parentNodeMock.Object);

            var previousPosition = go.transform.position;
            var previousRotation = go.transform.rotation;
            var previousScale = go.transform.localScale;

            // WHEN

            int numberOfFrames = 10;

            for (int i = 0; i < numberOfFrames; i++)
            {
                parentGo.transform.Translate(1, 1, 1);
                parentGo.transform.Rotate(1, 1, 1);

                binding.Apply(out bool success);

                // THEN
                Assert.IsTrue(success);
                Assert.AreEqual(previousPosition, go.transform.position);
                Assert.IsTrue(parentGo.transform.rotation * offsetRotation == go.transform.rotation);
                Assert.AreEqual(previousScale, go.transform.localScale);

                yield return null;
            }
        }

        #endregion

        #region Scale

        [Test]
        public void Apply_SyncScale([ValueSource("scaleOffsets")] Vector3 offSetScale)
        {
            // GIVEN
            var dto = new NodeBindingDataDto()
            {
                parentNodeId = 1008uL,
                syncScale = true,
                offSetScale = offSetScale.Dto()
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            parentNodeMock.Setup(x => x.transform).Returns(parentGo.transform);

            NodeBinding binding = new(dto, go.transform, parentNodeMock.Object);

            var previousPosition = go.transform.position;
            var previousRotation = go.transform.rotation;
            var previousScale = go.transform.localScale;

            // WHEN
            binding.Apply(out bool succes);

            // THEN
            Assert.IsTrue(succes);
            Assert.AreEqual(previousPosition, go.transform.position);
            Assert.AreEqual(previousRotation, go.transform.rotation);
            Assert.AreEqual(Vector3.Scale(parentGo.transform.localScale, go.transform.localScale), go.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Apply_SyncScale_SeveralFrames([ValueSource("scaleOffsets")] Vector3 offSetScale)
        {
            // GIVEN
            var dto = new NodeBindingDataDto()
            {
                parentNodeId = 1008uL,
                syncScale = true,
                offSetPosition = offSetScale.Dto()
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            parentNodeMock.Setup(x => x.transform).Returns(parentGo.transform);

            NodeBinding binding = new(dto, go.transform, parentNodeMock.Object);

            var previousPosition = go.transform.position;
            var previousRotation = go.transform.rotation;
            var previousScale = go.transform.localScale;

            // WHEN
            int numberOfFrames = 10;

            for (int i = 0; i < numberOfFrames; i++)
            {
                parentGo.transform.Translate(1, 1, 1);
                parentGo.transform.Rotate(1, 1, 1);

                binding.Apply(out bool succes);

                // THEN
                Assert.IsTrue(succes);
                Assert.AreEqual(previousPosition, go.transform.position);
                Assert.AreEqual(previousRotation, go.transform.rotation);
                Assert.AreEqual(Vector3.Scale(parentGo.transform.localScale, go.transform.localScale), go.transform.localScale);

                yield return null;
            }
        }
        #endregion Scale

        #endregion Apply
    }
}