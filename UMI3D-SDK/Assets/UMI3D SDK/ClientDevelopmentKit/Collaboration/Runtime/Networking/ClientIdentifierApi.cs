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
using System.Linq;
using System.Threading.Tasks;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// API to give a password for the client, return filled up Form, and state if needed libraries should be downloaded.
    /// </summary>
    public abstract class ClientIdentifierApi : ScriptableObject
    {
        /// <summary>
        /// Fill a form and return it via a callback.
        /// </summary>
        /// <param name="parameter">FormDto to be filled.</param>
        /// <param name="callback">Action to return the completed FormDto.</param>
        public virtual async Task<FormAnswerDto> GetParameterDtos(ConnectionFormDto parameter)
        {
            return await Task.FromResult(new FormAnswerDto()
            {
                id = parameter.id,
                toolId = 0,
                boneType = 0,
                hoveredObjectId = 0,
                answers = parameter.fields.Select(a => new ParameterSettingRequestDto() { toolId = 0, id = a.id, boneType = 0, hoveredObjectId = 0, parameter = a.GetValue() }).ToList()
            });
        }

        /// <summary>
        /// Fill a form and return it via a callback.
        /// </summary>
        /// <param name="parameter">FormDto to be filled.</param>
        /// <param name="callback">Action to return the completed FormDto.</param>
        public virtual async Task<umi3d.common.interaction.form.FormAnswerDto> GetParameterDtos(common.interaction.form.ConnectionFormDto parameter)
        {
            return await Task.FromResult(new umi3d.common.interaction.form.FormAnswerDto() {
                formId = parameter.guid,
                pageId = "",
                submitId = "",
                isBack = false,
                isCancelation = false,
                inputs = new()
            });
        }

        /// <summary>
        /// Display A wait Connection .
        /// </summary>
        /// <param name="parameter">FormDto to be filled.</param>
        /// <param name="callback">Action to return the completed FormDto.</param>
        public virtual async Task GetParameterDtos(WaitConnectionDto parameter)
        {
            if (parameter is WebConnectionDto webConnection)
            {
                UnityEngine.Application.OpenURL(webConnection.connectionUrl);
            }
            UnityEngine.Debug.Log($"Wait for {parameter.waitTimeSecond} : {parameter.message} {parameter.id}");
            await UMI3DAsyncManager.Delay((int)(parameter.waitTimeSecond * 1000));
            UnityEngine.Debug.Log($"End Wait for {parameter.waitTimeSecond} : {parameter.message} {parameter.id}");
        }

        /// <summary>
        /// Should State if the needed libraries should be downloaded.
        /// </summary>
        /// <param name="LibrariesId">Ids of all library that need to be downloaded or updated</param>
        /// <param name="callback">Action to return the answer</param>
        public virtual async Task<bool> ShouldDownloadLibraries(List<string> LibrariesId)
        {
            return await Task.FromResult(true);
        }
    }
}