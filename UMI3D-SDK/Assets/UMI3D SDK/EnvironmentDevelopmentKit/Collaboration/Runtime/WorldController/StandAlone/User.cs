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

using System.Collections.Generic;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;

namespace umi3d.worldController
{
    /// <summary>
    /// User representation for a world controller.
    /// </summary>
    public class User
    {
        public bool LoadLibraryOnly;
        public GateDto gate;
        private readonly PrivateIdentityDto privateIdentity;
        public string Token => privateIdentity.globalToken;

        public string localToken => privateIdentity.localToken;
        public string headearToken => privateIdentity.headerToken;
        public string key => privateIdentity.key;

        public bool isServer => privateIdentity.isServer;

        public string globalToken;
        private static readonly Dictionary<ulong, User> ids = new Dictionary<ulong, User>();

        public User(ConnectionDto connectionDto, string globalToken)
        {
            this.globalToken = globalToken;
            privateIdentity = new PrivateIdentityDto
            {
                guid = NewID(),
                isServer = connectionDto.isServer
            };
            Update(connectionDto);
        }

        #region id
        private readonly System.Random random = new System.Random();
        private string NewID()
        {
            return System.Guid.NewGuid().ToString();
        }
        #endregion

        #region IdentityDto
        public PublicIdentityDto PublicIdentityDto()
        {
            var id = new PublicIdentityDto();
            SetPublicIdentity(id);
            return id;
        }

        public IdentityDto IdentityDto()
        {
            var id = new IdentityDto();
            SetIdentity(id);
            return id;
        }

        public RegisterIdentityDto RegisterIdentityDto()
        {
            var id = new RegisterIdentityDto();
            SetRegisterIdentity(id);
            return id;
        }

        public PrivateIdentityDto PrivateIdentityDto()
        {
            var id = new PrivateIdentityDto();
            SetPrivateIdentity(id);
            return id;
        }

        private void SetPublicIdentity(PublicIdentityDto identity)
        {
            identity.login = privateIdentity.login;
            identity.displayName = privateIdentity.displayName;
            identity.isServer = privateIdentity.isServer;
        }

        private void SetIdentity(IdentityDto identity)
        {
            identity.localToken = privateIdentity.localToken;
            identity.headerToken = privateIdentity.headerToken;
            identity.key = privateIdentity.key;
            identity.guid = privateIdentity.guid;
            SetPublicIdentity(identity);
        }

        private void SetRegisterIdentity(RegisterIdentityDto identity)
        {
            identity.metaData = gate?.metaData;
            SetIdentity(identity);
        }

        private void SetPrivateIdentity(PrivateIdentityDto identity)
        {
            identity.connectionDto = privateIdentity.connectionDto;
            identity.globalToken = privateIdentity.globalToken;
            SetIdentity(identity);
        }
        #endregion

        public void Update(ConnectionDto connectionDto)
        {
            privateIdentity.globalToken = connectionDto.globalToken;
            gate = connectionDto.gate;
            LoadLibraryOnly = connectionDto.libraryPreloading;
        }

        public void Set(string globalToken)
        {
            privateIdentity.globalToken = globalToken;
        }

        public void Set(string localToken, string headerToken, string key)
        {
            privateIdentity.localToken = localToken;
            privateIdentity.headerToken = headerToken;
            privateIdentity.key = headerToken;
        }

        public void Set(common.EnvironmentConnectionDto connectionDto)
        {
            privateIdentity.connectionDto = connectionDto;
        }
    }
}