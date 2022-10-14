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
using System.Threading.Tasks;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DAvatarNodeDto"/>.
    /// </summary>
    public class UMI3DAvatarNodeLoader : UMI3DNodeLoader
    {
        /// <summary>
        /// Load an avatar node.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override async Task ReadUMI3DExtension(UMI3DDto dto, GameObject node)
        {
            var nodeDto = dto as UMI3DAbstractNodeDto;
            if (node == null)
            {
                throw (new Umi3dException("dto should be an UMI3DAbstractNodeDto"));
            }

            await base.ReadUMI3DExtension(dto, node);

            if ((dto as UMI3DAvatarNodeDto).userId.Equals(UMI3DClientServer.Instance.GetUserId()))
            {
                UserAvatar ua = node.GetOrAddComponent<UserAvatar>();
                ua.Set(dto as UMI3DAvatarNodeDto);
                UMI3DClientUserTracking.Instance.RegisterEmbd((nodeDto as UMI3DAvatarNodeDto).userId, ua);
            }

        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            switch (property.property)
            {
                case UMI3DPropertyKeys.UserBindings:
                    {
                        UserAvatar embd = UMI3DEnvironmentLoader.GetNode(property.entityId).gameObject.GetComponent<UserAvatar>();
                        if (embd != null)
                        {
                            switch (property)
                            {
                                case SetEntityListAddPropertyDto add:
                                    embd.AddBinding(add.index, property.value as BoneBindingDto);
                                    break;
                                case SetEntityListRemovePropertyDto rem:
                                    embd.RemoveBinding(rem.index);
                                    break;
                                case SetEntityListPropertyDto set:
                                    embd.UpdateBinding(set.index, property.value as BoneBindingDto);
                                    break;
                                default:
                                    embd.SetBindings(property.value as List<BoneBindingDto>);
                                    break;
                            }
                        }
                        else
                        {
                            throw new System.Exception("Internal error");
                        }
                    }
                    break;

                case UMI3DPropertyKeys.ActiveBindings:
                    {
                        UserAvatar embd = UMI3DEnvironmentLoader.GetNode(property.entityId).gameObject.GetComponent<UserAvatar>();
                        if (embd != null)
                        {
                            embd.SetActiveBindings((bool)property.value);
                        }
                        else
                        {
                            throw new System.Exception("Internal error");
                        }
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (base.SetUMI3DProperty(entity, operationId, propertyKey, container)) return true;
            var node = entity as UMI3DNodeInstance;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.UserBindings:
                    {
                        UserAvatar embd = node.gameObject.GetComponent<UserAvatar>();
                        if (embd != null)
                        {
                            int index;
                            BoneBindingDto bone;
                            switch (operationId)
                            {
                                case UMI3DOperationKeys.SetEntityListAddProperty:
                                    index = UMI3DNetworkingHelper.Read<int>(container);
                                    bone = UMI3DNetworkingHelper.Read<BoneBindingDto>(container);
                                    embd.AddBinding(index, bone);
                                    break;
                                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                                    index = UMI3DNetworkingHelper.Read<int>(container);
                                    embd.RemoveBinding(index);
                                    break;
                                case UMI3DOperationKeys.SetEntityListProperty:
                                    index = UMI3DNetworkingHelper.Read<int>(container);
                                    bone = UMI3DNetworkingHelper.Read<BoneBindingDto>(container);
                                    embd.UpdateBinding(index, bone);
                                    break;
                                default:
                                    embd.SetBindings(UMI3DNetworkingHelper.ReadList<BoneBindingDto>(container));
                                    break;
                            }
                        }
                        else
                        {
                            throw new System.Exception("Internal error");
                        }
                    }
                    break;

                case UMI3DPropertyKeys.ActiveBindings:
                    {
                        UserAvatar embd = node.gameObject.GetComponent<UserAvatar>();
                        if (embd != null)
                        {
                            embd.SetActiveBindings(UMI3DNetworkingHelper.Read<bool>(container));
                        }
                        else
                        {
                            throw new System.Exception("Internal error");
                        }
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}