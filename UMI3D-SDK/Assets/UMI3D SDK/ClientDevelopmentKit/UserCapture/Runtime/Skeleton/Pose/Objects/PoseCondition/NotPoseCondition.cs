﻿/*
Copyright 2019 - 2023 Inetum

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
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Wrapper for <see cref="NotConditionDto"/>.
    /// </summary>
    public class NotPoseCondition : IPoseCondition
    {
        protected readonly NotConditionDto notConditionDto;

        public readonly IPoseCondition condition;

        public NotPoseCondition(NotConditionDto dto, IPoseCondition condition)
        {
            this.notConditionDto = dto ?? throw new ArgumentNullException(nameof(dto));
            this.condition = condition;
        }

        /// <inheritdoc/>
        public bool Check()
        {
            return !(condition?.Check() ?? false);
        }
    }
}