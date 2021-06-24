using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.volumes
{
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