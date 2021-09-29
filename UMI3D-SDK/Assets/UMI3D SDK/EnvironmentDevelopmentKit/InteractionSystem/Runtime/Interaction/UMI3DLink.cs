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

using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.edk.interaction
{

    public class UMI3DLink : AbstractInteraction
    {
        public string url;


        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public InteractionEvent onLinkUsed = new InteractionEvent();

        ///<inheritdoc/>
        protected override AbstractInteractionDto CreateDto()
        {
            return new LinkDto();
        }

        ///<inheritdoc/>
        protected override void WriteProperties(AbstractInteractionDto dto_, UMI3DUser user)
        {
            base.WriteProperties(dto_, user);
            if (dto_ is LinkDto dto)
            {
                dto.url = url;
            }
        }

        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.Link;
        }


        public override Bytable ToByte(UMI3DUser user)
        {
            return base.ToByte(user)
                + UMI3DNetworkingHelper.Write(url);
        }

        ///<inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case LinkOpened linkOpened:
                    onLinkUsed.Invoke(new InteractionEventContent(user, interactionRequest));
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }

        }

        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            switch (interactionId)
            {
                case UMI3DOperationKeys.LinkOpened:
                    onLinkUsed.Invoke(new InteractionEventContent(user, toolId, interactionId, hoverredId, boneType));
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }
    }
}