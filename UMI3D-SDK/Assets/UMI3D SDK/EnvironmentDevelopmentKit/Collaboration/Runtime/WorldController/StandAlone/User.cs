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
using umi3d.common.collaboration;

namespace umi3d.worldController
{
    public class User
    {
        public bool LoadLibraryOnly;
        public GateDto gate;
        private PrivateIdentityDto privateIdentity;
        public string Token => privateIdentity.GlobalToken;

        public string localToken => privateIdentity.localToken;
        public string headearToken => privateIdentity.headerToken;
        public string key => privateIdentity.key;

        public string globalToken;

        static Dictionary<ulong, User> ids = new Dictionary<ulong, User>();

        public User(ConnectionDto connectionDto, string globalToken)
        {
            this.globalToken = globalToken;
            privateIdentity = new PrivateIdentityDto();
            privateIdentity.guid = NewID();
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
            identity.GlobalToken = privateIdentity.GlobalToken;
            SetIdentity(identity);
        }
        #endregion

        public void Update(ConnectionDto connectionDto)
        {
            privateIdentity.GlobalToken = connectionDto.GlobalToken;
            gate = connectionDto.gate;
            LoadLibraryOnly = connectionDto.LibraryPreloading;
        }

        public void Set(string globalToken)
        {
            privateIdentity.GlobalToken = globalToken;
        }

        public void Set(string localToken,string headerToken,string key)
        {
            privateIdentity.localToken = localToken;
            privateIdentity.headerToken = headerToken;
            privateIdentity.key = headerToken;
        }

        public void Set(common.ForgeConnectionDto connectionDto)
        {
            privateIdentity.connectionDto = connectionDto;
        }

    }
}