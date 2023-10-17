/*
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
using System.Linq;
using umi3d.common.collaboration.userCapture.pose.dto;
using umi3d.common.userCapture.pose;

namespace umi3d.common.collaboration.userCapture.pose
{
    /// <summary>
    /// Serializer for <see cref="AbstractPoseConditionDto"/>;
    /// </summary>
    public class CollaborationPoseConditionSerializerModule : UMI3DSerializerModule
    {
        public static class CollaborationPoseConditionSerializingIndex
        {
            private static readonly int BaseMax = Enum.GetValues(typeof(PoseConditionSerializerModule.PoseConditionSerializingIndex)).Cast<int>().Max();
            public static readonly int PROJECTION_CONDITION = BaseMax + 1;
        }

        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(ProjectedPoseConditionDto) => true,

                _ => null
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(ProjectedPoseConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out int index);

                        if (readable)
                        {
                            AbstractPoseConditionDto poseConditionDto;

                            if (index == CollaborationPoseConditionSerializingIndex.PROJECTION_CONDITION)
                            {
                                ReadConditionDTO(container, out readable, out ProjectedPoseConditionDto projectionPoseCondition);
                                poseConditionDto = projectionPoseCondition;

                                if (poseConditionDto != null)
                                {
                                    result = (T)(object)poseConditionDto;
                                    return true;
                                }
                            }
                        }

                        result = default;
                        return false;
                    }

                default:
                    result = default;
                    readable = false;
                    return false;
            }
        }

        private bool ReadConditionDTO<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(ProjectedPoseConditionDto):
                    {
                        readable = UMI3DSerializer.TryRead<ulong>(container, out ulong interactableId);

                        if (!readable)
                            break;

                        var projectedPoseCondition = new ProjectedPoseConditionDto()
                        {
                            interactableId = interactableId
                        };
                        result = (T)Convert.ChangeType(projectedPoseCondition, typeof(T));
                        readable = true;
                        return true;
                    }
            }
            result = default;
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case ProjectedPoseConditionDto projectedPoseConditionDto:
                    bytable = UMI3DSerializer.Write((int)CollaborationPoseConditionSerializingIndex.PROJECTION_CONDITION)
                        + UMI3DSerializer.Write(projectedPoseConditionDto.interactableId);
                    break;

                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}