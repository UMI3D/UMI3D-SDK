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

using umi3d.cdk.interaction;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.menu.interaction
{
    /// <summary>
    /// <see cref="Menu"/> for <see cref="GlobalTool"/>.
    /// </summary>
    public class GlobalToolMenu : Menu
    {
        /// <summary>
        /// Parent <see cref="Menu"/>
        /// </summary>
        public AbstractMenu parent;

        /// <summary>
        /// <see cref="GlobalTool"/> associated with the menu.
        /// </summary>
        public GlobalTool tool { get; private set; }

        /// <summary>
        /// Set up the menu for the global tool.
        /// </summary>
        /// <param name="tool"></param>
        public async void Setup(GlobalTool tool)
        {
            RemoveAll();
            this.tool = tool;
            Name = tool.name;
            icon2D = new Texture2D(0, 0);

            if (tool.icon2D != null)
            {
                FileDto icon2DFile = UMI3DEnvironmentLoader.Parameters.ChooseVariant(tool.icon2D.variants);

                if ((icon2DFile != null) && (icon2DFile.url != null) && (icon2DFile.url != ""))
                {
                    var rawData = await UMI3DResourcesManager.GetFile(icon2DFile.url);
                    this.icon2D.LoadRawTextureData(rawData);
                }
            }

            foreach(var interaction in tool.interactions)
            {
                var inter = await interaction;
                this.Add(GlobalToolMenuManager.GetMenuForInteraction(inter, tool.id));
            }
        }
    }
}