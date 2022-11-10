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
using umi3d.common.interaction;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// DTO describing the browser answer to connection requirements.
    /// </summary>
    [Serializable]
    public class UserConnectionAnswerDto : UserDto
    {
        /// <summary>
        /// Answers to the connection form.
        /// </summary>
        /// Not null if the received <see cref="UserConnectionDto"/> contained a form.
        public FormAnswerDto parameters;

        public bool librariesUpdated = false;

        public UserConnectionAnswerDto() : base()
        {
        }

        public UserConnectionAnswerDto(UserDto user) : base(user)
        {
        }

        public UserConnectionAnswerDto(UserConnectionDto user) : base(user)
        {
            librariesUpdated = user.librariesUpdated;
        }
    }
}