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
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// An opertion to stop interpolation on a property's entity
    /// </summary>
    public class StopInterpolationProperty : AbstractInterpolationProperty
    {
        /// <summary>
        /// The value with which to stop interpolation
        /// </summary>
        public object stopValue;

        public override (int, Func<byte[], int, int>) ToBytes(UMI3DUser user)
        {
            int size = 2*sizeof(uint) + sizeof(ulong) + UMI3DNetworkingHelper.GetSize(stopValue);
            Func<byte[], int, int> func = (b, i) => {
                i += UMI3DNetworkingHelper.Write(UMI3DOperationKeys.SetEntityProperty, b, i);
                i += UMI3DNetworkingHelper.Write(entityId, b, i);
                i += UMI3DNetworkingHelper.Write(property, b, i);
                i += UMI3DNetworkingHelper.Write(stopValue, b, i);
                return size;
            };
            return (size, func);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var stopInterpolation = new StopInterpolationPropertyDto();
            stopInterpolation.property = property;
            stopInterpolation.entityId = entityId;
            stopInterpolation.stopValue = stopValue;
            return stopInterpolation;
        }
    }
}
