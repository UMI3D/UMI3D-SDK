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

using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using umi3d.edk.interaction;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    [CreateAssetMenu(fileName = "PinIdentifierApi", menuName = "UMI3D/Pin Identifier")]
    public class PinIdentifierWithFormApi : PinIdentifierApi
    {
        public UMI3DForm form;

        public override FormDto GetParameterDtosFor(UMI3DCollaborationUser user)
        {
            return form.ToDto(user) as FormDto;
        }

        public override StatusType UpdateIdentity(UMI3DCollaborationUser user, UserConnectionDto identity)
        {
            return base.UpdateIdentity(user, identity);
        }
    }
}