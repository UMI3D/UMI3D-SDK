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

using umi3d.edk.userCapture.pose;

using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    [Serializable]
    public class BrowserPoseAnimatorActivationConditionField
    {
        public enum ConditionType
        {
            MAGNITUDE,
            DIRECTION,
            SCALE,
            BONE_ROTATION
        }

        public ConditionType conditionType;

        [Header("Parameters")]
        public MagnitudeCondition magnitudeCondition = new();
        public DirectionCondition directionCondition = new();
        public ScaleCondition scaleCondition = new();
        public BoneRotationCondition boneRotationCondition = new();

        public static AbstractBrowserPoseAnimatorActivationCondition ToCondition(BrowserPoseAnimatorActivationConditionField field)
        {
            return field.conditionType switch
            {
                ConditionType.MAGNITUDE => field.magnitudeCondition,
                ConditionType.DIRECTION => field.directionCondition,
                ConditionType.SCALE => field.scaleCondition,
                ConditionType.BONE_ROTATION => field.boneRotationCondition,
                _ => null
            };
        }

        public static BrowserPoseAnimatorActivationConditionField ToField(AbstractBrowserPoseAnimatorActivationCondition condition)
        {
            var field = new BrowserPoseAnimatorActivationConditionField();

            switch (condition)
            {
                case MagnitudeCondition magnitudeCondition:
                    field.magnitudeCondition = magnitudeCondition;
                    break;

                case DirectionCondition directionCondition:
                    field.directionCondition = directionCondition;
                    break;

                case ScaleCondition scaleCondition:
                    field.scaleCondition = scaleCondition;
                    break;

                case BoneRotationCondition boneRotationCondition:
                    field.boneRotationCondition = boneRotationCondition;
                    break;

                default:
                    break;
            }

            return field;
        }
    }
}