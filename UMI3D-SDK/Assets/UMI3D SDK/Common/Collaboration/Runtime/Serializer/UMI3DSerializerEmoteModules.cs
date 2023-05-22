using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Rendering;


namespace umi3d.common.collaboration
{
    public class UMI3DSerializerEmoteModules : UMI3DSerializerModule
    {
        public override bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(EmoteRequestDto) => true,
                _ => null,
            };
        }

        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                
            }

            result = default(T);
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case EmoteRequestDto v:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.EmoteRequest)
                        + UMI3DSerializer.Write(v.emoteId)
                        + UMI3DSerializer.Write(v.shouldTrigger);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}