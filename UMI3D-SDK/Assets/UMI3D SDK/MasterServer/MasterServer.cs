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
using umi3d.common.interaction;
using System.Collections.Generic;
using System;
using inetum.unityUtils;

namespace umi3d.ms
{
    public class MasterServer : Singleton<MasterServer>
    {
        public class User
        {
            public string GlobalToken;
            public GateDto gate;
            public string GUID;
            public string localToken;

            public User(ConnectionDto connectionDto)
            {
                Update(connectionDto);
            }
            public void Update(ConnectionDto connectionDto)
            {
                GlobalToken = connectionDto.GlobalToken;
                gate = connectionDto.gate;
            }
        }


        IIAM IAM; 
        Dictionary<string, User> userMap = new Dictionary<string, User>();

        UMI3DDto RegisterUser(UMI3DDto dto)
        {
            return Instance?._RegisterUser(dto);
        }



        UMI3DDto _RegisterUser(UMI3DDto dto)
        {
            User user;
            switch (dto)
            {
                case FormConnectionAnswerDto formAnswer:
                    user = GetUser(formAnswer);
                    return IAM.isFormValid(user, formAnswer.FormAnswerDto) ? GetIdentityDto(user) : IAM.GenerateForm(user);
                case ConnectionDto connectionDto:
                    user = GetUser(connectionDto);
                    return IAM.IsUserValid(user) ? GetIdentityDto(user) : IAM.GenerateForm(user);
            }
            return null;
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

        interface IIAM
        {
            UMI3DDto GenerateForm(User user);
            bool IsUserValid(User user);
            string generateToken(User user);
            bool isFormValid(User user,FormAnswerDto formAnswer);

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