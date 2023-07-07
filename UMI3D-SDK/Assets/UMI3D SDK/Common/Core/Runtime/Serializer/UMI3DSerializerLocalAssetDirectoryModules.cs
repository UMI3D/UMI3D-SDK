using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Rendering;


namespace umi3d.common
{
    public class UMI3DSerializerLocalAssetDirectoryModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(UMI3DLocalAssetDirectoryDto) => true,
                _ => null,
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(UMI3DLocalAssetDirectoryDto):
                    throw new NotImplementedException("need to implement UMI3DLocalAssetDirectoryDto case");
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case UMI3DLocalAssetDirectoryDto c:
                    bytable = UMI3DSerializer.Write(c.name)
                            + UMI3DSerializer.Write(c.path)
                            + UMI3DSerializer.Write(c.metrics.resolution)
                            + UMI3DSerializer.Write(c.metrics.size)
                            + UMI3DSerializer.WriteCollection(c.formats);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}