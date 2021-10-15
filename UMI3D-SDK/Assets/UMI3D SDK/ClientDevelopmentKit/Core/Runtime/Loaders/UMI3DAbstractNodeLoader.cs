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

using MainThreadDispatcher;
using System;
using System.Collections;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// loader for abstract node
    /// </summary>
    public class UMI3DAbstractNodeLoader
    {
        /// <summary>
        /// Load an abstract node dto.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public virtual void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {
            var nodeDto = dto as UMI3DAbstractNodeDto;
            if (node == null)
            {
                failed.Invoke(new Umi3dException(0,"dto should be an  UMI3DAbstractNodeDto"));
                return;
            }
            if (dto != null)
            {

                UMI3DNodeInstance parent = UMI3DEnvironmentLoader.GetNode(nodeDto.pid);
                node.transform.SetParent(parent != null ? parent.transform : UMI3DEnvironmentLoader.Exists ? UMI3DEnvironmentLoader.Instance.transform : null);

                if (parent == null && nodeDto.pid != 0)
                    UnityMainThreadDispatcher.Instance().Enqueue(WaitForParent(nodeDto, node));
                if (node.activeSelf != nodeDto.active)
                    node.SetActive(nodeDto.active);

                if (nodeDto.isStatic != node.isStatic)
                    node.isStatic = nodeDto.isStatic;
                finished?.Invoke();

            }
            else
                finished?.Invoke();
        }

        IEnumerator WaitForParent(UMI3DAbstractNodeDto dto, GameObject node)
        {
            var wait = new WaitForFixedUpdate();
            UMI3DNodeInstance parent = UMI3DEnvironmentLoader.GetNode(dto.pid);
            while ((parent = UMI3DEnvironmentLoader.GetNode(dto.pid)) == null && dto.pid != 0)
            {
                UnityEngine.Debug.Log($"parent not found {dto.pid} [{dto.id}:{node.name}]");
                yield return wait;
            }

            node.transform.SetParent(parent != null ? parent.transform : UMI3DEnvironmentLoader.Exists ? UMI3DEnvironmentLoader.Instance.transform : null, false);

        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public virtual bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;
            UMI3DAbstractNodeDto dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DAbstractNodeDto;
            if (dto == null) dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d as UMI3DAbstractNodeDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.Static:
                    dto.isStatic = (bool)property.value;
                    if (dto.isStatic != node.gameObject.isStatic)
                        node.gameObject.isStatic = dto.isStatic;
                    break;
                case UMI3DPropertyKeys.Active:
                    dto.active = (bool)property.value;
                    if (node.gameObject.activeSelf != dto.active)
                        node.gameObject.SetActive(dto.active);
                    break;
                case UMI3DPropertyKeys.ParentId:
                    ulong pid = dto.pid = (ulong)(long)property.value;
                    UMI3DNodeInstance parent = UMI3DEnvironmentLoader.GetNode(pid);
                    node.transform.SetParent(parent != null ? parent.transform : UMI3DEnvironmentLoader.Exists ? UMI3DEnvironmentLoader.Instance.transform : null);

                    break;
                default:
                    return false;
            }
            return true;
        }

        public virtual bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Static:
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.Active:
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.ParentId:
                    value = UMI3DNetworkingHelper.Read<ulong>(container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public virtual bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;
            UMI3DAbstractNodeDto dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DAbstractNodeDto;
            if (dto == null) dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d as UMI3DAbstractNodeDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Static:
                    dto.isStatic = UMI3DNetworkingHelper.Read<bool>(container);
                    if (dto.isStatic != node.gameObject.isStatic)
                        node.gameObject.isStatic = dto.isStatic;
                    break;
                case UMI3DPropertyKeys.Active:
                    dto.active = UMI3DNetworkingHelper.Read<bool>(container);
                    if (node.gameObject.activeSelf != dto.active)
                        node.gameObject.SetActive(dto.active);
                    break;
                case UMI3DPropertyKeys.ParentId:
                    ulong pid = dto.pid = UMI3DNetworkingHelper.Read<ulong>(container);
                    UMI3DNodeInstance parent = UMI3DEnvironmentLoader.GetNode(pid);
                    node.transform.SetParent(parent != null ? parent.transform : UMI3DEnvironmentLoader.Exists ? UMI3DEnvironmentLoader.Instance.transform : null);

                    break;
                default:
                    return false;
            }
            return true;
        }

    }
}