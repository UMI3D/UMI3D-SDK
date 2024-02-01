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
    /// Identity and Access Management interface.
    /// </summary>
    public interface IIAM
    {
        /// <summary>
        /// Generate A formdto
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ConnectionFormDto> GenerateForm(User user);

        /// <summary>
        /// State if a user is valid.
        /// The globalToken shoulb be valide if true.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> IsUserValid(User user);

        /// <summary>
        /// State if a user is valid.
        /// The globalToken shoulb be valide if true.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="formAnswer"></param>
        /// <returns></returns>
        Task<bool> isFormValid(User user, FormAnswerDto formAnswer);

        /// <summary>
        /// Select an Environment for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnvironment> GetEnvironment(User user);

        /// <summary>
        /// Return a list of library to donwload.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<List<AssetLibraryDto>> GetLibraries(User user);

        /// <summary>
        /// Update user credential
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task RenewCredential(User user);
    }
}