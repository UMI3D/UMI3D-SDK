/*
Copyright 2019 - 2021 Inetum

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
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.cdk.userCapture
{

    public class TrackedSkeletonBoneController : TrackedSkeletonBone, IController
    {
        public Vector3 position
        {
            get
            {
                return this.transform.position;
            }

            set
            {
                this.transform.position = value;
            }
        }

        public Quaternion rotation
        {
            get
            {
                return this.transform.localRotation;
            }
            set
            {
                this.transform.rotation = value;
            }
        }

        public bool isActif { get; set; }

        uint IController.boneType => boneType;

        public void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }

        public override ControllerDto ToControllerDto()
        {
            var dto = base.ToControllerDto();
            dto.isOverrider = true;
            return dto;
        }
    }
}
