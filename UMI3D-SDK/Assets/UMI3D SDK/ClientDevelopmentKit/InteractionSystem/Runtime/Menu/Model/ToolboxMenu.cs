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
    public class ToolboxMenu : Menu
    {
        public AbstractMenu parent;

        public Toolbox toolbox { get; private set; }

        public void Setup(Toolbox toolbox)
        {
            this.toolbox = toolbox;
            Name = toolbox.name;
            if (icon2D != null)
            {
                icon2D = new Texture2D(0, 0);

                FileDto icon2DFile = UMI3DEnvironmentLoader.Parameters.ChooseVariant(toolbox.icon2D.variants);

                if ((icon2DFile.url != null) && (icon2DFile.url != ""))
                {
                    UMI3DResourcesManager.GetFile(
                        icon2DFile.url,
                        rawData => this.icon2D.LoadRawTextureData(rawData),
                        e => Debug.LogError(e));
                }
            }

            toolbox.interactions.ForEach(inter => this.Add(GlobalToolMenuManager.GetMenuForInteraction(inter, toolbox.id)));
        }
    }
}