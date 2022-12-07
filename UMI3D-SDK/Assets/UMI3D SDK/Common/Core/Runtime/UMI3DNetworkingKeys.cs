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

namespace umi3d.common
{
    /// <summary>
    /// Contains all the URI required for HTTP requests within the UMI3D protocol. 
    /// </summary>
    public static class UMI3DNetworkingKeys
    {
        //users
        public const string connectionInfo = "/me";
        public const string connection_information_update = "/me/update";
        public const string status_update = "/me/status_update";
        public const string logout = "/logout";
        public const string localData = "/LocalData/key/:param";
        public const string uploadFile = "/uploadFile/:param";

        //MEDIA
        public const string root = "/";
        public const string media = "/media";
        public const string connect = "/connect";
        public const string renew_connect = "/renew_connect";

        //RESOURCES
        public const string libraries = "/libraries";
        public const string files = "/file/";
        public const string publicFiles = "/file/public/";
        public const string privateFiles = "/file/private/";
        public const string directory = "/directory/";
        public const string directory_zip = "/zip/";

        //ENVIRONMENT
        public const string register = "/register";
        public const string environment = "/environment";
        public const string join = "/environment/join";
        public const string entity = "/environment/entity";
        public const string scene = "/environment/scene";
        public const string playerCount = "/environment/player_count";

        //Prefix
        public const string websocketProtocol = "echo-protocol";
        public const string Authorization = "AUTHORIZATION";
        public const string ResourceServerAuthorization = "umi3dResourceToken";
        public const string bearer = "BEARER";
        public const string contentHeader = "FileName";

    }
}