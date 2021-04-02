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
using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for Avatar node
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
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {
            var nodeDto = dto as UMI3DAbstractNodeDto;
            if (node == null)
            {
                failed.Invoke("dto should be an  UMI3DAbstractNodeDto");
                return;
            }
            base.ReadUMI3DExtension(dto, node, () =>
            {
                if ((dto as UMI3DAvatarNodeDto).userId.Equals(UMI3DClientServer.Instance.GetId()))
                {
                    UserAvatar ua = node.GetOrAddComponent<UserAvatar>();
                    ua.Set(dto as UMI3DAvatarNodeDto);
                    UMI3DClientUserTracking.Instance.RegisterEmbd((nodeDto as UMI3DAvatarNodeDto).userId, ua);
                }

                finished.Invoke();
            }, (s) => failed.Invoke(s));
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
                            if (property is SetEntityListPropertyDto)
                            {
                                if (property is SetEntityListAddPropertyDto)
                                {
                                    embd.AddBinding((property as SetEntityListAddPropertyDto).index, (property.value as BoneBindingDto));
                                }
                                else if (property is SetEntityListRemovePropertyDto)
                                {
                                    embd.RemoveBinding((property as SetEntityListRemovePropertyDto).index, (property.value as BoneBindingDto));
                                }
                                else
                                {
                                    embd.UpdateBinding((property as SetEntityListPropertyDto).index, (property.value as BoneBindingDto));
                                }
                            }
                            else
                            {
                                embd.SetBindings(property.value as List<BoneBindingDto>);
                            }
                        }
                        else
                            throw new System.Exception("Internal error");
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
                            throw new System.Exception("Internal error");
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}