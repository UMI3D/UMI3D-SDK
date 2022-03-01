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

using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public class GlobalToolMenuManager : inetum.unityUtils.SingleBehaviour<GlobalToolMenuManager>
    {
        public MenuAsset menuAsset;

        /// <summary>
        /// Menus needing to be stored into menuAsset.menu but missing their parent (not recieved yet).
        /// </summary>
        private List<AbstractMenuItem> menuToStoreInMenuAsset = new List<AbstractMenuItem>();

        private Dictionary<ulong, AbstractMenuItem> toolboxIdToMenu = new Dictionary<ulong, AbstractMenuItem>();

        private void Start()
        {
            UMI3DGlobalToolLoader.SubscribeToGlobalToolCreation(OnToolCreation);
            UMI3DGlobalToolLoader.SubscribeToGlobalToolUpdate(OnToolUpdate);
            UMI3DGlobalToolLoader.SubscribeToGlobalToolDelete(OnToolDelete);
        }

        void OnToolCreation(GlobalTool tool)
        {
            if (tool is Toolbox)
            {
                ToolboxMenu tbmenu = new ToolboxMenu();
                tbmenu.Setup(tool as Toolbox);

                ToolboxDto dto = tool.dto as ToolboxDto;
                if (dto.isInsideToolbox)
                {
                    if (toolboxIdToMenu.ContainsKey(dto.toolboxId))
                    {
                        ToolboxMenu parentMenu = (toolboxIdToMenu[dto.toolboxId] as ToolboxMenu);
                        parentMenu.Add(tbmenu);
                        tbmenu.parent = parentMenu;
                        toolboxIdToMenu.Add(dto.id, tbmenu);
                    }
                    else
                    {
                        menuToStoreInMenuAsset.Add(tbmenu);
                        toolboxIdToMenu.Add(dto.id, tbmenu);
                    }
                }
                else
                {
                    menuAsset.menu.Add(tbmenu);
                    tbmenu.parent = menuAsset.menu;
                    toolboxIdToMenu.Add(dto.id, tbmenu);
                }

                foreach(AbstractMenuItem menu in new List<AbstractMenuItem>(menuToStoreInMenuAsset))
                {
                    GlobalToolMenu gtm = menu as GlobalToolMenu;
                    if ((gtm != null) && ((gtm.tool.dto as GlobalToolDto).toolboxId == dto.id))
                    {
                        tbmenu.Add(gtm);
                        gtm.parent = tbmenu;
                        menuToStoreInMenuAsset.Remove(menu);
                    }

                    ToolboxMenu tbm = menu as ToolboxMenu;
                    if ((tbm != null) && ((tbm.toolbox.dto as ToolboxDto).toolboxId == dto.id))
                    {
                        tbmenu.Add(tbm);
                        tbm.parent = tbmenu;
                        menuToStoreInMenuAsset.Remove(menu);
                    }
                }
            }
            else
            {
                GlobalToolMenu menu = new GlobalToolMenu();
                menu.Setup(tool);
                GlobalToolDto dto = tool.dto as GlobalToolDto;
                if (dto.isInsideToolbox)
                {
                    if (toolboxIdToMenu.ContainsKey(dto.toolboxId))
                    {
                        ToolboxMenu parentMenu = toolboxIdToMenu[dto.toolboxId] as ToolboxMenu;
                        parentMenu.Add(menu);
                        menu.parent = parentMenu;
                    }
                    else
                    {
                        menuToStoreInMenuAsset.Add(menu);
                    }
                }
                else
                {
                    menuAsset.menu.Add(menu);
                    menu.parent = menuAsset.menu;
                }
            }
        }

        void OnToolUpdate(GlobalTool tool)
        {
            if (tool is Toolbox)
            {
                ToolboxMenu tbmenu = toolboxIdToMenu[tool.id] as ToolboxMenu;
                tbmenu.Setup(tool as Toolbox);
            }
            else
            {
                GlobalToolMenu gtmenu = toolboxIdToMenu[tool.id] as GlobalToolMenu;
                gtmenu.Setup(tool);
            }
        }

        void OnToolDelete(GlobalTool tool)
        {
            if (tool is Toolbox)
            {
                ToolboxMenu tbmenu = toolboxIdToMenu[tool.id] as ToolboxMenu;
                tbmenu.parent.Remove(tbmenu);
            }
            else
            {
                GlobalToolMenu gtmenu = toolboxIdToMenu[tool.id] as GlobalToolMenu;
                gtmenu.parent.Remove(gtmenu); 
            }
        }
    
        public static AbstractMenuItem GetMenuForInteraction(AbstractInteractionDto interactionDto, ulong toolId)
        {
            Texture2D icon2DTex = new Texture2D(0, 0);
            FileDto icon2DFile = UMI3DEnvironmentLoader.Parameters.ChooseVariant(interactionDto.icon2D.variants);
            if ((icon2DFile != null) && (icon2DFile.url != null) && (icon2DFile.url != ""))
            {
                UMI3DResourcesManager.GetFile(
                    icon2DFile.url,
                    rawData => icon2DTex.LoadRawTextureData(rawData),
                    e => Debug.LogError(e));
            }
            

            if (interactionDto is EventDto evt)
                return new EventMenuItem()
                {
                    icon2D = icon2DTex,
                    interaction = evt,
                    toggle = evt.hold,
                    Name = evt.name,
                    toolId = toolId
                };


            if (interactionDto is FormDto form)
                return new FormMenuItem()
                {
                    dto = form,
                    icon2D = icon2DTex,
                    Name = form.name,
                    toolId = toolId,
                    interaction = form
                };

            

            if (interactionDto is LinkDto link)
                throw new System.NotImplementedException(); //todo

            if (interactionDto is AbstractParameterDto param)
                return GetInteractionItem(param).Item1;
            

            throw new System.Exception("Unknown dto !");
        }

        public static (MenuItem, ParameterSettingRequestDto) GetInteractionItem(AbstractInteractionDto dto)
        {
            MenuItem result = null;
            ParameterSettingRequestDto requestDto = null;
            switch (dto)
            {
                case BooleanParameterDto booleanParameterDto:
                    var b = new BooleanInputMenuItem() { dto = booleanParameterDto };
                    b.NotifyValueChange(booleanParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = booleanParameterDto.id,
                        parameter = booleanParameterDto.value,
                        hoveredObjectId = 0
                    };
                    b.Subscribe((x) =>
                    {
                        booleanParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = b;
                    break;
                case FloatRangeParameterDto floatRangeParameterDto:
                    var f = new FloatRangeInputMenuItem() { dto = floatRangeParameterDto, max = floatRangeParameterDto.max, min = floatRangeParameterDto.min, value = floatRangeParameterDto.value, increment = floatRangeParameterDto.increment };
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = floatRangeParameterDto.id,
                        parameter = floatRangeParameterDto.value,
                        hoveredObjectId = 0
                    };
                    f.Subscribe((x) =>
                    {
                        floatRangeParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = f;
                    break;
                case EnumParameterDto<string> enumParameterDto:
                    var en = new DropDownInputMenuItem() { dto = enumParameterDto, options = enumParameterDto.possibleValues };
                    en.NotifyValueChange(enumParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = enumParameterDto.id,
                        parameter = enumParameterDto.value,
                        hoveredObjectId = 0
                    };
                    en.Subscribe((x) =>
                    {
                        enumParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = en;
                    break;
                case StringParameterDto stringParameterDto:
                    var s = new TextInputMenuItem() { dto = stringParameterDto };
                    s.NotifyValueChange(stringParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = stringParameterDto.id,
                        parameter = stringParameterDto.value,
                        hoveredObjectId = 0
                    };
                    s.Subscribe((x) =>
                    {
                        stringParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = s;
                    break;
                case LocalInfoRequestParameterDto localInfoRequestParameterDto:
                    LocalInfoRequestInputMenuItem localReq = new LocalInfoRequestInputMenuItem() { dto = localInfoRequestParameterDto };
                    localReq.NotifyValueChange(localInfoRequestParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = localInfoRequestParameterDto.id,
                        parameter = localInfoRequestParameterDto.value,
                        hoveredObjectId = 0
                    };
                    localReq.Subscribe((x) =>
                    {
                        localInfoRequestParameterDto.value = x;
                        requestDto.parameter = x;
                    }
                    );
                    result = localReq;
                    break;
                default:
                    result = new MenuItem();
                    result.Subscribe(() => Debug.Log($"Missing case for {dto?.GetType()}"));
                    break;
            }
            result.Name = dto.name;
            //icon;
            return (result, requestDto);
        }
    }
}