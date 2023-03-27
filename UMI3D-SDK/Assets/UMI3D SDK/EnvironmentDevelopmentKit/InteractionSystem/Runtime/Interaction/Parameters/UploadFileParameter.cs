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
using System.IO;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// File parameter that could be uploaded to the environment server.
    /// </summary>
    public class UploadFileParameter : AbstractParameter
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Interaction;

        public static Dictionary<string, UploadFileParameter> uploadTokens { get; private set; } = new Dictionary<string, UploadFileParameter>();

        /// <summary>
        /// The path of the folder where the file will be saved
        /// </summary>
        public string pathToSaveFile = "./TMP/";

        /// <summary>
        /// Current input value.
        /// </summary>
        public string value;

        /// <summary>
        /// Only these extensions could be upload by the client. Enpty list = allow all, the extensions contain a dot (".obj" for exemple)
        /// </summary>
        public List<string> authorizedExtensions = new List<string>();

        [System.Serializable]
        public class UploadListener : ParameterEvent<(string, string)> { }


        /// <summary>
        /// Event raised on value change. string arg is the file name
        /// </summary>
        public UploadListener onChange = new UploadListener();


        [System.Serializable]
        public class SaveFileListener : UnityEvent<string> { }
        /// <summary>
        /// Event raised on value change. string arg is the file path on server
        /// </summary>
        public SaveFileListener onSave = new SaveFileListener();

        /// <summary>
        /// Event class with token, fileName, file
        /// </summary>
        [System.Serializable]
        public class ReceiveListener : UnityEvent<string, string, byte[]> { }

        /// <summary>
        /// Event raised when file is received.
        /// </summary>
        public ReceiveListener onReceive = new ReceiveListener();

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractInteractionDto CreateDto()
        {
            return new UploadFileParameterDto();
        }

        /// <inheritdoc/>
        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.UploadParameter;
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case UploadFileRequestDto settingRequestDto:
                    if (settingRequestDto.parameter is UploadFileParameterDto)
                    {
                        var parameter = settingRequestDto.parameter as UploadFileParameterDto;
                        //value = parameter.value;
                        // if(System.IO.File.Exists(parameter.value))
                        // {
                        string ext = System.IO.Path.GetExtension(parameter.value);
                        if (!string.IsNullOrEmpty(ext))
                        {
                            if (authorizedExtensions.Count == 0 || authorizedExtensions.Contains(ext))
                            {
                                onChange.Invoke(new ParameterEventContent<(string, string)>(user, settingRequestDto, (parameter.value, settingRequestDto.fileId)));
                            }
                            else
                            {
                                UMI3DLogger.LogWarning("Unauthorized extension : " + ext, scope);
                            }
                        }
                        else
                        {
                            UMI3DLogger.LogWarning("unvalide extension", scope);
                        }

                        /*    }
                            else
                            {
                                UMI3DLogger.LogWarning("Unvalide path, this file doesn't exist or is not accessible : " + parameter.value);
                            }
                            */
                    }
                    else
                    {
                        throw new System.Exception($"parameter of type {settingRequestDto.parameter.GetType()}");
                    }

                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, SerializableVector3 bonePosition, SerializableVector4 boneRotation, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.ParameterSettingRequest:

                    uint parameterId = UMI3DSerializer.Read<uint>(container);
                    if (UMI3DParameterKeys.StringUploadFile == parameterId)
                    {
                        UMI3DSerializer.Read<bool>(container);
                        value = UMI3DSerializer.Read<string>(container);
                        List<string> exts = UMI3DSerializer.ReadList<string>(container);
                        string fileId = UMI3DSerializer.Read<string>(container);
                        //authorizedExtensions = UMI3DSerializer.ReadList<string>(container);
                        //UnityEngine.UMI3DLogger.Log(value);
                        //if (System.IO.File.Exists(value))
                        //{
                        string ext = System.IO.Path.GetExtension(value);
                        if (!string.IsNullOrEmpty(ext))
                        {
                            if (authorizedExtensions.Count == 0 || authorizedExtensions.Contains(ext))
                            {

                                onChange.Invoke(new ParameterEventContent<(string, string)>(user, toolId, interactionId, hoverredId, boneType, bonePosition, boneRotation, (value, fileId)));
                            }
                            else
                            {
                                UMI3DLogger.LogWarning("Unauthorized extension : " + ext, scope);
                            }
                        }
                        else
                        {
                            UMI3DLogger.LogWarning("unvalide extension", scope);
                        }

                        /*    }
                            else
                            {
                                UMI3DLogger.LogWarning("Unvalide path, this file doesn't exist or is not accessible : " + value,scope);
                            }*/


                    }
                    else
                    {
                        throw new System.Exception($"parameter of type {parameterId}");
                    }

                    break;
                default:
                    throw new System.Exception($"User interaction not supported {operationId} ");
            }
        }

        protected virtual void OnChange(ParameterEventContent<(string, string)> responseContainer)
        {
            //RequestHttpUploadDto httpDto = new RequestHttpUploadDto();
            var request = new UploadFileRequest(responseContainer.value.Item2) { users = new HashSet<UMI3DUser>() { responseContainer.user } };
            uploadTokens.Add(request.token, this);

            request.ToTransaction(true).Dispatch();
        }

        /// <summary>
        /// Remove a token from the list of valide tokens.
        /// </summary>
        /// <param name="token">Token to remove.</param>
        public static void RemoveToken(string token)
        {
            if (uploadTokens.ContainsKey(token))
                uploadTokens.Remove(token);
            else
                UMI3DLogger.LogWarning("this token : " + token + " is not a valide token", scope);
        }

        /// <summary>
        /// Called when a file is received.
        /// </summary>
        /// <param name="token">Token for upload.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="bytes">File as a byte array.</param>
        public virtual void OnFileReceive(string token, string fileName, byte[] bytes)
        {
            if (!Directory.Exists(pathToSaveFile))
            {
                var dir = new DirectoryInfo(pathToSaveFile);
                dir.Create();
            }
            string path = inetum.unityUtils.Path.Combine(pathToSaveFile, fileName);
            if (File.Exists(path))
            {
                //path += "_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString();
                path = inetum.unityUtils.Path.Combine(pathToSaveFile, System.IO.Path.GetFileNameWithoutExtension(path) + "_" + DateTime.Now.ToShortDateString().Replace(@"/", "-") + "_" + DateTime.Now.ToLongTimeString().Replace(':', '-') + System.IO.Path.GetExtension(path));

            }
            File.WriteAllBytes(path, bytes);
            onSave.Invoke(path);

        }

        private void Start()
        {
            onChange.AddListener(OnChange);
            onReceive.AddListener(OnFileReceive);
        }
    }
}