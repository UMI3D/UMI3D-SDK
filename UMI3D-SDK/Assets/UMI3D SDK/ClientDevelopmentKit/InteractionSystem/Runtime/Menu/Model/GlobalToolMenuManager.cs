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

using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.cdk.menu;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public class GlobalToolMenuManager : Singleton<GlobalToolMenuManager>
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
    }
}