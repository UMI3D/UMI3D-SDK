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
using System.Linq;
using inetum.unityUtils;
using System.Threading.Tasks;

namespace umi3d.ms
{
    public class MasterServer : Singleton<MasterServer>, IMasterServer
    {
        IIAM IAM;
        IKeyGenerator keyGenerator;
        Dictionary<string, User> userMap = new Dictionary<string, User>();

        public string ip;
        public string mediaName;

        public MasterServer() : base()
        {}

        public MasterServer(IIAM iAM,IKeyGenerator keyGenerator): this()
        {
            this.IAM = iAM;
            this.keyGenerator = keyGenerator;
        }

        static public async Task<UMI3DDto> RegisterUser(ConnectionDto connectionDto)
        {
            if (Exists)
               return await Instance?._RegisterUser(connectionDto);
            else
                throw new Exception("Instance of master server do not exist");
        }

        async Task<UMI3DDto> _RegisterUser(ConnectionDto connectionDto)
        {
            User user = GetUser(connectionDto);

            var dto = (connectionDto is FormConnectionAnswerDto formAnswer)
                ? await IAM.isFormValid(user, formAnswer.FormAnswerDto) ? await GetIdentityDto(user) : (UMI3DDto)await IAM.GenerateForm(user)
                : await IAM.IsUserValid(user) ? await GetIdentityDto(user) : (UMI3DDto)await IAM.GenerateForm(user);

            UnityEngine.Debug.Log($"Register {dto}");

            return dto;
        }

        async public Task<PrivateIdentityDto> RenewCredential(string globalTocken)
        {
            if (globalTocken != null && userMap.ContainsKey(globalTocken))
            {
                var user  = userMap[globalTocken];
                if(user != null)
                {
                    await IAM.RenewCredential(user);
                    return await GetIdentityDto(user);
                }
            }
            return await Task.FromResult<PrivateIdentityDto>(null);
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

        /// <summary>
        /// return a token that can be used to identify a user temporaly.
        /// </summary>
        /// <returns></returns>
        string generateFakeToken()
        {
            return (new Guid()).ToString();
        }

        async Task<PrivateIdentityDto> GetIdentityDto(User user) {
            //General token is valid.
            if (!userMap.ContainsKey(user.Token))
            {
                var tmp = userMap.FirstOrDefault(uk => uk.Value == user).Key;
                userMap.Remove(tmp);
                userMap.Add(user.Token, user);
            }

            if (!user.LoadLibraryOnly)
            {
                //Select environment
                var env = await IAM.GetEnvironment(user);

                user.Set(await env.ToDto());
                user.Set(
                    keyGenerator.GenerateLocalToken(user.localToken),
                    keyGenerator.GenerateHeaderToken(user.headearToken),
                    keyGenerator.GenerateKey(user.key)
                    );

                await env.Register(user.RegisterIdentityDto());
            }
            var l = await IAM.GetLibraries(user);
            var privateId = user.PrivateIdentityDto();
            privateId.libraries = l;
            return privateId;
        }

        async Task<UMI3DDto> IMasterServer.Connect(ConnectionDto connectionDto)
        {
            return await _RegisterUser(connectionDto);
        }


        /// <summary>
        /// Get scene's information required for client connection.
        /// </summary>
        public MediaDto ToDto()
        {
            var res = new MediaDto
            {
                name = mediaName,
                url = ip,
                //connection = UMI3DServer.Instance.ToDto(),
                versionMajor = UMI3DVersion.major,
                versionMinor = UMI3DVersion.minor,
                versionStatus = UMI3DVersion.status,
                versionDate = UMI3DVersion.date
            };

            return res;
        }

    }
}