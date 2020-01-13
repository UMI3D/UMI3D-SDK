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
using umi3d.common;

namespace umi3d.cdk.menu.core
{
    public class InteractionMenuItem : MenuItem
    {
        public virtual AbstractInteractionDto interaction { get; set; }
        
        public bool isProjected()
        {
            if ((interaction == null) || (interaction.Id == null))
                return false;

            if (!AbstractInteractionMapper.Instance.IsToolSelected(interaction.ToolId))
                return false;

            AbstractController controller = AbstractInteractionMapper.Instance.GetController(interaction.ToolId);
            if (controller != null)
            {
                return controller.inputs.Exists(input => (input.CurrentInteraction() != null) ? input.CurrentInteraction().Id.Equals(interaction.Id) : false);
            }
            else
            {
                return false;
            }
        }
    }
}