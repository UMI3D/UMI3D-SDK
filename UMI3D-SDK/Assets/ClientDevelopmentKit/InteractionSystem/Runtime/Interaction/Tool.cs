/*
Copyright 2019 Gfi Informatique

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

        public static ToolMenuItem IdToMenu(string id) { return (UMI3DEnvironmentLoader.GetEntity(id)?.Object as Tool)?.Menu; }


        public ToolMenuItem Menu;



        /// <summary>
        /// Interactable dto describing this object.
        /// </summary>
        public ToolDto dto;

        protected override AbstractToolDto abstractDto { get => dto; set => dto = value as ToolDto; }

        public Tool(ToolDto dto, Toolbox toolbox) : base(dto)
        {

            Menu = new ToolMenuItem()
            {
                tool = this,
                Name = name,
            };
            foreach(var interaction in dto.interactions)
            {
                var item = getInteractionItem(interaction);
                Menu.Add(item);
            }

            toolbox.tools.Add(this);
            toolbox.sub.Add(Menu);
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
                    e.Subscribe(() => Debug.Log("hellooo"));
                    result = e;
                    break;
                case BooleanParameterDto booleanParameterDto:
                    var b = new BooleanInputMenuItem() { dto = booleanParameterDto };
                    b.Subscribe((x) =>
                        {
                            booleanParameterDto.value = x;
                            var pararmeterDto = new ParameterSettingRequestDto()
                            {
                                toolId = dto.id,
                                id = booleanParameterDto.id,
                                parameter = booleanParameterDto,
                            };
                            UMI3DClientServer.Send(pararmeterDto, true);
                        }
                    );
                    result = b;
                    break;
                case FloatRangeParameterDto floatRangeParameterDto:
                    var f = new FloatRangeInputMenuItem() { dto = floatRangeParameterDto, max = floatRangeParameterDto.Max, min = floatRangeParameterDto.Min, value = floatRangeParameterDto.value, increment = floatRangeParameterDto.Increment };
                    f.Subscribe((x) =>
                    {
                        floatRangeParameterDto.value = x;
                        var pararmeterDto = new ParameterSettingRequestDto()
                        {
                            toolId = dto.id,
                            id = floatRangeParameterDto.id,
                            parameter = floatRangeParameterDto,
                        };
                        UMI3DClientServer.Send(pararmeterDto, true);
                    }
                    );
                    result = f;
                    break;
                case EnumParameterDto<string> enumParameterDto:
                    var en = new DropDownInputMenuItem() { dto = enumParameterDto, options = enumParameterDto.PossibleValues };
                    en.Subscribe((x) =>
                    {
                        enumParameterDto.value = x;
                        var pararmeterDto = new ParameterSettingRequestDto()
                        {
                            toolId = dto.id,
                            id = enumParameterDto.id,
                            parameter = enumParameterDto,
                        };
                        UMI3DClientServer.Send(pararmeterDto, true);
                    }
                    );
                    result = en;
                    break;
                case StringParameterDto stringParameterDto:
                    var s = new TextInputMenuItem() { dto = stringParameterDto};
                    s.Subscribe((x) =>
                    {
                        stringParameterDto.value = x;
                        var pararmeterDto = new ParameterSettingRequestDto()
                        {
                            toolId = dto.id,
                            id = stringParameterDto.id,
                            parameter = stringParameterDto,
                        };
                        UMI3DClientServer.Send(pararmeterDto, true);
                    }
                    );
                    result = s;
                    break;
                default:
                    result = new MenuItem();
                    result.Subscribe(() => Debug.Log("hellooo 2"));
                    break;
            }
            result.Name = dto.name;
            //icon;
            return result;
        }


    }
}