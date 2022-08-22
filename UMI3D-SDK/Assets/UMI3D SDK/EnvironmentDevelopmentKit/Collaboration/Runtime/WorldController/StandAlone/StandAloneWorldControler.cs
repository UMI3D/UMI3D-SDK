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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.collaboration;
using UnityEngine;


namespace umi3d.worldController
{
    [CreateAssetMenu(fileName = "SandAloneWorldControlerAPI", menuName = "UMI3D/SandAlone WorldControler")]
    public class StandAloneWorldControler : StandAloneWorldControllerAPI
    {
        protected IIAM IAM;
        protected IKeyGenerator keyGenerator;
        protected Dictionary<string, User> userMap = new Dictionary<string, User>();

        public override void Setup()
        {
            base.Setup();
            if (IAM == null)
                IAM = new StandAloneIAM(edk.collaboration.UMI3DCollaborationServer.Instance);
            if (keyGenerator == null)
                keyGenerator = new StandAloneKeyGenerator();
        }

        public override async Task<UMI3DDto> Connect(ConnectionDto connectionDto)
        {
            User user = GetUser(connectionDto);

            UMI3DDto dto = (connectionDto is FormConnectionAnswerDto formAnswer && formAnswer?.formAnswerDto != null)
                ? await IAM.isFormValid(user, formAnswer.formAnswerDto) ? await GetIdentityDto(user) : (UMI3DDto)await IAM.GenerateForm(user)
                : await IAM.IsUserValid(user) ? await GetIdentityDto(user) : (UMI3DDto)await IAM.GenerateForm(user);
            return dto;
        }

        public override Task NotifyUserJoin(UMI3DCollaborationUser user)
        {
            return Task.CompletedTask;
        }

        public override async Task<PrivateIdentityDto> RenewCredential(PrivateIdentityDto identityDto)
        {
            if (identityDto?.GlobalToken != null && userMap.ContainsKey(identityDto.GlobalToken))
            {
                User user = userMap[identityDto.GlobalToken];
                if (user != null)
                {
                    await IAM.RenewCredential(user);
                    return await GetIdentityDto(user);
                }
            }
            return await Task.FromResult<PrivateIdentityDto>(null);
        }

        private User GetUser(ConnectionDto connectionDto)
        {
            string gt = connectionDto.globalToken ?? generateFakeToken();

            if (userMap.ContainsKey(gt))
                userMap[gt].Update(connectionDto);
            else
                userMap.Add(gt, new User(connectionDto, gt));

            return userMap[gt];
        }

        /// <summary>
        /// return a token that can be used to identify a user temporaly.
        /// </summary>
        /// <returns></returns>
        private string generateFakeToken()
        {
            return new Guid().ToString();
        }

        private async Task<PrivateIdentityDto> GetIdentityDto(User user)
        {
            //General token is valid.
            if (!userMap.ContainsKey(user.Token))
            {
                string tmp = userMap.FirstOrDefault(uk => uk.Value == user).Key;
                userMap.Remove(tmp);
                userMap.Add(user.Token, user);
            }

            if (!user.LoadLibraryOnly)
            {
                //Select environment
                IEnvironment env = await IAM.GetEnvironment(user);

                user.Set(await env.ToDto());
                user.Set(
                    keyGenerator.GenerateLocalToken(user.localToken),
                    keyGenerator.GenerateHeaderToken(user.headearToken),
                    keyGenerator.GenerateKey(user.key)
                    );

                await env.Register(user.RegisterIdentityDto());
            }
            List<LibrariesDto> l = await IAM.GetLibraries(user);
            PrivateIdentityDto privateId = user.PrivateIdentityDto();
            privateId.libraries = l;
            Debug.Log(privateId);
            return privateId;
        }

        public override Task NotifyUserUnregister(UMI3DCollaborationUser user)
        {
            return Task.CompletedTask;
        }

        public override Task NotifyUserLeave(UMI3DCollaborationUser user)
        {
            return Task.CompletedTask;
        }

        public override Task NotifyUserRegister(UMI3DCollaborationUser user)
        {
            return Task.CompletedTask;
        }
    }
}