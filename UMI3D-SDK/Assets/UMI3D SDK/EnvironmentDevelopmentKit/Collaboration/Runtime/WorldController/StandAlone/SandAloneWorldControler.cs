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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;


namespace umi3d.worldController
{
    [CreateAssetMenu(fileName = "SandAloneWorldControlerAPI", menuName = "UMI3D/SandAlone WorldControler")]
    public class SandAloneWorldControler : StandAloneWorldControllerAPI
    {
        IIAM IAM;
        IKeyGenerator keyGenerator;
        Dictionary<string, User> userMap = new Dictionary<string, User>();

        public override void Setup()
        {
            base.Setup();
            IAM = new StandAloneIAM(edk.collaboration.UMI3DCollaborationServer.Instance);
            keyGenerator = new StandAloneKeyGenerator();
        }

        public override async Task<UMI3DDto> Connect(ConnectionDto connectionDto)
        {
            User user = GetUser(connectionDto);

            var dto = (connectionDto is FormConnectionAnswerDto formAnswer)
                ? await IAM.isFormValid(user, formAnswer.FormAnswerDto) ? await GetIdentityDto(user) : (UMI3DDto)await IAM.GenerateForm(user)
                : await IAM.IsUserValid(user) ? await GetIdentityDto(user) : (UMI3DDto)await IAM.GenerateForm(user);

            return dto;
        }

        public override Task NotifyUserJoin(string uid)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<PrivateIdentityDto> RenewCredential(PrivateIdentityDto identityDto)
        {
            if (identityDto?.GlobalToken != null && userMap.ContainsKey(identityDto.GlobalToken))
            {
                var user = userMap[identityDto.GlobalToken];
                if (user != null)
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
                userMap.Add(id, new User(connectionDto, id));

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


        async Task<PrivateIdentityDto> GetIdentityDto(User user)
        {
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

        public override Task NotifyUserLeave(string uid)
        {
            throw new NotImplementedException();
        }
    }
}