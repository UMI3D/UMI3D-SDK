/*
Copyright 2019 Gfi Informatique

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

using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DVideoPlayer : UMI3DAbstractAnimation
    {

        //[SerializeField]
        //UMI3DMAterial material;
        [SerializeField]
        UMI3DResource videoResources;
        private UMI3DAsyncProperty<UMI3DResource> objectVideoResource;

        //public UMI3DAsyncProperty<UMI3DMaterial> ObjectMaterial;
        public UMI3DAsyncProperty<UMI3DResource> ObjectVideoResource { get { Register(); return objectVideoResource; } protected set => objectVideoResource = value; }

        protected override void InitDefinition(string id)
        {
            base.InitDefinition(id);
            //ObjectMaterial = new UMI3DAsyncProperty<UMI3DNode>(id, UMI3DPropertyKeys.AnimationNode, material, (m, u) => m?.Id());
            ObjectVideoResource = new UMI3DAsyncProperty<UMI3DResource>(id, UMI3DPropertyKeys.AnimationResource, videoResources, (r, u) => r?.ToDto());

            //ObjectMaterial.OnValueChanged += (n) => material = n;
            ObjectVideoResource.OnValueChanged += (r) => videoResources = r;
        }

        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DVideoPlayerDto;
            //Adto.materialId = ObjectMaterial.GetValue(user)?.Id();
            Adto.videoResource = ObjectVideoResource.GetValue(user)?.ToDto();
        }

        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DVideoPlayerDto();
        }
    }
}