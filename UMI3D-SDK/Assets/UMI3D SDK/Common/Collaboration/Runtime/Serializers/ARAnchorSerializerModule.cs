using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using umi3d.common.lbe.description;
using System;

namespace umi3d.common.lbe
{
    public class ARAnchorSerializerModule : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return typeof(T) == typeof(ARAnchorDto) ? true : null;
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            if (typeof(T) == typeof(ARAnchorDto))
            {

                if (UMI3DSerializer.TryRead(container, out Vector3Dto position)
                    && UMI3DSerializer.TryRead(container, out Vector4Dto rotation))
                {
                    var arAnchorDto = new ARAnchorDto
                    {
                        position = position,
                        rotation = rotation,
                    };

                    readable = true;
                    result = (T)Convert.ChangeType(arAnchorDto, typeof(T));
                    return true;
                }
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (value is ARAnchorDto c)
            {
                // Ecriture de la liste anchorAR
                bytable = UMI3DSerializer.Write(c.position)
                    + UMI3DSerializer.Write(c.rotation);
                return true;
            }
            bytable = null;
            return false;
        }
    }
}
