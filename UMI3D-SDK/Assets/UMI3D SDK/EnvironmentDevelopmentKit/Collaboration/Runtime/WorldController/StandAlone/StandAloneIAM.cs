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
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.worldController
{
    /// <summary>
    /// Identity and Access manager that runs on the server as a standalone.
    /// </summary>
    public class StandAloneIAM : IIAM
    {
        protected readonly IEnvironment environment;
        public StandAloneIAM(IEnvironment environment) { this.environment = environment; }

        private readonly List<string> tokens = new List<string>();

        public virtual async Task<ConnectionFormDto> GenerateForm(User user)
        {
            var form = new ConnectionFormDto()
            {
                globalToken = user.globalToken,
                name = "Connection",
                description = null,
                fields = new List<AbstractParameterDto>()
            };

            form.fields.Add(
                new StringParameterDto()
                {
                    id = 1,
                    name = "Login",
                    value = ""
                });
            form.fields.Add(
                new StringParameterDto()
                {
                    id = 2,
                    privateParameter = true,
                    name = "Password",
                    value = ""
                });
            form.fields.Add(
                new EnumParameterDto<string>()
                {
                    id = 3,
                    name = "Select an option",
                    possibleValues = new List<string>() { "ValueA", "ValueB", "ValueC", "ValueD", "Une Pomme", "@&&&é" },
                    value = ""
                });


            return await Task.FromResult(form);
        }

        public virtual async Task<IEnvironment> GetEnvironment(User user)
        {
            return await Task.FromResult(environment);
        }

        public virtual async Task<List<AssetLibraryDto>> GetLibraries(User user)
        {
            return await Task.FromResult<List<AssetLibraryDto>>(null);
        }

        public virtual async Task<bool> isFormValid(User user, common.interaction.FormAnswerDto formAnswer)
        {
            UnityEngine.Debug.Log(formAnswer.ToJson(Newtonsoft.Json.TypeNameHandling.None));

            SetToken(user);
            return await Task.FromResult(true);
        }

        public virtual async Task<bool> IsUserValid(User user)
        {
            if (user.Token != null && tokens.Contains(user.Token))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public virtual async Task RenewCredential(User user)
        {
            SetToken(user);
            await Task.CompletedTask;
        }

        public void SetToken(User user)
        {
            if (user.Token == null || !tokens.Contains(user.Token))
            {
                string token = System.Guid.NewGuid().ToString();
                tokens.Add(token);
                user.Set(token);
            }
        }
    }
}