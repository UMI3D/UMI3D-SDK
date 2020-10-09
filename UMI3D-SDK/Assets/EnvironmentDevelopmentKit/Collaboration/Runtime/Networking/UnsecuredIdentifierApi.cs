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
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    [CreateAssetMenu(fileName = "UnsecuredIdentifierApi", menuName = "UMI3D/Unsecured Identifier")]
    public class UnsecuredIdentifierApi : IdentifierApi
    {
        public Dictionary<string, WebSocketSharp.Net.NetworkCredential> PasswordMap = new Dictionary<string, WebSocketSharp.Net.NetworkCredential>();
        public Dictionary<string, FormDto> idMap = new Dictionary<string, FormDto>();

        public override FormDto GetParameterDtosFor(string login)
        {
            if (!idMap.ContainsKey(login) || idMap[login] == null)
            {
                idMap[login] = new FormDto();
                idMap[login].fields = new List<AbstractParameterDto>();
                StringParameterDto username = new StringParameterDto()
                {
                    name = "username",
                    value = "",
                };
                idMap[login].fields.Add(username);
            }

            return idMap[login];
        }

        public override WebSocketSharp.Net.NetworkCredential GetPasswordFor(string login)
        {
            if (PasswordMap.ContainsKey(login))
                return PasswordMap[login];
            Debug.Log("no pw found for " + login);
            return null;
        }

        public override StatusType UpdateIdentity(UMI3DCollaborationUser user, UserConnectionDto identity)
        {
            idMap[user.login] = identity.parameters;
            return (idMap[user.login] != null && (idMap[user.login].fields[0] is StringParameterDto) && (idMap[user.login].fields[0] as StringParameterDto).value != "") ? StatusType.READY : StatusType.CREATED;
        }
    }
}