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

using System;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public sealed class Projection 
    {
        public Selector selector {  get; internal set; }
        public Controller controller { get; internal set; }
        public Tool tool { get; internal set; }
        public Interaction interaction { get; internal set; }
        public Input input { get; internal set; }

        internal Action _clear;

        IClientServerCommunicationSelectorDelegate _clientServerCommunicationSelectorDelegate;
        public IClientServerCommunicationSelectorDelegate clientServerCommunicationSelectorDelegate
        {
            get => _clientServerCommunicationSelectorDelegate;
            set
            {
                if (value == null)
                {
                    _clientServerCommunicationSelectorDelegate = new ClientServerCommunicationSelectorDelegate();
                    return;
                }
                _clientServerCommunicationSelectorDelegate = value;
            }
        }

        internal Projection(Selector selector, Controller controller, Tool tool, Interaction interaction, Input input)
        {
            this.selector = selector;
            this.controller = controller;
            this.tool = tool;
            this.interaction = interaction;
            this.input = input;

            clientServerCommunicationSelectorDelegate = selector.clientServerCommunicationSelectorDelegate;
        }

        public void SendEventStateChanged(bool value)
        {
            IBoneRepresentable bone = selector.boneRepresentable;

            var request = new EventStateChangedDto
            {
                active = value,

                environmentId = interaction.environmentId,
                toolId = tool.dto.id,
                id = interaction.dto.id,
                hoveredObjectId = selector.hoveredObjectId,

                boneType = bone.bone,
                bonePosition = bone.bonePosition.Dto(),
                boneRotation = new Vector4(bone.boneRotation.x, bone.boneRotation.y, bone.boneRotation.z, bone.boneRotation.w).Dto(),
            };

            clientServerCommunicationSelectorDelegate.SendRequest(request, true);
        }

        public void SendEventTriggered()
        {
            IBoneRepresentable bone = selector.boneRepresentable;

            var request = new EventTriggeredDto
            {
                environmentId = interaction.environmentId,
                toolId = tool.dto.id,
                id = interaction.dto.id,
                hoveredObjectId = selector.hoveredObjectId,

                boneType = bone.bone,
                bonePosition = bone.bonePosition.Dto(),
                boneRotation = new Vector4(bone.boneRotation.x, bone.boneRotation.y, bone.boneRotation.z, bone.boneRotation.w).Dto(),
            };

            clientServerCommunicationSelectorDelegate.SendRequest(request, true);
        }

        public void SendDrawing(bool drawingEnd)
        {
            IBoneRepresentable bone = selector.boneRepresentable;

            var request = new DrawingDto
            {
                environmentId = interaction.environmentId,
                toolId = tool.dto.id,
                id = interaction.dto.id,
                hoveredObjectId = selector.hoveredObjectId,

                drawingEnd = drawingEnd,
                //clientLineId = 0, // TODO
                //clientDrawingId = 0,
                //surfaceId = 0,
                //positions = null,

                boneType = bone.bone,
                bonePosition = bone.bonePosition.Dto(),
                boneRotation = new Vector4(bone.boneRotation.x, bone.boneRotation.y, bone.boneRotation.z, bone.boneRotation.w).Dto(),
            };

            clientServerCommunicationSelectorDelegate.SendRequest(request, true);
        }

        public void SendParameterSetting()
        {
            IBoneRepresentable bone = selector.boneRepresentable;

            var request = new ParameterSettingRequestDto
            {
                environmentId = interaction.environmentId,
                toolId = tool.dto.id,
                id = interaction.dto.id,
                hoveredObjectId = selector.hoveredObjectId,

                parameter = interaction.dto,

                boneType = bone.bone,
                bonePosition = bone.bonePosition.Dto(),
                boneRotation = new Vector4(bone.boneRotation.x, bone.boneRotation.y, bone.boneRotation.z, bone.boneRotation.w).Dto(),
            };

            clientServerCommunicationSelectorDelegate.SendRequest(request, true);
        }

        public void SendUploadFile(string fileId)
        {
            IBoneRepresentable bone = selector.boneRepresentable;

            var request = new UploadFileRequestDto
            {
                environmentId = interaction.environmentId,
                toolId = tool.dto.id,
                id = interaction.dto.id,
                hoveredObjectId = selector.hoveredObjectId,

                fileId = fileId,
                parameter = interaction.dto,

                boneType = bone.bone,
                bonePosition = bone.bonePosition.Dto(),
                boneRotation = new Vector4(bone.boneRotation.x, bone.boneRotation.y, bone.boneRotation.z, bone.boneRotation.w).Dto(),
            };

            clientServerCommunicationSelectorDelegate.SendRequest(request, true);
        }

        public void SendLinkOpened()
        {
            IBoneRepresentable bone = selector.boneRepresentable;

            var request = new LinkOpened
            {
                environmentId = interaction.environmentId,
                toolId = tool.dto.id,
                id = interaction.dto.id,
                hoveredObjectId = selector.hoveredObjectId,

                boneType = bone.bone,
                bonePosition = bone.bonePosition.Dto(),
                boneRotation = new Vector4(bone.boneRotation.x, bone.boneRotation.y, bone.boneRotation.z, bone.boneRotation.w).Dto(),
            };

            clientServerCommunicationSelectorDelegate.SendRequest(request, true);
        }

        public void SendManipulation(Vector3 translation, Vector4 rotation)
        {
            IBoneRepresentable bone = selector.boneRepresentable;

            var request = new ManipulationRequestDto
            {
                environmentId = interaction.environmentId,
                toolId = tool.dto.id,
                id = interaction.dto.id,
                hoveredObjectId = selector.hoveredObjectId,

                translation = translation.Dto(),
                rotation = rotation.Dto(),

                boneType = bone.bone,
                bonePosition = bone.bonePosition.Dto(),
                boneRotation = new Vector4(bone.boneRotation.x, bone.boneRotation.y, bone.boneRotation.z, bone.boneRotation.w).Dto(),
            };

            clientServerCommunicationSelectorDelegate.SendRequest(request, true);
        }

        public void Animate(ulong animationId)
        {
            clientServerCommunicationSelectorDelegate.Animate(tool.environmentId, animationId);
        }

        public void Clear()
        {
            try
            {
                _clear?.Invoke();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public string debugDescription
        {
            get
            {
                string result = "---- Projection ----\n";
                result += $"{selector?.id ?? "No selector"}, {controller?.id ?? "No controller"}, ";
                result += $"{tool?.dto?.name ?? "No tool"}, {interaction?.dto?.name ?? "No interaction"}, ";
                result += $"{input?.inputSystem?.id ?? "No input"}\n";
                result += "\n";

                return result;
            }
        }
    }
}