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

using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Client's side interactable object.
    /// </summary>
    /// <see cref="InteractableDto"/>
    public class Tool : AbstractTool
    {

        public class Event : UnityEvent<Tool> { }

        public static ToolMenuItem IdToMenu(ulong id) { return (UMI3DEnvironmentLoader.GetEntity(id)?.Object as Tool)?.Menu; }

        public ToolMenuItem Menu;

        public Toolbox toolbox;
        /// <summary>
        /// ToolFix t dto describing this object.
        /// </summary>
        public ToolDto dto;

        ///<inheritdoc/>
        protected override AbstractToolDto abstractDto { get => dto; set => dto = value as ToolDto; }

        public Tool(ToolDto dto, Toolbox toolbox) : base(dto)
        {
            this.toolbox = toolbox;
            Menu = new ToolMenuItem()
            {
                tool = this,
                Name = name,
            };
            foreach (var interaction in dto.interactions)
            {
                var item = getInteractionItem(interaction);
                Menu.Add(item);
            }

            toolbox?.tools.Add(this);
            toolbox?.sub.Add(Menu);
        }


        MenuItem getInteractionItem(AbstractInteractionDto dto)
        {
            MenuItem result = null;
            switch (dto)
            {
                case ManipulationDto manipulation:
                    result = new MenuItem();
                    break;
                case EventDto eventDto:
                    var e = new EventMenuItem();
                    e.interaction = eventDto;
                    e.toggle = eventDto.hold;
                    e.Subscribe((x) =>
                    {
                        if (eventDto.hold)
                        {
                            var stateChangeDto = new EventStateChangedDto
                            {
                                active = x,
                                boneType = (uint)0,
                                id = eventDto.id,
                                toolId = dto.id,
                                hoveredObjectId = 0
                            };
                            UMI3DClientServer.SendData(stateChangeDto, true);
                        }
                        else
                        {
                            var triggeredDto = new EventTriggeredDto
                            {
                                boneType = (uint)0,
                                id = eventDto.id,
                                toolId = dto.id,
                                hoveredObjectId = 0
                            };
                            UMI3DClientServer.SendData(triggeredDto, true);
                        }
                    });
                    result = e;
                    break;
                case BooleanParameterDto booleanParameterDto:
                    var b = new BooleanInputMenuItem() { dto = booleanParameterDto };
                    b.GetParameterFunc = (x) =>
                    {
                        booleanParameterDto.value = x;
                        var pararmeterDto = new ParameterSettingRequestDto()
                        {
                            toolId = dto.id,
                            id = booleanParameterDto.id,
                            parameter = booleanParameterDto,
                            hoveredObjectId = 0
                        };
                        return pararmeterDto;
                    };
                    b.Subscribe((x) =>
                        UMI3DClientServer.SendData(b.GetParameter(), true)

                    );
                    result = b;
                    break;
                case FloatRangeParameterDto floatRangeParameterDto:
                    var f = new FloatRangeInputMenuItem() { dto = floatRangeParameterDto, max = floatRangeParameterDto.max, min = floatRangeParameterDto.min, value = floatRangeParameterDto.value, increment = floatRangeParameterDto.increment };
                    f.GetParameterFunc = (x) =>
                    {
                        floatRangeParameterDto.value = x;
                        var pararmeterDto = new ParameterSettingRequestDto()
                        {
                            toolId = dto.id,
                            id = floatRangeParameterDto.id,
                            parameter = floatRangeParameterDto,
                            hoveredObjectId = 0
                        };
                        return pararmeterDto;
                    };
                    f.Subscribe((x) =>
                        UMI3DClientServer.SendData(f.GetParameter(), true)
                    );
                    result = f;
                    break;
                case EnumParameterDto<string> enumParameterDto:
                    var en = new DropDownInputMenuItem() { dto = enumParameterDto, options = enumParameterDto.possibleValues };
                    en.GetParameterFunc = (x) =>
                    {
                        enumParameterDto.value = x;
                        var pararmeterDto = new ParameterSettingRequestDto()
                        {
                            toolId = dto.id,
                            id = enumParameterDto.id,
                            parameter = enumParameterDto,
                            hoveredObjectId = 0
                        };
                        return pararmeterDto;
                    };
                    en.Subscribe((x) =>
                        UMI3DClientServer.SendData(en.GetParameter(), true)
                    );
                    result = en;
                    break;
                case UploadFileParameterDto uploadParameterDto:
                    var u = new UploadInputMenuItem() { dto = uploadParameterDto, authorizedExtensions = uploadParameterDto.authorizedExtensions };
                    u.GetParameterFunc = (x) =>
                    {
                        uploadParameterDto.value = x;
                        var pararmeterDto = new UploadFileRequestDto()
                        {
                            toolId = dto.id,
                            id = uploadParameterDto.id,
                            parameter = uploadParameterDto,
                            hoveredObjectId = 0,
                            fileId = FileUploader.AddFileToUpload(x)
                        };
                        return pararmeterDto;
                    };
                    u.Subscribe((x) =>
                        UMI3DClientServer.SendData(u.GetParameter(), true)
                    );
                    result = u;
                    break;
                case StringParameterDto stringParameterDto:
                    var s = new TextInputMenuItem() { dto = stringParameterDto };
                    s.GetParameterFunc = (x) =>
                    {
                        stringParameterDto.value = x;
                        var pararmeterDto = new ParameterSettingRequestDto()
                        {
                            toolId = dto.id,
                            id = stringParameterDto.id,
                            parameter = stringParameterDto,
                            hoveredObjectId = 0
                        };
                        return pararmeterDto;
                    };
                    s.Subscribe((x) =>
                        UMI3DClientServer.SendData(s.GetParameter(), true)
                    );
                    result = s;
                    break;
                case FormDto formDto:
                    var form = new FormMenuItem() { dto = formDto };
                    form.Subscribe((x) =>
                    {
                        var FormAnswer = new FormAnswerDto()
                        {
                            toolId = dto.id,
                            id = formDto.id,
                            answers = x,
                            hoveredObjectId = 0
                        };
                        UMI3DClientServer.SendData(FormAnswer, true);
                    }
                    );
                    result = form;
                    break;
                default:
                    Debug.LogWarning($"Unknown Menu Item for {dto}");
                    result = new MenuItem();
                    result.Subscribe(() => Debug.Log("Unknown case."));
                    break;
            }
            result.Name = dto.name;
            //icon;
            return result;
        }


    }
}
