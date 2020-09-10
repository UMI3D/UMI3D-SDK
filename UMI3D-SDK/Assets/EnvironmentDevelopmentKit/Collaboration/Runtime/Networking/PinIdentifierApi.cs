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

using System.Collections.Generic;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    [CreateAssetMenu(fileName = "PinIdentifierApi", menuName = "UMI3D/Pin Identifier")]
    public class PinIdentifierApi : IdentifierApi
    {
        Dictionary<string, WebSocketSharp.Net.NetworkCredential> PasswordMap = new Dictionary<string, WebSocketSharp.Net.NetworkCredential>();
        public string Pin = "defaultPin";

        public override WebSocketSharp.Net.NetworkCredential GetPasswordFor(string login)
        {
            if (!PasswordMap.ContainsKey(login))
                PasswordMap[login] = new WebSocketSharp.Net.NetworkCredential(login, Pin);
            return PasswordMap[login];
        }
    }
}