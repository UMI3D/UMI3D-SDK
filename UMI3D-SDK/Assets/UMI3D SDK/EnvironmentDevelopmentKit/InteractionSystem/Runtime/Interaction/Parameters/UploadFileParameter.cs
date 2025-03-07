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
using System.Linq;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// File parameter that could be uploaded to the environment server.
    /// </summary>
    public class UploadFileParameter : AbstractParameter
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Interaction;

        public static Dictionary<string, UploadFileParameter> uploadTokens { get; private set; } = new();

        [Header("Upload configurations")]
        [Tooltip("The path where the file will be saved on the server.")]
        public string pathToSaveFile = "./TMP/";

        [Tooltip("Extensions allowed. Empty means all extensions allowed")]
        /// <summary>
        /// Only these extensions could be upload by the client. Empty list = allow all.<br/>
        /// <br/>
        /// You are free to add a dot (.) at the beginning of the extensions. All the extension comparaison have to be made with <see cref="extensionToLowerWithoutDot"/> and <see cref="GetFormattedExtension(string)"/>.
        /// </summary>
        public List<string> authorizedExtensions = new List<string>();
        List<string> extensionToLowerWithoutDot
        {
            get
            {
                return authorizedExtensions
                    .Select(ext => GetFormattedExtension(ext))
                    .ToList();
            }
        }

        [Space]
        /// <summary>
        /// The file path on client.
        /// </summary>
        public string filePathOnClient;

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
            return new UploadFileParameterDto() { authorizedExtensions = extensionToLowerWithoutDot };
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

                    if (settingRequestDto.parameter is not UploadFileParameterDto parameter)
                    {
                        throw new Exception($"parameter of type {settingRequestDto.parameter.GetType()}");
                    }

                    if (!TryGetExtension(filePathOnClient, out string ext)) { return; }

                    if (!MatchExtension(ext)) { return; }

                    filePathOnClient = parameter.value;

                    onChange.Invoke(new(
                            user,
                            settingRequestDto,
                            (filePathOnClient, settingRequestDto.fileId)
                        )
                    );
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(
            UMI3DUser user,
            ulong operationId,
            ulong toolId,
            ulong interactionId,
            ulong hoverredId,
            uint boneType,
            Vector3Dto bonePosition,
            Vector4Dto boneRotation,
            ByteContainer container
        )
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.UploadFileRequest:

                    uint parameterId = UMI3DSerializer.Read<uint>(container);
                    if (UMI3DParameterKeys.StringUploadFile != parameterId)
                    {
                        throw new Exception($"parameter of type {parameterId}");
                    }

                    UMI3DSerializer.Read<bool>(container);

                    // Get the path of the file.
                    string path = UMI3DSerializer.Read<string>(container);
                    // Get the authorized extensions.
                    List<string> extensions = UMI3DSerializer.ReadList<string>(container);
                    // Get the file extension.
                    string ext2 = UMI3DSerializer.Read<string>(container);
                    // Get the fileID.
                    string fileId = UMI3DSerializer.Read<string>(container);

                    UnityEngine.Debug.Log($"{path}, {ext2}, {fileId}");

                    if (!TryGetExtension(path, out string ext)) { return; }

                    if (!MatchExtension(ext)) { return; }

                    this.filePathOnClient = path;

                    onChange.Invoke(new(
                            user,
                            toolId,
                            interactionId,
                            hoverredId,
                            boneType,
                            bonePosition,
                            boneRotation,
                            (filePathOnClient, fileId)
                        )
                    );
                    break;
                default:
                    throw new System.Exception($"User interaction not supported {operationId} ");
            }
        }

        /// <summary>
        /// Remove a token from the list of valide tokens.
        /// </summary>
        /// <param name="token">Token to remove.</param>
        public static void RemoveToken(string token)
        {
            if (!uploadTokens.ContainsKey(token))
            {
                UMI3DLogger.LogWarning("this token : " + token + " is not a valide token", scope);
                return;
            }

            uploadTokens.Remove(token);
        }

        protected virtual void OnChange(ParameterEventContent<(string, string)> responseContainer)
        {
            UnityEngine.Debug.Log($"OnChange upload");
            UploadFileRequest request = new(responseContainer.value.Item2)
            {
                // Add the user who triggered the event to the request's users HashSet
                users = new HashSet<UMI3DUser>() { responseContainer.user }
            };

            // Add the request's token and this object to the uploadTokens dictionary
            uploadTokens.Add(request.token, this);

            request.ToTransaction(true).Dispatch();
        }

        /// <summary>
        /// Called when a file is received.
        /// </summary>
        /// <param name="token">Token for upload.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="bytes">File as a byte array.</param>
        public virtual void OnFileReceive(string token, string fileName, byte[] bytes)
        {
            // Question: Token ?


            // Check if the directory to save the file exists.
            if (!Directory.Exists(pathToSaveFile))
            {
                // If it doesn't exist, create a new directory.
                var dir = new DirectoryInfo(pathToSaveFile);
                dir.Create();
            }

            // Combine the path to save the file with the file name to create a full path.
            string path = inetum.unityUtils.Path.Combine(pathToSaveFile, fileName);

            // If a file with the same name already exists, append the current date and time to the file name.
            if (File.Exists(path))
            {
                string pathWithoutExtension = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);
                DateTime now = DateTime.Now;
                fileName = $"{pathWithoutExtension}_{now.ToShortDateString().Replace(@"/", "-")}_{now.ToLongTimeString().Replace(':', '-')}{extension}";

                path = inetum.unityUtils.Path.Combine(pathToSaveFile, fileName);
            }

            // Write the byte array to the specified path
            File.WriteAllBytes(path, bytes);

            onSave.Invoke(path);

        }

        /// <summary>
        /// Try to get the extension <paramref name="ext"/> of the file located at <paramref name="path"/>.<br/>
        /// <br/>
        /// If the path is valide the extension will contains a dot (.) has the first character.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        bool TryGetExtension(string path, out string ext)
        {
            ext = Path.GetExtension(path);

            if (string.IsNullOrEmpty(ext))
            {
                UMI3DLogger.LogWarning($"No extension found in path {path}", scope);
                return false;
            }

            return true;
        }

        public bool MatchExtension(string ext)
        {
            ext = GetFormattedExtension(ext);

            if (authorizedExtensions.Count != 0 && !extensionToLowerWithoutDot.Contains(ext))
            {
                UMI3DLogger.LogWarning("Unauthorized extension : " + ext, scope);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return the extension without the dot (.) at the beginning and in lower case.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        string GetFormattedExtension(string extension)
        {
            // Remove the dot (.) at the beginning of the extension.
            if (extension.StartsWith('.'))
            {
                extension = extension.Substring(1);
            }

            // Get the lower case extension.
            extension = extension.ToLowerInvariant();

            return extension;
        }

        private void Start()
        {
            onChange.AddListener(OnChange);
            onReceive.AddListener(OnFileReceive);
        }
    }
}