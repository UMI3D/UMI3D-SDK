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
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.common.binding;
using umi3d.common.dto.binding;
using UnityEngine;
using UnityEngine.Events;

namespace EditMode_Tests.Core.Binding.CDK
{
    public class BindingManager_Test
    {
        private BindingManager bindingManager => mockBindingManager.Object;

        private Mock<BindingManager> mockBindingManager;

        private Mock<IUMI3DClientServer> mockClientServer;

        private Mock<ILateRoutineService> mockLateRoutineService;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (BindingManager.Exists)
                BindingManager.Destroy();
        }

        [SetUp]
        public void SetUp()
        {
            mockLateRoutineService = new Mock<ILateRoutineService>();
            mockClientServer = new Mock<IUMI3DClientServer>();
            mockBindingManager = new Mock<BindingManager>(mockLateRoutineService.Object, mockClientServer.Object);
            mockBindingManager.CallBase = true;
        }

        [TearDown]
        public void TearDown()
        {
            mockBindingManager.Reset();

            if (BindingManager.Exists)
                BindingManager.Destroy();
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
            var nodeBindingDto = new NodeBindingDataDto() { parentNodeId = 1005uL, priority = 10 };
            var binding = new NodeBinding(nodeBindingDto, null, null);

            var initialSize = bindingManager.Bindings.Count;
            mockLateRoutineService.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false));

            // WHEN
            bindingManager.AddBinding(1005uL, binding);

            // THEN
            mockLateRoutineService.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false), Times.Never());
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
            var nodeBindingDto = new NodeBindingDataDto() { parentNodeId = 1005uL, priority = 10 };
            var binding = new NodeBinding(nodeBindingDto, null, null);
            var initialSize = bindingManager.Bindings.Count;
            mockLateRoutineService.Setup(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false));
            mockClientServer.Setup(x => x.OnLeavingEnvironment).Returns(new UnityEvent());

            // WHEN
            bindingManager.AddBinding(1005uL, binding);

            // THEN
            mockLateRoutineService.Verify(x => x.AttachLateRoutine(It.IsAny<IEnumerator>(), false), Times.Once());
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
            mockLateRoutineService.Verify(x => x.DetachLateRoutine(It.IsAny<IEnumerator>()), Times.Never());
            Assert.AreEqual(initialSize, bindingManager.Bindings.Count);
        }

        [Test]
        [TestCase(10005uL)]
        [TestCase(0uL)]
        public void RemoveBinding_Registered(ulong boundNodeId)
        {
            NodeBindingDataDto dto = new();
            NodeBinding nodeBinding = new NodeBinding(dto, null, null);

            mockBindingManager.CallBase = false;
            mockBindingManager.Setup(x => x.AddBinding(It.IsAny<ulong>(), It.IsAny<AbstractBinding>())).CallBase();
            mockBindingManager.Setup(x => x.RemoveBinding(It.IsAny<ulong>())).CallBase();
            mockBindingManager.Setup(x => x.Bindings).CallBase();

            bindingManager.AddBinding(boundNodeId, nodeBinding);

            // GIVEN
            int initialSize = bindingManager.Bindings.Count;
            mockLateRoutineService.Setup(x => x.DetachLateRoutine(It.IsAny<IEnumerator>()));

            // WHEN
            bindingManager.RemoveBinding(boundNodeId);

            // THEN
            Assert.AreEqual(initialSize - 1, bindingManager.Bindings.Count);
        }

        #endregion RemoveBinding

        #region BindingRoutine

        private static (ulong id, int priority, bool partialFit)[][] values = new (ulong id, int priority, bool partialFit)[][]
        {
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 10, true), (1009uL, 5, true), (1010uL, 1, true)  },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 10, true), (1009uL, 5, true), (1010uL, 1, false) },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true),  (1009uL, 1, true), (1010uL, 1, true)  },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true),  (1009uL, 1, true), (1010uL, 1, false) },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true), (1009uL, 5, true), (1010uL, 10, true)  },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true), (1009uL, 5, true), (1010uL, 10, false) },
        };

        [Test]
        public void BindingRoutine([ValueSource("values")] (ulong id, int priority, bool partialFit)[] testValue)
        {
            // GIVEN
            Queue<ulong> executionTracker = new();
            mockBindingManager.CallBase = false;
            mockBindingManager.Setup(x => x.Bindings).CallBase();
            mockBindingManager.Setup(x => x.BindingApplicationRoutine()).CallBase();
            mockBindingManager.Setup(x => x.AreBindingsActivated).Returns(true);
            mockBindingManager.Setup(x => x.AddBinding(It.IsAny<ulong>(), It.IsAny<AbstractBinding>())).CallBase();
            mockClientServer.Setup(x => x.OnLeavingEnvironment).Returns(new UnityEvent());

            Queue<Mock<NodeBinding>> mockNodeBindings = new Queue<Mock<NodeBinding>>();
            foreach (var v in testValue)
            {
                var mockNodeBinding = new Mock<NodeBinding>(null, null, null);
                mockNodeBinding.Setup(x=> x.ParentNodeId).Returns(v.id);
                mockNodeBinding.Setup(x=> x.Priority).Returns(v.priority);
                mockNodeBinding.Setup(x=> x.IsPartiallyFit).Returns(v.partialFit);
                
                bool success = true;
                mockNodeBinding.Setup(x => x.Apply(out success)).Callback(() => { executionTracker.Enqueue(v.id); success = true; });
                mockNodeBindings.Enqueue(mockNodeBinding);
                bindingManager.AddBinding(v.id, mockNodeBinding.Object);
            }

            // WHEN
            bindingManager.BindingApplicationRoutine().MoveNext();

            // THEN
            var orderedtestValues = testValue.OrderByDescending(x => x.priority).ToArray();

            Assert.AreEqual(orderedtestValues.Length, executionTracker.Count, "Not all bindings were applied.");
            var callbackCache = executionTracker.ToArray();
            for (int j = 0; j < orderedtestValues.Length; j++)
            {
                Assert.AreEqual(orderedtestValues[0].id, callbackCache[0], $"Order of binding application is not right. Problem at {j}");
            }
        }

        #endregion
    }
}