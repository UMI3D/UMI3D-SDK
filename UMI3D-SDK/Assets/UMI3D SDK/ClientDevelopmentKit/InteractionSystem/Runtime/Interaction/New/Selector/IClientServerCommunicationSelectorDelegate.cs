/*
Copyright 2019 - 2025 Inetum

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

namespace umi3d.cdk.interaction
{
    public interface IClientServerCommunicationSelectorDelegate 
    {
        /// <summary>
        /// Send a browser request to the server.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="reliable">Should the request be reliable? Reliable are more expensive but are always delivered.</param>
        void SendRequest(AbstractBrowserRequestDto dto, bool reliable)
        {
            UMI3DClientServer.SendRequest(dto, true);
        }

        async void Animate(ulong environmentId, ulong animationId)
        {
            if (animationId == 0) { return; }

            UMI3DEntityInstance entityInstance = UMI3DEnvironmentLoader.Instance.TryGetEntityInstance(environmentId, animationId);
            UMI3DAbstractAnimation animation = entityInstance?.Object as UMI3DAbstractAnimation;

            await animation.SetUMI3DProperty(
                new SetUMI3DPropertyData(
                    environmentId,
                    new SetEntityPropertyDto()
                    {
                        entityId = animationId,
                        property = UMI3DPropertyKeys.AnimationPlaying,
                        value = true
                    },
                    entityInstance
                )
            );

            if (animation != null) animation.Start();
        }
    }

    public class ClientServerCommunicationSelectorDelegate : IClientServerCommunicationSelectorDelegate
    {

    }
}