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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Helper class that manages the loading of <see cref="Interactable"/> entities.
    /// </summary>
    public class UMI3DInteractionLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is AbstractInteractionDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as AbstractInteractionDto;

            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();

        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity.dto is AbstractInteractionDto abstractDto)
            {
                if(value.property.property == UMI3DPropertyKeys.Interaction_UI_Link)
                {
                    abstractDto.uiLinkId = (ulong)value.property.value;
                    UnityEngine.Debug.Log("TODO need to update UI link");
                    return true;
                }

                //try to read commun value
                switch (value.entity.dto)
                {
                    case EventDto dto:
                        {
                            switch (value.property.property)
                            {
                                case UMI3DPropertyKeys.EventTriggerAnimation:
                                    dto.TriggerAnimationId = (ulong)value.property.value;
                                    break;
                                case UMI3DPropertyKeys.EventReleaseAnimation:
                                    dto.ReleaseAnimationId = (ulong)value.property.value;
                                    break;
                                default:
                                    return false;
                            }
                            return true;
                        }
                    default:
                        return false;
                }
            }
            return false;
        }


        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity.dto is AbstractInteractionDto abstractDto)
            {
                if(value.propertyKey == UMI3DPropertyKeys.Interaction_UI_Link)
                {
                    abstractDto.uiLinkId = UMI3DSerializer.Read<ulong>(value.container);
                    UnityEngine.Debug.Log("TODO need to update UI link");
                    return true;
                }

                //try to read commun value
                switch (value.entity?.dto)
                {
                    case EventDto dto:
                        switch (value.propertyKey)
                        {
                            case UMI3DPropertyKeys.EventTriggerAnimation:
                                dto.TriggerAnimationId = UMI3DSerializer.Read<ulong>(value.container);
                                break;
                            case UMI3DPropertyKeys.EventReleaseAnimation:
                                dto.TriggerAnimationId = UMI3DSerializer.Read<ulong>(value.container);
                                break;
                            default:
                                return false;
                        }

                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Reads the value of an unknown <see cref="object"/> based on a received <see cref="ByteContainer"/> and updates it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="value">Unknown object</param>
        /// <param name="propertyKey">Property to update key in <see cref="UMI3DPropertyKeys"/></param>
        /// <param name="container">Received byte container</param>
        /// <returns>True if property setting was successful</returns>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            return false;
        }



        /// <summary>
        /// Read an <see cref="AbstractInteractionDto"/> from a received <see cref="ByteContainer"/>.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="container">Byte container</param>
        /// <param name="readable">True if the byte container has been correctly read.</param>
        /// <returns></returns>
        public static AbstractInteractionDto ReadAbstractInteractionDto(ByteContainer container, out bool readable)
        {
            AbstractInteractionDto interaction;
            byte interactionCase = UMI3DSerializer.Read<byte>(container);
            switch (interactionCase)
            {
                case UMI3DInteractionKeys.Event:
                    var Event = new EventDto();
                    ReadAbstractInteractionDto(Event, container);
                    Event.hold = UMI3DSerializer.Read<bool>(container);
                    Event.TriggerAnimationId = UMI3DSerializer.Read<ulong>(container);
                    Event.ReleaseAnimationId = UMI3DSerializer.Read<ulong>(container);
                    interaction = Event;
                    break;
                case UMI3DInteractionKeys.Manipulation:
                    var Manipulation = new ManipulationDto();
                    ReadAbstractInteractionDto(Manipulation, container);
                    Manipulation.frameOfReference = UMI3DSerializer.Read<ulong>(container);
                    Manipulation.dofSeparationOptions = UMI3DSerializer.ReadList<DofGroupOptionDto>(container);
                    interaction = Manipulation;
                    break;
                case UMI3DInteractionKeys.Form:
                    var Form = new FormDto();
                    ReadAbstractInteractionDto(Form, container);
                    Form.fields = UMI3DSerializer.ReadList<ulong>(container);
                    interaction = Form;
                    break;
                case UMI3DInteractionKeys.Link:
                    var Link = new LinkDto();
                    ReadAbstractInteractionDto(Link, container);
                    Link.url = UMI3DSerializer.Read<string>(container);
                    interaction = Link;
                    break;
                case UMI3DInteractionKeys.BooleanParameter:
                    var Bool = new BooleanParameterDto();
                    ReadAbstractParameterDto(Bool, container);
                    Bool.value = UMI3DSerializer.Read<bool>(container);
                    interaction = Bool;
                    break;
                case UMI3DInteractionKeys.ColorParameter:
                    var Color = new ColorParameterDto();
                    ReadAbstractParameterDto(Color, container);
                    Color.value = UMI3DSerializer.Read<ColorDto>(container);
                    interaction = Color;
                    break;
                case UMI3DInteractionKeys.Vector2Parameter:
                    var Vector2 = new Vector2ParameterDto();
                    ReadAbstractParameterDto(Vector2, container);
                    Vector2.value = UMI3DSerializer.Read<Vector2Dto>(container);
                    interaction = Vector2;
                    break;
                case UMI3DInteractionKeys.Vector3Parameter:
                    var Vector3 = new Vector3ParameterDto();
                    ReadAbstractParameterDto(Vector3, container);
                    Vector3.value = UMI3DSerializer.Read<Vector3Dto>(container);
                    interaction = Vector3;
                    break;
                case UMI3DInteractionKeys.Vector4Parameter:
                    var Vector4 = new Vector4ParameterDto();
                    ReadAbstractParameterDto(Vector4, container);
                    Vector4.value = UMI3DSerializer.Read<Vector4Dto>(container);
                    interaction = Vector4;
                    break;
                case UMI3DInteractionKeys.UploadParameter:
                    var Upload = new UploadFileParameterDto();
                    ReadAbstractParameterDto(Upload, container);
                    Upload.value = UMI3DSerializer.Read<string>(container);
                    Upload.authorizedExtensions = UMI3DSerializer.Read<List<string>>(container);
                    interaction = Upload;
                    break;
                case UMI3DInteractionKeys.StringParameter:
                    var String = new StringParameterDto();
                    ReadAbstractParameterDto(String, container);
                    String.value = UMI3DSerializer.Read<string>(container);
                    interaction = String;
                    break;
                case UMI3DInteractionKeys.LocalInfoParameter:
                    var LocalInfo = new LocalInfoRequestParameterDto();
                    ReadAbstractParameterDto(LocalInfo, container);
                    LocalInfo.app_id = UMI3DSerializer.Read<string>(container);
                    LocalInfo.serverName = UMI3DSerializer.Read<string>(container);
                    LocalInfo.reason = UMI3DSerializer.Read<string>(container);
                    LocalInfo.key = UMI3DSerializer.Read<string>(container);
                    LocalInfo.value = new LocalInfoRequestParameterValue(UMI3DSerializer.Read<bool>(container), UMI3DSerializer.Read<bool>(container));
                    interaction = LocalInfo;
                    break;
                case UMI3DInteractionKeys.StringEnumParameter:
                    var EString = new EnumParameterDto<string>();
                    ReadAbstractParameterDto(EString, container);
                    EString.possibleValues = UMI3DSerializer.ReadList<string>(container);
                    EString.value = UMI3DSerializer.Read<string>(container);
                    interaction = EString;
                    break;
                case UMI3DInteractionKeys.FloatRangeParameter:
                    var RFloat = new FloatRangeParameterDto();
                    ReadAbstractParameterDto(RFloat, container);
                    RFloat.min = UMI3DSerializer.Read<float>(container);
                    RFloat.max = UMI3DSerializer.Read<float>(container);
                    RFloat.increment = UMI3DSerializer.Read<float>(container);
                    RFloat.value = UMI3DSerializer.Read<float>(container);
                    interaction = RFloat;
                    break;
                default:
                    interaction = null;
                    readable = false;
                    return null;
            }
            readable = true;
            return interaction;
        }

        /// <summary>
        /// Reads an <see cref="AbstractInteractionDto"/> from a received <see cref="ByteContainer"/> and updates it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        ///  <param name="interactionDto">interaction dto to update.</param>
        /// <param name="container">Byte container</param>
        /// <returns></returns>
        private static void ReadAbstractInteractionDto(AbstractInteractionDto interactionDto, ByteContainer container)
        {
            interactionDto.id = UMI3DSerializer.Read<ulong>(container);
            interactionDto.name = UMI3DSerializer.Read<string>(container);
            interactionDto.icon2D = UMI3DSerializer.Read<ResourceDto>(container);
            interactionDto.icon3D = UMI3DSerializer.Read<ResourceDto>(container);
            interactionDto.description = UMI3DSerializer.Read<string>(container);

        }

        /// <summary>
        /// Reads an <see cref="AbstractParameterDto"/> from a received <see cref="ByteContainer"/> and updates it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        ///  <param name="ParameterDto">interaction dto to update.</param>
        /// <param name="container">Byte container</param>
        /// <returns></returns>
        private static void ReadAbstractParameterDto(AbstractParameterDto ParameterDto, ByteContainer container)
        {
            ReadAbstractParameterDto(ParameterDto, container);
            ParameterDto.privateParameter = UMI3DSerializer.Read<bool>(container);
            ParameterDto.isDisplayer = UMI3DSerializer.Read<bool>(container);
        }
    }
}