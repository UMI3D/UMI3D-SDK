/*
Copy 2019 - 2023 Inetum

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
#if UNITY_EDITOR
using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using umi3d.common.userCapture.pose;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture
{
    public class ConditionField : VisualElement
    {
        DropdownField dropdownField = null;

        List<VisualElement> temporaryChildren = new List<VisualElement>();
        TypeCache.TypeCollection typeCollection;

        Type fieldActualConditionType = null;

        public ConditionField()
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets\\UMI3D SDK\\Common\\UserCapture\\Editor\\PoseSetter\\poseSetterStyle.uss"));
            AddToClassList("EditorParts");

            dropdownField = new DropdownField();
            dropdownField.label = "Condition type";
            Add(dropdownField);
            SetUpDropDown();
        }

        public AbstractPoseConditionDto GetPoseConditionDto()
        {
            if (fieldActualConditionType == typeof(MagnitudeConditionDto))
            {
                return new MagnitudeConditionDto()
                {
                    Magnitude = (temporaryChildren[0] as FloatField).value,
                    BoneOrigin = (uint)(temporaryChildren[1] as IntegerField).value,
                    //TargetNodeId  = (uint)(temporaryChildren[2] as IntegerField).value
                };
            }
            else if (fieldActualConditionType == typeof(DirectionConditionDto))
            {
                return new DirectionConditionDto()
                {
                    Direction = (temporaryChildren[0] as Vector3Field).value.Dto()
                };
            }
            else if (fieldActualConditionType == typeof(BoneRotationConditionDto))
            {
                return new BoneRotationConditionDto()
                {
                    BoneId = (uint)(temporaryChildren[0] as IntegerField).value,
                    Rotation = (temporaryChildren[1] as Vector4Field).value.Dto()
                };
            }
            else if (fieldActualConditionType == typeof(UserScaleConditionDto))
            {
                return new UserScaleConditionDto()
                {
                    Scale = (temporaryChildren[0] as Vector3Field).value.Dto()
                };
            }
            else if (fieldActualConditionType == typeof(ScaleConditionDto))
            {
                return new UserScaleConditionDto()
                {
                    Scale = (temporaryChildren[0] as Vector3Field).value.Dto()
                };
            }
            else if (fieldActualConditionType == typeof(RangeConditionDto))
            {
                return new RangeConditionDto();
            }
            else if (fieldActualConditionType == typeof(NotConditionDto))
            {
                return new NotConditionDto();
            }

            return null;
        }

        private void SetUpDropDown()
        {
            typeCollection = TypeCache.GetTypesDerivedFrom<AbstractPoseConditionDto>();

            foreach (var type in typeCollection)
            {
                dropdownField.choices.Add(type.Name);
            }

            dropdownField.RegisterValueChangedCallback((data) =>
            {
                temporaryChildren.ForEach(child =>
                {
                    this.Remove(child);
                });

                temporaryChildren.Clear();

                HandleConditionTypeChange(data.newValue);
            });
        }

        private void HandleConditionTypeChange(string newValue)
        {
            fieldActualConditionType = null;

            typeCollection.ForEach(type =>
            {
                if (type.Name == newValue)
                {
                    fieldActualConditionType = type;
                }
            });

            fieldActualConditionType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ForEach(property =>
            {
                if (property.PropertyType == typeof(Vector3Dto))
                {
                    Vector3Field vector3Field = new Vector3Field();
                    vector3Field.label = property.Name;
                    temporaryChildren.Add(vector3Field);
                    Add(vector3Field);
                }
                else if (property.PropertyType == typeof(Vector4Dto))
                {
                    Vector4Field vector4Field = new Vector4Field();
                    vector4Field.label = property.Name;
                    temporaryChildren.Add(vector4Field);
                    Add(vector4Field);
                }
                else if (property.PropertyType == typeof(uint) || property.PropertyType == typeof(int))
                {
                    IntegerField integerField = new IntegerField();
                    integerField.label = property.Name;
                    temporaryChildren.Add(integerField);
                    Add(integerField);
                }
                else if (property.PropertyType == typeof(float))
                {
                    FloatField floatField = new FloatField();
                    floatField.label = property.Name;
                    temporaryChildren.Add(floatField);
                    Add(floatField);
                }
            });
        }
    }
}
#endif

