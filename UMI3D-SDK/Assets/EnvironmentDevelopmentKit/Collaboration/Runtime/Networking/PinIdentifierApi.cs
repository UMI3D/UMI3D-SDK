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

using BeardedManStudios.Forge.Networking;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    [CreateAssetMenu(fileName = "PinIdentifierApi", menuName = "UMI3D/Pin Identifier")]
    public class PinIdentifierApi : IdentifierApi
    {
        Dictionary<string, WebSocketSharp.Net.NetworkCredential> PasswordMap = new Dictionary<string, WebSocketSharp.Net.NetworkCredential>();
        [SerializeField, EditorReadOnly]
        string _pin = "0000";
        /// <summary>
        /// Pin used to connect to this server.
        /// </summary>
        public string Pin
        {
            get {
                if (Authenticator == null) Authenticator = new PinAuthenticator(_pin);
                return Authenticator.pin; }
            set {
                _pin = value;
                if (Authenticator == null) Authenticator = new PinAuthenticator(value);
                Authenticator.pin = value;
            }
        }
        PinAuthenticator Authenticator;

        ///<inheritdoc/>
        public override IUserAuthenticator GetAuthenticator()
        {
            if (Authenticator == null) 
                Authenticator = new PinAuthenticator(_pin);
            return Authenticator;
        }
    }
}