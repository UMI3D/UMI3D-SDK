#if !UMI3D_NEW_LABEL
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
using System;
using umi3d.cdk.interaction;
using umi3d.common.interaction;

namespace umi3d.cdk.menu.interaction
{
    [Obsolete("This class or method will be removed when the new label feature will be activated.")]
    /// <summary>
    /// <see cref="AbstractMenuItem"/> for <see cref="AbstractInteractionDto"/>
    /// </summary>
    public class InteractionMenuItem : AbstractMenuItem
    {
        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Interaction that belongs to the menu item.
        /// </summary>
        public virtual AbstractInteractionDto interaction { get; set; }
        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Tool id if the interaciton belongs to one.
        /// </summary>
        public virtual ulong toolId { get; set; }
        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        public virtual ulong environmentId { get; set; }

        [Obsolete("This class or method will be removed when the new label feature will be activated.")]
        /// <summary>
        /// Is the interaction projected on a controller ?
        /// </summary>
        /// <returns></returns>
        public bool isProjected()
        {
            if ((interaction == null) || (interaction.id == 0))
                return false;

            if (!InteractionMapper.Instance.IsToolSelected(environmentId, toolId))
                return false;

            AbstractController controller = InteractionMapper.Instance.GetController(environmentId, toolId);
            if (controller != null)
            {
                return controller.inputs.Exists(input => (input.CurrentInteraction() != null) && input.CurrentInteraction().id.Equals(interaction.id));
            }
            else
            {
                return false;
            }
        }
    }
}
#endif