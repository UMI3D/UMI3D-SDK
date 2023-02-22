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
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;

namespace EditMode_Tests
{
    public class UMI3DCollaborationSerializerModule_Test
    {
        UMI3DCollaborationSerializerModule serializerModule = null;

        [SetUp] public void Setup() 
        {
            serializerModule = new UMI3DCollaborationSerializerModule();
        }

        [TearDown] public void Teardown()
        {

        }

        #region Bindings
        [Test] public void ReadBindingDTO_BindingDTOData()
        {
            BindingDataDto bindingDataDto = new BindingDataDto(
                priority : 10,
                partialFit : true
            );

            BindingDto bindingDto = new BindingDto(
                objectId : 123,
                active : true,
                data : bindingDataDto
            );

            serializerModule.Write<BindingDto>(bindingDto, out Bytable data);

            Assert.IsTrue(data != null);
            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes()) ;

            serializerModule.Read(byteContainer, out bool readable, out BindingDto result);
            Assert.IsTrue(readable);

        }
        #endregion
    }
}

