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
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.common;
using umi3d.common.binding;

namespace EditMode_Tests.Core.Binding.CDK
{
    public class BindingLoader_Test
    {
        protected BindingLoader bindingLoader;

        protected Mock<IBindingBrowserService> bindingManagementServiceMock;
        protected Mock<UMI3DEnvironmentLoader> environmentLoaderServiceMock;

        #region Test SetUp

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            ClearSingletons();
        }

        [SetUp]
        public virtual void SetUp()
        {
            bindingManagementServiceMock = new();
            environmentLoaderServiceMock = new();
            bindingLoader = new BindingLoader(bindingManagementServiceMock.Object, environmentLoaderServiceMock.Object);
        }

        [TearDown]
        public virtual void TearDown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();

            if (BindingManager.Exists)
                BindingManager.Destroy();
        }

        #endregion Test SetUp

        #region CanReadUMI3DExtension

        [Test]
        public void CanReadUMI3DExtension()
        {
            // GIVEN
            var dto = new BindingDto();
            var extensionData = new ReadUMI3DExtensionData(dto);

            // WHEN
            var canReadResult = bindingLoader.CanReadUMI3DExtension(extensionData);

            // THEN
            Assert.IsTrue(canReadResult);
        }

        [Test]
        public void CanReadUMI3DExtension_InvalidDto()
        {
            // GIVEN
            var dto = new UMI3DDto();
            var extensionData = new ReadUMI3DExtensionData(dto);

            // WHEN
            var canReadResult = bindingLoader.CanReadUMI3DExtension(extensionData);

            // THEN
            Assert.IsFalse(canReadResult);
        }

        #endregion CanReadUMI3DExtension

        #region ReadUMI3DExtension

        [Test]
        public virtual async void ReadUMI3DExtension_NodeBinding()
        {
            // GIVEN
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new NodeBindingDataDto() { parentNodeId = 1008uL }
            };

            var extensionData = new ReadUMI3DExtensionData(dto);

            var entityFake = new UMI3DEntityInstance(() => { }); 
            var nodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            nodeMock.Setup(x => x.transform).Returns(default(UnityEngine.Transform));

            environmentLoaderServiceMock.Setup(x => x.WaitUntilEntityLoaded(dto.id, null)).Returns(Task.FromResult(entityFake));
            environmentLoaderServiceMock.Setup(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(dto.boundNodeId)).Returns(nodeMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));
           
            // WHEN
            await bindingLoader.ReadUMI3DExtension(extensionData);

            // THEN
            environmentLoaderServiceMock.Verify(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));
        }

        [Test]
        public virtual async void ReadUMI3DExtension_MultiBinding()
        {
            // GIVEN
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new MultiBindingDataDto() { Bindings = new AbstractSimpleBindingDataDto[] { new NodeBindingDataDto() { parentNodeId = 1008uL } } }
            };

            var extensionData = new ReadUMI3DExtensionData(dto);

            var entityFake = new UMI3DEntityInstance(() => { });
            var nodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            nodeMock.Setup(x => x.transform).Returns(default(UnityEngine.Transform));

            environmentLoaderServiceMock.Setup(x => x.WaitUntilEntityLoaded(dto.id, null)).Returns(Task.FromResult(entityFake));
            environmentLoaderServiceMock.Setup(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(dto.boundNodeId)).Returns(nodeMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // WHEN
            await bindingLoader.ReadUMI3DExtension(extensionData);

            // THEN
            environmentLoaderServiceMock.Verify(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));
        }

        #endregion ReadUMI3DExtension
    }
}