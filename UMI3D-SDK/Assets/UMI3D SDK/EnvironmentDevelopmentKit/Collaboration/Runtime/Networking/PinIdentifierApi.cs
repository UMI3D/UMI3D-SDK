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
using UnityEngine;

namespace umi3d.edk.collaboration
{
    [CreateAssetMenu(fileName = "PinIdentifierApi", menuName = "UMI3D/Pin Identifier")]
    public class PinIdentifierApi : IdentifierApi
    {
        const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;

        public string pin = "0000";

        ///<inheritdoc/>
        public override UMI3DAuthenticator GetAuthenticator(ref AuthenticationType type)
        {
            if (type != AuthenticationType.Pin) UMI3DLogger.LogWarning($"PinIdentifierApi does not handle other AuthenticationType than PIN [ignored type : {type}]",scope);
            type = AuthenticationType.Pin;
            return new UMI3DAuthenticator(pin);
        }
    }
}