﻿/*
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
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// An operation to transmit information about interpolation to users.
    /// </summary>
    public class StartInterpolationProperty : AbstractInterpolationProperty
    {
        /// <summary>
        /// The new value for the property
        /// </summary>
        public object startValue;

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var startInterpolation = new StartInterpolationPropertyDto();
            startInterpolation.property = property;
            startInterpolation.entityId = entityId;
            startInterpolation.frequency = frequency;
            startInterpolation.startValue = startValue;
            return startInterpolation;
        }
    }
}
