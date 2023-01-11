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
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DAvatarNodeDto && base.CanReadUMI3DExtension(data);
        }

        /// <summary>
        /// Load an avatar node for collaborative user.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            var nodeDto = data.dto as UMI3DAvatarNodeDto;
            if (nodeDto == null)
            {
                throw (new Umi3dException("dto should be an UMI3DAvatarNodeDto"));
            }

            await base.ReadUMI3DExtension(data);

            UserAvatar ua = (nodeDto.userId.Equals(UMI3DClientServer.Instance.GetUserId())) ? GetUserAvatar(data) : GetOtherUserAvatar(data);
            ua.Set(nodeDto);
            UMI3DClientUserTracking.Instance.RegisterEmbd(nodeDto.userId, ua);
        }

        public virtual UserAvatar GetUserAvatar(ReadUMI3DExtensionData data)
        {
            return data.node.GetOrAddComponent<UserAvatar>();
        }

        public virtual UserAvatar GetOtherUserAvatar(ReadUMI3DExtensionData data)
        {
            return data.node.GetOrAddComponent<UserAvatar>();
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if (await base.SetUMI3DProperty(data)) 
                return true;
            switch (data.property.property)
            {
                case UMI3DPropertyKeys.UserBindings:
                    {
                        UserAvatar embd = UMI3DEnvironmentLoader.GetNode(data.property.entityId).gameObject.GetComponent<UserAvatar>();
                        if (embd != null)
                        {
                            switch (data.property)
                            {
                                case SetEntityListAddPropertyDto add:
                                    embd.AddBinding(add.index, data.property.value as BoneBindingDto);
                                    break;
                                case SetEntityListRemovePropertyDto rem:
                                    embd.RemoveBinding(rem.index);
                                    break;
                                case SetEntityListPropertyDto set:
                                    embd.UpdateBinding(set.index, data.property.value as BoneBindingDto);
                                    break;
                                default:
                                    embd.SetBindings(data.property.value as List<BoneBindingDto>);
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
                        UserAvatar embd = UMI3DEnvironmentLoader.GetNode(data.property.entityId).gameObject.GetComponent<UserAvatar>();
                        if (embd != null)
                        {
                            embd.SetActiveBindings((bool)data.property.value);
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
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (await base.SetUMI3DProperty(data)) return true;
            var node = data.entity as UMI3DNodeInstance;
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.UserBindings:
                    {
                        UserAvatar embd = node.gameObject.GetComponent<UserAvatar>();
                        if (embd != null)
                        {
                            int index;
                            BoneBindingDto bone;
                            switch (data.operationId)
                            {
                                case UMI3DOperationKeys.SetEntityListAddProperty:
                                    index = UMI3DSerializer.Read<int>(data.container);
                                    bone = UMI3DSerializer.Read<BoneBindingDto>(data.container);
                                    embd.AddBinding(index, bone);
                                    break;
                                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                                    index = UMI3DSerializer.Read<int>(data.container);
                                    embd.RemoveBinding(index);
                                    break;
                                case UMI3DOperationKeys.SetEntityListProperty:
                                    index = UMI3DSerializer.Read<int>(data.container);
                                    bone = UMI3DSerializer.Read<BoneBindingDto>(data.container);
                                    embd.UpdateBinding(index, bone);
                                    break;
                                default:
                                    embd.SetBindings(UMI3DSerializer.ReadList<BoneBindingDto>(data.container));
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
                            embd.SetActiveBindings(UMI3DSerializer.Read<bool>(data.container));
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