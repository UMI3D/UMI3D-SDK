﻿/*
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Centralise volume primitive management.
    /// </summary>
    public class VolumePrimitiveManager : inetum.unityUtils.SingleBehaviour<VolumePrimitiveManager>
    {
        private static readonly Dictionary<ulong, AbstractPrimitive> primitives = new Dictionary<ulong, AbstractPrimitive>();

        private class PrimitiveEvent : UnityEvent<AbstractVolumeCell> { }
        private static readonly PrimitiveEvent onPrimitiveCreation = new PrimitiveEvent();
        private static readonly PrimitiveEvent onPrimitiveDelete = new PrimitiveEvent();

        /// <summary>
        /// Subscribe an action to a cell reception.
        /// </summary>
        /// <param name="catchUpWithPreviousCells">If true, the action will be called for each already received cells.</param>
        public static void SubscribeToPrimitiveCreation(UnityAction<AbstractVolumeCell> callback, bool catchUpWithPreviousCells)
        {
            onPrimitiveCreation.AddListener(callback);

            if (catchUpWithPreviousCells)
                foreach (AbstractVolumeCell cell in primitives.Values)
                    callback(cell);
        }

        /// <see cref="SubscribeToPrimitiveCreation(UnityAction{AbstractVolumeCell}, bool)"/>
        public static void UnsubscribeToPrimitiveCreation(UnityAction<AbstractVolumeCell> callback)
        {
            onPrimitiveCreation.RemoveListener(callback);
        }

        /// <summary>
        /// Subscribe an action to a cell delete.
        /// </summary>
        public static void SubscribeToPrimitiveDelete(UnityAction<AbstractVolumeCell> callback)
        {
            onPrimitiveDelete.AddListener(callback);
        }

        /// <see cref="SubscribeToPrimitiveDelete(UnityAction{AbstractVolumeCell})"/>
        public static void UnsubscribeToPrimitiveDelete(UnityAction<AbstractVolumeCell> callback)
        {
            onPrimitiveDelete.RemoveListener(callback);
        }

        public static async Task<AbstractVolumeCell> CreatePrimitive(AbstractPrimitiveDto dto)
        {
            Matrix4x4 localToWorldMatrix = UMI3DEnvironmentLoader.GetNode(dto.rootNodeId)?.transform.localToWorldMatrix ?? Matrix4x4.identity;

            switch (dto)
            {
                case BoxDto boxDto:
                    var box = new Box() { id = boxDto.id };
                    box.SetBounds(new Bounds() { center = boxDto.center.Struct(), size = boxDto.size.Struct() });
                    box.RootNodeId = dto.rootNodeId;

                    primitives.Add(boxDto.id, box);
                    box.isTraversable = dto.isTraversable;
                    onPrimitiveCreation.Invoke(box);
                    return (box);
                case CylinderDto cylinderDto:
                    var c = new Cylinder()
                    {
                        id = cylinderDto.id,
                        position = localToWorldMatrix.MultiplyPoint(Vector3.zero),
                        rotation = localToWorldMatrix.rotation,
                        scale = localToWorldMatrix.lossyScale
                    };
                    c.SetRadius(cylinderDto.radius);
                    c.SetHeight(cylinderDto.height);
                    c.RootNodeId = dto.rootNodeId;

                    primitives.Add(dto.id, c);
                    c.isTraversable = dto.isTraversable;
                    onPrimitiveCreation.Invoke(c);
                    return (c);
                default:
                    throw new System.Exception("Unknown primitive type !");
            }
        }

        public static void DeletePrimitive(ulong id)
        {
            if (primitives.TryGetValue(id, out AbstractPrimitive prim))
            {
                prim.Delete();
                primitives.Remove(id);
                onPrimitiveDelete.Invoke(prim);
            }
            else
            {
                throw new System.Exception("No primitive found with this id");
            }
        }

        public static AbstractPrimitive GetPrimitive(ulong id)
        {
            return primitives[id];
        }

        public static List<AbstractPrimitive> GetPrimitives()
        {
            return primitives.Values.ToList();
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (Box box in GetPrimitives().Where(p => p is Box))
            {
                Gizmos.matrix = box.rootNode?.localToWorldMatrix ?? Matrix4x4.identity;
                Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
            }

            foreach (Cylinder cyl in GetPrimitives().Where(c => c is Cylinder))
            {
                Gizmos.matrix = cyl.rootNode?.localToWorldMatrix ?? Matrix4x4.identity;
                Gizmos.DrawWireMesh(GeometryTools.GetCylinder(Vector3.zero, Quaternion.identity, Vector3.one, cyl.radius, cyl.height));
            }
        }
    }
}