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

using System;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Implementation of ClientIdentifier returning a pre-filled pin
    /// </summary>
    /// <see cref="ClientIdentifierApi"/>
    [CreateAssetMenu(fileName = "ClientPinIdentifier", menuName = "UMI3D/Client Pin Identifier")]
    public class PinClientIdentifierApi : ClientIdentifierApi
    {
        public string Pin = "defaultPin";

        ///<inheritdoc/>
        public override void GetIdentity(Action<UMI3DAuthenticator> callback)
        {
            callback?.Invoke(new common.collaboration.UMI3DAuthenticator(GetPin, GetLoginPassword, GetIdentity));
        }

        private void GetPin(Action<string> callback)
        {
            callback?.Invoke(Pin);
        }

        private void GetLoginPassword(Action<(string, string)> callback)
        {
            callback?.Invoke((null, Pin));
        }

        private void GetIdentity(Action<IdentityDto> callback)
        {
            callback?.Invoke(UMI3DCollaborationClientServer.Identity);
        }

    }
}