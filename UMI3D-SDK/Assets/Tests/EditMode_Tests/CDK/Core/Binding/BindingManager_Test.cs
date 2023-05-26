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

using inetum.unityUtils;
using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.common;
using UnityEngine;

namespace EditMode_Tests.Core.Bindings.CDK
{
    public class BindingManager_Test
    {
        private BindingManager bindingManager => mockBindingManager.Object;

        private Mock<BindingManager> mockBindingManager;

        private Mock<UMI3DEnvironmentLoader> mockEnvironmentLoader;

        private Mock<CoroutineManager> mockCoroutineService;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (BindingManager.Exists)
                BindingManager.Destroy();

            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();

            if (CoroutineManager.Exists)
                CoroutineManager.Destroy();
        }

        [SetUp]
        public void SetUp()
        {
            mockCoroutineService = new Mock<CoroutineManager>();
            mockEnvironmentLoader = new Mock<UMI3DEnvironmentLoader>();
            mockBindingManager = new Mock<BindingManager>(mockCoroutineService.Object, mockEnvironmentLoader.Object);
            mockBindingManager.CallBase = true;
        }

        [TearDown]
        public void TearDown()
        {
            mockCoroutineService.Reset();
            mockEnvironmentLoader.Reset();
            mockBindingManager.Reset();

            if (BindingManager.Exists)
                BindingManager.Destroy();

            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();

            if (CoroutineManager.Exists)
                CoroutineManager.Destroy();
        }

        #region UpdateBindingActivation

        [TestCase(true)]
        [TestCase(false)]
        [Test]
        public void UpdateBindingActivation(bool setValue)
        {
            // GIVEN
            // defined in parameters

            // WHEN
            bindingManager.UpdateBindingsActivation(setValue);

            // THEN
            Assert.AreEqual(setValue, bindingManager.AreBindingsActivated);
        }

        #endregion UpdateBindingActivation

        #region AddBinding

        [Test]
        public void AddBinding_CannotLoad()
        {
            // GIVEN
            var initialSize = bindingManager.Bindings.Count;

            // WHEN
            bindingManager.AddBinding(1005uL, null);

            // THEN
            Assert.AreEqual(initialSize, bindingManager.Bindings.Count);
        }

        [Test]
        public void AddBinding_BindingsDisabled()
        {
            mockBindingManager.CallBase = false;
            mockBindingManager.Setup(x => x.Bindings).CallBase();
            mockBindingManager.Setup(x => x.AreBindingsActivated).Returns(false);
            mockBindingManager.Setup(x => x.AddBinding(It.IsAny<ulong>(), It.IsAny<AbstractBinding>())).CallBase();

            // GIVEN
            var binding = new NodeBinding(null, null, null);

            var initialSize = bindingManager.Bindings.Count;
            mockCoroutineService.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>()));

            // WHEN
            bindingManager.AddBinding(1005uL, binding);

            // THEN
            mockCoroutineService.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>()), Times.Never());
            Assert.AreEqual(initialSize + 1, bindingManager.Bindings.Count);
        }

        [Test]
        public void AddBinding_BindingsEnabled()
        {
            mockBindingManager.CallBase = false;
            mockBindingManager.Setup(x => x.Bindings).CallBase();
            mockBindingManager.Setup(x => x.AreBindingsActivated).Returns(true);
            mockBindingManager.Setup(x => x.AddBinding(It.IsAny<ulong>(), It.IsAny<AbstractBinding>())).CallBase();

            // GIVEN
            var binding = new NodeBinding(null, null, null);
            var initialSize = bindingManager.Bindings.Count;
            mockCoroutineService.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>()));

            // WHEN
            bindingManager.AddBinding(1005uL, binding);

            // THEN
            mockCoroutineService.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>()), Times.Once());
            Assert.AreEqual(initialSize + 1, bindingManager.Bindings.Count);
        }

        #endregion AddBinding

        #region RemoveBinding

        [Test]
        [TestCase(10005uL)]
        [TestCase(0uL)]
        public void RemoveBinding_NotRegistered(ulong boundNodeId)
        {
            Dictionary<ulong, AbstractBinding> mockBindingsDict = new() { { boundNodeId, new NodeBinding(null, null, null) } };

            mockBindingManager.CallBase = false;
            mockBindingManager.Setup(x => x.RemoveBinding(It.IsAny<ulong>())).CallBase();
            mockBindingManager.Setup(x => x.Bindings).Returns(mockBindingsDict);

            // GIVEN
            var otherBoundNodeId = boundNodeId + 5;
            var initialSize = mockBindingsDict.Count;

            // WHEN
            bindingManager.RemoveBinding(otherBoundNodeId);

            // THEN
            mockCoroutineService.Verify(x => x.DettachCoroutine(It.IsAny<Coroutine>()), Times.Never());
            Assert.AreEqual(initialSize, bindingManager.Bindings.Count);
        }

        [Test]
        [TestCase(10005uL)]
        [TestCase(0uL)]
        public void RemoveBinding_Registered(ulong boundNodeId)
        {
            Dictionary<ulong, AbstractBinding> mockBindingsDict = new() { { boundNodeId, new NodeBinding(null, null, null) } };

            mockBindingManager.CallBase = false;
            mockBindingManager.Setup(x => x.RemoveBinding(It.IsAny<ulong>())).CallBase();
            mockBindingManager.Setup(x => x.Bindings).Returns(mockBindingsDict);

            // GIVEN
            int initialSize = bindingManager.Bindings.Count;
            mockCoroutineService.Setup(x => x.DettachCoroutine(It.IsAny<Coroutine>()));

            // WHEN
            bindingManager.RemoveBinding(boundNodeId);

            // THEN
            Assert.AreEqual(initialSize - 1, bindingManager.Bindings.Count);
        }

        #endregion RemoveBinding
    }
}