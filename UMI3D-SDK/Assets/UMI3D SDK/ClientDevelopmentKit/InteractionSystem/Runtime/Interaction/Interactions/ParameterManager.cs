/*
Copyright 2019 - 2024 Inetum

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
using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public sealed class ParameterManager : IProjectionTreeNodeDelegate<AbstractParameterDto>, IControlDelegate<AbstractParameterDto>
    {
        // IProjectionTreeNodeDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public Predicate<ProjectionTreeNodeData> IsNodeCompatible(AbstractParameterDto interaction)
        {
            return node =>
            {
                var interactionDto = node.interactionData.Interaction;
                return interactionDto is AbstractParameterDto
                && interactionDto.GetType().Equals(interaction.GetType());
            };
        }

        public Func<ProjectionTreeNodeData> CreateNodeForControl(
            string treeId,
            AbstractParameterDto interaction,
            Func<AbstractControlEntity> getControl
        )
        {
            return () =>
            {
                AbstractControlEntity control = getControl?.Invoke();

                if (control == null)
                {
                    throw new NoInputFoundException($"For {nameof(AbstractParameterDto)}: {interaction.name}");
                }

                return new ProjectionTreeNodeData()
                {
                    treeId = treeId,
                    id = interaction.id,
                    children = new(),
                    interactionData = new ProjectionTreeParameterNodeData()
                    {
                        interaction = interaction
                    },
                    control = control
                };
            };
        }

        public Action<ProjectionTreeNodeData> ChooseProjection(
            UMI3DControlManager controlManager,
            ulong? environmentId = null,
            ulong? toolId = null,
            ulong? hoveredObjectId = null
        )
        {
            return node =>
            {
                if (environmentId.HasValue && toolId.HasValue && hoveredObjectId.HasValue)
                {
                    controlManager.Associate(
                        node.control,
                        environmentId.Value,
                        toolId.Value,
                        node.interactionData.Interaction,
                        hoveredObjectId.Value
                    );
                }
            };
        }

        // IControlDelegate
        // *****************************************************
        // *****************************************************
        // *****************************************************
        // *****************************************************

        public bool CanPerform(System.Object value)
        {
            throw new System.NotImplementedException();
        }

        public void Associate(
            UMI3DController controller,
            AbstractControlEntity control,
            ulong environmentId,
            AbstractInteractionDto interaction,
            ulong toolId,
            ulong hoveredObjectId
        )
        {
            if (interaction is not AbstractParameterDto abParam)
            {
                throw new Exception($"[UMI3D] Control: Interaction is not an {nameof(AbstractParameterDto)}.");
            }

            control.controlData.actionPerformedHandler = value =>
            {
                uint boneType = control
                   .controlData
                   .controller
                   .BoneType;
                Vector3Dto bonePosition = control
                    .controlData
                    .controller
                    .BoneTransform
                    .position
                    .Dto();
                Vector4Dto boneRotation = control
                    .controlData
                    .controller
                    .BoneTransform
                    .rotation
                    .Dto();

                var request = new ParameterSettingRequestDto
                {
                    toolId = toolId,
                    id = interaction.id,
                    boneType = boneType,
                    bonePosition = bonePosition,
                    boneRotation = boneRotation,
                    hoveredObjectId = hoveredObjectId
                };
                if (interaction is IntegerRangeParameterDto intRange)
                {
                    var _value = (int)value;
                    if (_value < intRange.min || _value > intRange.max)
                    {
                        return;
                    }
                    intRange.value = _value;
                    request.parameter = intRange;
                }
                else if (interaction is AbstractRangeParameterDto<float> floatRange)
                {
                    var _value = (float)value;
                    if (_value < floatRange.min || _value > floatRange.max)
                    {
                        return;
                    }
                    floatRange.value = _value;
                    request.parameter = floatRange;
                }
                else if (interaction is EnumParameterDto<string> stringEnum)
                {
                    var _value = (string)value;
                    if (!stringEnum.possibleValues.Contains(_value))
                    {
                        return;
                    }
                    stringEnum.value = _value;
                    request.parameter = stringEnum;
                }
                else if(interaction is BooleanParameterDto toggle)
                {
                    toggle.value = (bool)value;
                    request.parameter = toggle;
                }
                else if (interaction is StringParameterDto text)
                {
                    text.value = (string)value;
                    request.parameter = text;
                }
                else if(interaction is IntegerParameterDto intNumber)
                {
                    intNumber.value = (int)value;
                    request.parameter = intNumber;
                }
                else if (interaction is FloatParameterDto floatNumber)
                {
                    floatNumber.value = (float)value;
                    request.parameter = floatNumber;
                }
                else if (interaction is ColorParameterDto color)
                {
                    color.value = (ColorDto)value;
                    request.parameter = color;
                }
                else if (interaction is Vector2ParameterDto vector2D)
                {
                    vector2D.value = (Vector2Dto)value;
                    request.parameter = vector2D;
                }
                else if (interaction is Vector3ParameterDto vector3D)
                {
                    vector3D.value = (Vector3Dto)value;
                    request.parameter = vector3D;
                }
                else if (interaction is Vector4ParameterDto vector4D)
                {
                    vector4D.value = (Vector4Dto)value;
                    request.parameter = vector4D;
                }
                else
                {
                    throw new Exception("");
                }

                UMI3DClientServer.SendRequest(request, true);
            };
        }

        public void Dissociate(AbstractControlEntity control)
        {
            (this as IControlDelegate<AbstractParameterDto>).BaseDissociate(control);
        }

        public AbstractControlEntity GetControl(
            UMI3DController controller,
            AbstractParameterDto interaction
        )
        {
            var model = controller.controlManager.model;
            if (interaction is IntegerRangeParameterDto intRange)
            {
                return model.GetSlider();
            }
            else if (interaction is FloatRangeParameterDto floatRange)
            {
                return model.GetSlider();
            }
            else if (interaction is EnumParameterDto<string> stringEnum)
            {
                return model.GetEnum();
            }
            else if (interaction is BooleanParameterDto toggle)
            {
                return model.GetToggle();
            }
            else if (interaction is StringParameterDto text)
            {
                return model.GetText();
            }
            else if (interaction is IntegerParameterDto intNumber)
            {
                throw new System.NotImplementedException();
            }
            else if (interaction is FloatParameterDto floatNumber)
            {
                throw new System.NotImplementedException();
            }
            else if (interaction is ColorParameterDto color)
            {
                throw new System.NotImplementedException();
            }
            else if (interaction is Vector2ParameterDto vector2D)
            {
                throw new System.NotImplementedException();
            }
            else if (interaction is Vector3ParameterDto vector3D)
            {
                throw new System.NotImplementedException();
            }
            else if (interaction is Vector4ParameterDto vector4D)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                throw new Exception("");
            }
        }
    }
}