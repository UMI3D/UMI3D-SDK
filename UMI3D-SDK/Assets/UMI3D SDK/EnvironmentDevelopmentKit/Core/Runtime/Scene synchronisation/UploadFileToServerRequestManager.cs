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
using inetum.unityUtils;
using System;
using System.Collections.Generic;
using umi3d.common;

namespace umi3d.edk
{
    public class UploadFileToServerRequestManager : Singleton<UploadFileToServerRequestManager>
    {
        ulong availableId = 0;
        Dictionary<ulong, UploadFileToServerRequest> map = new();

        public static event Action<ulong, UploadFileToServerRequest, UMI3DUser, FileUploadProgressStatusRequestDto> OnStatusUpdate;

        public UploadFileToServerRequestManager() : base()
        {
            UMI3DServer.Instance.OnServerStopped += Instance_OnServerStopped;
        }

        private void Instance_OnServerStopped()
        {
            availableId = 0;
            map = new();
        }

        public ulong Register(UploadFileToServerRequest request)
        {
            var id = availableId++;
            map.Add(id, request);
            return id;
        }

        public void Notify(UMI3DUser user, FileUploadProgressStatusRequestDto update)
        {
            if (map.TryGetValue(update.requestId, out UploadFileToServerRequest request))
            {
                OnStatusUpdate?.Invoke(update.requestId, request,user, update);
                request.Notify(user, update);
            }
        }

        public void Notify(UMI3DUser user, ByteContainer request)
        {
            ulong requestId = UMI3DSerializer.Read<ulong>(request);
            float progress = UMI3DSerializer.Read<float>(request);
            string fileName = UMI3DSerializer.Read<string>(request);
            int fileSize = UMI3DSerializer.Read<int>(request);
            string status = UMI3DSerializer.Read<string>(request);
            bool succeeded = UMI3DSerializer.Read<bool>(request);
            bool completed = UMI3DSerializer.Read<bool>(request);

            Notify(user, new FileUploadProgressStatusRequestDto() 
            { 
                requestId = requestId,
                progress = progress,
                fileName = fileName,
                fileSize = fileSize,
                status = status,
                succeeded = succeeded,
                completed = completed
            });
        }
    }
}