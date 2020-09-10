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

using UnityEngine;

namespace umi3d.common
{
    static public class AuthenticationTypeConverter
    {

        static public AuthenticationType Convert(this WebSocketSharp.Net.AuthenticationSchemes Scheme)
        {
            switch (Scheme)
            {
                case WebSocketSharp.Net.AuthenticationSchemes.Digest:
                    return AuthenticationType.Digest;
                case WebSocketSharp.Net.AuthenticationSchemes.Basic:
                    return AuthenticationType.Basic;
                case WebSocketSharp.Net.AuthenticationSchemes.Anonymous:
                    return AuthenticationType.Anonymous;
            }
            return AuthenticationType.Anonymous;
        }

        public static WebSocketSharp.Net.AuthenticationSchemes Convert(this AuthenticationType Scheme)
        {
            switch (Scheme)
            {
                case AuthenticationType.Digest:
                    return WebSocketSharp.Net.AuthenticationSchemes.Digest;
                case AuthenticationType.Basic:
                    return WebSocketSharp.Net.AuthenticationSchemes.Basic;
                case AuthenticationType.Anonymous:
                    return WebSocketSharp.Net.AuthenticationSchemes.Anonymous;
            }
            return WebSocketSharp.Net.AuthenticationSchemes.None;
        }
    }

}