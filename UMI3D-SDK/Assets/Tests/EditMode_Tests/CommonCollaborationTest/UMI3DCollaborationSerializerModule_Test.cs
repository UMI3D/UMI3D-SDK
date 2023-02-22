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
using umi3d.common.collaboration;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace EditMode_Tests
{
    public class UMI3DCollaborationSerializerModule_Test
    {
        umi3d.common.collaboration.UMI3DCollaborationSerializerModule collabSerializerModule = null;
        UMI3DSerializerBasicModules basicModules = null;
        UMI3DSerializerVectorModules vectorModules = null;
        UMI3DSerializerStringModules stringModules = null;
        UMI3DSerializerShaderModules shaderModules = null;
        UMI3DSerializerAnimationModules animationModules = null;

        [OneTimeSetUp] public void InitSerializer()
        {
            collabSerializerModule = new umi3d.common.collaboration.UMI3DCollaborationSerializerModule();
            basicModules = new UMI3DSerializerBasicModules();
            vectorModules= new UMI3DSerializerVectorModules();
            stringModules = new UMI3DSerializerStringModules();
            shaderModules= new UMI3DSerializerShaderModules();
            animationModules= new UMI3DSerializerAnimationModules();

            UMI3DSerializer.AddModule(collabSerializerModule);
            UMI3DSerializer.AddModule(basicModules);
            UMI3DSerializer.AddModule(vectorModules);
            UMI3DSerializer.AddModule(stringModules);
            UMI3DSerializer.AddModule(shaderModules);
            UMI3DSerializer.AddModule(animationModules);
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

            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes()) ;

            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue(result.active == bindingDto.active);
            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);
        }

        [Test]
        public void ReadBindingDTO_SimpleBindingDto()
        {
            SimpleBindingDto simpleBindingDto = new SimpleBindingDto(
                priority: 10,
                partialFit: true,
                syncRotation : true,
                syncPosition : true,
                syncScale : true,
                offSetPosition : Vector3.one,
                offSetRotation : Vector4.one,
                offSetScale : Vector3.left
            ) ;

            BindingDto bindingDto = new BindingDto(
                objectId: 123,
                active: true,
                data: simpleBindingDto
            );

            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue(result.active == bindingDto.active);
            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

            Assert.IsTrue((result.data as SimpleBindingDto).offSetRotation
                == (bindingDto.data as SimpleBindingDto).offSetRotation);
            Assert.IsTrue((result.data as SimpleBindingDto).offSetPosition
                == (bindingDto.data as SimpleBindingDto).offSetPosition);
            Assert.IsTrue((result.data as SimpleBindingDto).offSetScale
                == (bindingDto.data as SimpleBindingDto).offSetScale);
            Assert.IsTrue((result.data as SimpleBindingDto).syncScale
                == (bindingDto.data as SimpleBindingDto).syncScale);
            Assert.IsTrue((result.data as SimpleBindingDto).syncPosition
                == (bindingDto.data as SimpleBindingDto).syncPosition);
            Assert.IsTrue((result.data as SimpleBindingDto).syncRotation
                == (bindingDto.data as SimpleBindingDto).syncRotation);
        }

        [Test]
        public void ReadBindingDTO_SimpleBoneBindingDTO()
        {
            SimpleBoneBindingDto simpleBoneBindingDto = new SimpleBoneBindingDto(
                priority: 10,
                partialFit: true,
                syncRotation: true,
                syncPosition: true,
                syncScale: true,
                offSetPosition: Vector3.one,
                offSetRotation: Vector4.one,
                offSetScale: Vector3.left,
                userId : 1,
                boneType : 15            
            );

            BindingDto bindingDto = new BindingDto(
                objectId: 123,
                active: true,
                data: simpleBoneBindingDto
            );

            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue(result.active == bindingDto.active);
            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

            Assert.IsTrue((result.data as SimpleBindingDto).offSetRotation
                == (bindingDto.data as SimpleBindingDto).offSetRotation);
            Assert.IsTrue((result.data as SimpleBindingDto).offSetPosition
                == (bindingDto.data as SimpleBindingDto).offSetPosition);
            Assert.IsTrue((result.data as SimpleBindingDto).offSetScale
                == (bindingDto.data as SimpleBindingDto).offSetScale);
            Assert.IsTrue((result.data as SimpleBindingDto).syncScale
                == (bindingDto.data as SimpleBindingDto).syncScale);
            Assert.IsTrue((result.data as SimpleBindingDto).syncPosition
                == (bindingDto.data as SimpleBindingDto).syncPosition);
            Assert.IsTrue((result.data as SimpleBindingDto).syncRotation
                == (bindingDto.data as SimpleBindingDto).syncRotation);

            Assert.IsTrue((result.data as SimpleBoneBindingDto).userId
                == (bindingDto.data as SimpleBoneBindingDto).userId);
            Assert.IsTrue((result.data as SimpleBoneBindingDto).boneType
                == (bindingDto.data as SimpleBoneBindingDto).boneType);
        }

        [Test]
        public void ReadBindingDTO_RigBindingDataDto()
        {
            RigBindingDataDto rigBindingDataDto = new RigBindingDataDto(
                priority: 10,
                partialFit: true,
                syncRotation: true,
                syncPosition: true,
                syncScale: true,
                offSetPosition: Vector3.one,
                offSetRotation: Vector4.one,
                offSetScale: Vector3.left,
                userId: 1,
                boneType: 15,
                rigName : "Platipus"
            );

            BindingDto bindingDto = new BindingDto(
                objectId: 123,
                active: true,
                data: rigBindingDataDto
            );

            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue(result.active == bindingDto.active);
            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

            Assert.IsTrue((result.data as SimpleBindingDto).offSetRotation
                == (bindingDto.data as SimpleBindingDto).offSetRotation);
            Assert.IsTrue((result.data as SimpleBindingDto).offSetPosition
                == (bindingDto.data as SimpleBindingDto).offSetPosition);
            Assert.IsTrue((result.data as SimpleBindingDto).offSetScale
                == (bindingDto.data as SimpleBindingDto).offSetScale);
            Assert.IsTrue((result.data as SimpleBindingDto).syncScale
                == (bindingDto.data as SimpleBindingDto).syncScale);
            Assert.IsTrue((result.data as SimpleBindingDto).syncPosition
                == (bindingDto.data as SimpleBindingDto).syncPosition);
            Assert.IsTrue((result.data as SimpleBindingDto).syncRotation
                == (bindingDto.data as SimpleBindingDto).syncRotation);

            Assert.IsTrue((result.data as SimpleBoneBindingDto).userId
                == (bindingDto.data as SimpleBoneBindingDto).userId);
            Assert.IsTrue((result.data as SimpleBoneBindingDto).boneType
                == (bindingDto.data as SimpleBoneBindingDto).boneType);

            Assert.IsTrue((result.data as RigBindingDataDto).rigName
                 == (bindingDto.data as RigBindingDataDto).rigName);
        }
        #endregion
    }
}

