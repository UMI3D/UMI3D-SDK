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

using System.Collections.Generic;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Centralise volume primitive management.
    /// </summary>
    public class VolumePrimitiveManager : Singleton<VolumePrimitiveManager>
    {
        public Dictionary<string, AbstractPrimitive> primitives = new Dictionary<string, AbstractPrimitive>();

        public void CreatePrimitive(AbstractPrimitiveDto dto, UnityAction<AbstractVolumeCell> finished)
        {
            switch (dto)
            {
                case BoxDto boxDto:
                    Box box = new Box()
                    {
                        id = boxDto.id,
                        bounds = new Bounds()
                        {
                            center = boxDto.center,
                            size = boxDto.size
                        }
                    };
                    primitives.Add(boxDto.id, box);
                    break;
                default:
                    throw new System.Exception("Unknown primitive type !");
            }
        }

        public void DeletePrimitive(string id)
        {
            if (primitives.TryGetValue(id, out AbstractPrimitive prim))
            {
                prim.Delete();
                primitives.Remove(id);
            }
            else
            {
                throw new System.Exception("No primitive found with this id");
            }
        }

        public AbstractPrimitive GetPrimitive(string id)
        {
            return primitives[id];
        }

        public List<AbstractPrimitive> GetPrimitives()
        {
            return new List<AbstractPrimitive>(primitives.Values);
        }
    }
}