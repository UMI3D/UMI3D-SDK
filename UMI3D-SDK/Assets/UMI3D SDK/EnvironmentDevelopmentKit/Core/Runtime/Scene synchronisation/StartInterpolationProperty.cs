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
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// An operation to start interpolatation on a property's entity.
    /// </summary>
    public class StartInterpolationProperty : AbstractInterpolationProperty
    {
        /// <summary>
        /// The value at which to start interpolation.
        /// </summary>
        public object startValue;

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            if (startValue == null)
                UMI3DLogger.LogError($"{nameof(startValue)} property was not set for {nameof(StartInterpolationProperty)}", DebugScope.EDK | DebugScope.Core);

            return UMI3DSerializer.Write(UMI3DOperationKeys.StartInterpolationProperty)
                + UMI3DSerializer.Write(entityId)
                + UMI3DSerializer.Write(property)
                + UMI3DSerializer.Write(startValue);

        }

        /// <inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            if (startValue == null)
                UMI3DLogger.LogError($"{nameof(startValue)} property was not set for {nameof(StartInterpolationProperty)}", DebugScope.EDK | DebugScope.Core);

            var startInterpolation = new StartInterpolationPropertyDto
            {
                property = property,
                entityId = entityId,
                startValue = startValue
            };
            return startInterpolation;
        }
    }

    /// <summary>
    /// An operation to start interpolatation on a property's entity.
    /// </summary>
    public class StartInterpolationProperty<PropertyType> : AbstractInterpolationProperty
    {
        /// <summary>
        /// The value at which to start interpolation.
        /// </summary>
        public PropertyType startValue = default;

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.StartInterpolationProperty)
                + UMI3DSerializer.Write(entityId)
                + UMI3DSerializer.Write(property)
                + UMI3DSerializer.Write(startValue);

        }

        /// <inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var startInterpolation = new StartInterpolationPropertyDto
            {
                property = property,
                entityId = entityId,
                startValue = startValue
            };
            return startInterpolation;
        }
    }
}
