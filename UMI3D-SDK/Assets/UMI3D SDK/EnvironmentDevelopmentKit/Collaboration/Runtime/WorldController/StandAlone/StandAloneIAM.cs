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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using umi3d.common.interaction.form;
using umi3d.common.interaction.form.ugui;
using umi3d.edk;

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

        public virtual async Task<common.interaction.ConnectionFormDto> GenerateForm(User user)
        {
            var form = new common.interaction.ConnectionFormDto()
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
        public virtual async Task<umi3d.common.interaction.form.ConnectionFormDto> GenerateDivForm(User user)
        {
            return await GenerateDivFormLogin(user);
        }

        public virtual async Task<umi3d.common.interaction.form.ConnectionFormDto> GenerateDivFormVignettes(User user)
        {
            var root = new ConnectionFormBuilder("form-environment", "environment");

            var lastPage = root.AddPage("page-last", "Last");
            {
                for (int i = 0; i < 3; i++)
                {
                    var vignette = lastPage.AddImage("vignette-image-" + i, UMI3DServer.publicRepository + "/ButtonOk.png", "png", new AssetMetricDto() { resolution = 0, size = 0.063f });
                    vignette.AddLabel("vignette-label-" + i, "Name"); // TODO : Correct vignette name
                }
            }

            var connectionGroup = root.AddGroup("group-connectionInfo")
                .Position(-275, 160).Size(190, 61);
            {
                connectionGroup.AddImage("image-user", UMI3DServer.publicRepository + "/ButtonOk.png", "png", new AssetMetricDto() { resolution = 0, size = 0.063f }) // TODO Correct User icon
                    .Position(-68, -4).Size(35, 35);
                connectionGroup.AddLabel("label-connected", "Connected as")
                    .Position(5, 13).Size(100, 19)
                    .TextSize(16).TextColor(1, 1, 1, 1);
                connectionGroup.AddLabel("label-user", "User Name") // TODO Correct User name
                    .Position(207, 13).Size(300, 19)
                    .TextSize(16).TextColor(1, 1, 1, 1).AddTextStyle(E_FontStyle.Bold);
                connectionGroup.AddLabel("label-portal", "Portal Name") // TODO Correct Portal name
                    .Position(26.7f, -3).Size(142.6f, 16)
                    .TextSize(16).TextColor(0, 0.8f, 1, 1);
                connectionGroup.AddButton("button-cancel", "Log out").Type(ButtonType.Cancel)
                    .Position(26.7f, -18).Size(142.6f, 16)
                    .TextSize(12).TextColor(0.8f, 0.8f, 0.8f, 1).AddTextAlignement(E_FontAlignment.Left);
            }
            root.AddButton("button-back", "< BACK").Type(ButtonType.Back)
                .Image(UMI3DServer.publicRepository + "/ButtonBack.png", "png", new() { resolution = 8, size = 0.6f }) // TODO Image button
                .Position(-460, 220).Size(90, 32).Color(0.447f, 0.447f, 0.447f, 1)
                .TextSize(18).AddTextStyle(E_FontStyle.Bold).AddTextStyle(E_FontStyle.Uppercase);

            return await Task.FromResult(root.Get());
        }

        public virtual async Task<umi3d.common.interaction.form.ConnectionFormDto> GenerateDivFormLogin(User user)
        {
            var root = new ConnectionFormBuilder("form-login", "login");

            var loginPage = root.AddPage("page-login", "Login");
            {
                loginPage.AddInput<string>("input-username", "Username")
                    .Placeholder("John Doe").Type(TextType.Text)
                    .Position(0, 98);
                loginPage.AddInput<string>("input-password", "Password")
                    .Placeholder("***************").Type(TextType.Password)
                    .Position(0, -15);
            }

            var pinPage = root.AddPage("page-pin", "Pin");
            {
                pinPage.AddInput<string>("input-pin", "Pin")
                    .Placeholder("123456").Type(TextType.Number)
                    .Position(0, 55);
            }

            root.AddButton("button-submit", "OK").Type(ButtonType.Submit)
                .Image(UMI3DServer.publicRepository + "/ButtonOk.png", "png", new() { resolution = 8, size = 0.6f }) // TODO Image button
                .Position(0, -160).Size(95, 54).Color(0.447f, 0.447f, 0.447f, 1)
                .TextSize(24).AddTextStyle(E_FontStyle.Bold).AddTextStyle(E_FontStyle.Uppercase);

            root.AddButton("button-back", "< BACK").Type(ButtonType.Cancel)
                .Image(UMI3DServer.publicRepository + "/ButtonBack.png", "png", new() { resolution = 8, size = 0.6f }) // TODO Image button
                .Position(-460, 220).Size(90, 32).Color(0.447f, 0.447f, 0.447f, 1)
                .TextSize(18).AddTextStyle(E_FontStyle.Bold).AddTextStyle(E_FontStyle.Uppercase);

            return await Task.FromResult(root.Get());
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

        public virtual async Task<bool> isDivFormValid(User user, common.interaction.form.FormAnswerDto formAnswer)
        {
            UnityEngine.Debug.Log(formAnswer.ToJson(Newtonsoft.Json.TypeNameHandling.Auto));

            SetToken(user);
            if (formAnswer.isCancelation || formAnswer.isBack)
                return await Task.FromResult(false);
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