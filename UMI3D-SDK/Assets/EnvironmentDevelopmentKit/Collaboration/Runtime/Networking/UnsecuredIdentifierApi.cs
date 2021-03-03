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
    [CreateAssetMenu(fileName = "UnsecuredIdentifierApi", menuName = "UMI3D/Unsecured Identifier")]
    public class UnsecuredIdentifierApi : IdentifierApi
    {
        ///<inheritdoc/>
        public override UMI3DAuthenticator GetAuthenticator(ref AuthenticationType type)
        {
            if (type != AuthenticationType.None) Debug.LogWarning($"UnsecuredIdentifierApi does not handle other AuthenticationType than None [ignored type : {type}]");
            type = AuthenticationType.None;
            return new UMI3DAuthenticator();
        }
    }
}
