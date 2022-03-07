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
using System.Collections.Generic;
using System;
using inetum.unityUtils;

namespace umi3d.ms
{
    public class MasterServer : Singleton<MasterServer>
    {
        IIAM IAM; 
        Dictionary<string, User> userMap = new Dictionary<string, User>();

        public MasterServer() : base()
        {}

        public MasterServer(IIAM iAM): this()
        {
            this.IAM = iAM;
        }

        static UMI3DDto RegisterUser(ConnectionDto dto)
        {
            return Instance?._RegisterUser(dto);
        }

        UMI3DDto _RegisterUser(ConnectionDto dto)
        {
            User user = GetUser(dto);

            return dto is FormConnectionAnswerDto formAnswer
                ? IAM.isFormValid(user, formAnswer.FormAnswerDto) ? GetIdentityDto(user) : IAM.GenerateForm(user)
                : IAM.IsUserValid(user) ? GetIdentityDto(user) : IAM.GenerateForm(user);
        }

        User GetUser(ConnectionDto connectionDto)
        {
            var id = connectionDto.GlobalToken ?? generateFakeToken();

            if (userMap.ContainsKey(id))
                userMap[id].Update(connectionDto);
            else
                userMap.Add(id, new User(connectionDto));

            return userMap[id];
        }

        string generateFakeToken()
        {
            return (new Guid()).ToString();
        }

        IdentityDto GetIdentityDto(User user) { throw new Exception(); }

        string generateLocalToken()
        {
            return (new Guid()).ToString();
        }

        string generateKey()
        {
            return null;
        }

    }
}