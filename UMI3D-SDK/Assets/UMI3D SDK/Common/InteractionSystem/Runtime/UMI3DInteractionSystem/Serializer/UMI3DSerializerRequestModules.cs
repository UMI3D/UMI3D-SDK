using MathNet.Numerics.Distributions;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.common.interaction
{
    public class UMI3DSerializerRequestModules : UMI3DSerializerModule
    {
        public override bool IsCountable<T>()
        {
            return true;
        }

        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
                case true when typeof(T) == typeof(InteractionRequestDto):
                    throw new NotImplementedException("need to implement InteractionRequestDto case");
                case true when typeof(T) == typeof(NotificationCallbackDto):
                    throw new NotImplementedException("need to implement NotificationCallbackDto case");
                case true when typeof(T) == typeof(ToolProjectedDto):
                    throw new NotImplementedException("need to implement ToolProjectedDto case");
                case true when typeof(T) == typeof(ToolReleasedDto):
                    throw new NotImplementedException("need to implement ToolReleasedDto case");

            }

            result = default(T);
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case ManipulationRequestDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.ManipulationRequest, parameters)
                            + UMI3DSerializer.Write(c.translation ?? new SerializableVector3())
                            + UMI3DSerializer.Write(c.rotation ?? new SerializableVector4());
                    return true;
                case LinkOpened c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.LinkOpened, parameters)
                            ;
                    return true;
                case HoverStateChangedDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.HoverStateChanged, parameters)
                            + UMI3DSerializer.Write(c.position)
                            + UMI3DSerializer.Write(c.normal)
                            + UMI3DSerializer.Write(c.direction)
                            + UMI3DSerializer.Write(c.state);
                    return true;
                case HoveredDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.Hoverred, parameters)
                        + UMI3DSerializer.Write(c.position)
                        + UMI3DSerializer.Write(c.normal)
                        + UMI3DSerializer.Write(c.direction);
                    return true;
                case UploadFileRequestDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.UploadFileRequest, parameters)
                        + UMI3DSerializer.Write(c.parameter)
                        + UMI3DSerializer.Write(c.fileId);
                    return true;
                case ParameterSettingRequestDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.ParameterSettingRequest, parameters)
                        + UMI3DSerializer.Write(c.parameter);
                    return true;
                case FormAnswerDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.FormAnswer, parameters)
                        + UMI3DSerializer.Write(c.answers);
                    return true;
                case EventStateChangedDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.EventStateChanged, parameters) 
                        + UMI3DSerializer.Write(c.active);
                    return true;
                case EventTriggeredDto c:
                    bytable = WriteInteraction(c, UMI3DOperationKeys.EventTriggered, parameters);
                    return true;
                case NotificationCallbackDto c:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.NotificationCallback)
                        + UMI3DSerializer.Write(c.id)
                        + UMI3DSerializer.Write(c.callback);
                    return true;
                case ToolProjectedDto c:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.ToolProjected)
                        + UMI3DSerializer.Write(c.toolId)
                        + UMI3DSerializer.Write(c.boneType);
                    return true;
                case ToolReleasedDto c:
                    bytable = UMI3DSerializer.Write(UMI3DOperationKeys.ToolReleased)
                        + UMI3DSerializer.Write(c.toolId)
                        + UMI3DSerializer.Write(c.boneType);
                    return true;
            }
            bytable = null;
            return false;
        }

        Bytable WriteInteraction(InteractionRequestDto c,uint? operation, params object[] parameters)
        {
            return UMI3DSerializer.Write(operation ?? UMI3DOperationKeys.InteractionRequest)
                        + UMI3DSerializer.Write(c.toolId)
                        + UMI3DSerializer.Write(c.id)
                        + UMI3DSerializer.Write(c.hoveredObjectId)
                        + UMI3DSerializer.Write(c.boneType)
                        + UMI3DSerializer.Write(c.bonePosition)
                        + UMI3DSerializer.Write(c.boneRotation);
        }

    }
}
