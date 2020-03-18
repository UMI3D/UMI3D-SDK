/*
Copyright 2019 Gfi Informatique

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
using System.IO;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{

    [System.Serializable]
    public class CVEResource : IHasAsyncProperties
    {
        protected bool inited = false;
        protected bool updated = false;
        protected List<UMI3DUser> updatedFor = new List<UMI3DUser>();

        protected List<IHasAsyncProperties> lstListeners = new List<IHasAsyncProperties>();
        
        public string Domain = "";
        public string Path = "";
        public string ApiKey = "";
        public string Login = "";
        public string Password = "";
        public string RequestType = "";
        public bool IsLocalFile = false;
        
        public UMI3DAsyncProperty<string> objectLogin;
        public UMI3DAsyncProperty<string> objectPassword;

        public void initDefinition()
        {
            if (objectLogin == null)
            {
                objectLogin = new UMI3DAsyncProperty<string>(this, Login);
                objectLogin.OnValueChanged += (string value) => Login = value;

                objectPassword = new UMI3DAsyncProperty<string>(this, Password);
                objectPassword.OnValueChanged += (string value) => Password = value;
            }
        }

        public void SyncProperties()
        {
            if (objectLogin == null)
                initDefinition();

            objectLogin.SetValue(Login);
            objectPassword.SetValue(Password);
            NotifyUpdate();
        }

        public string GetOsFolder(string path, UMI3DUser user = null)
        {
            if (path != null && path != "")
            {
                OSQualitycollection.OSQualityFolder bestQualities = null;
                UMI3D.Scene.OSQualitycollection.DefaultQuality.path = UMI3D.Scene.OSQualitycollection.DefaultQuality.path.Replace(@"\", "/");
                if (user != null && UMI3D.Exist && user.os != null && user.os != "")
                {
                    string basePath = UMI3D.GetResourceRoot();
                    foreach (OSQualitycollection.OSQualityFolder qualities in UMI3D.Scene.OSQualitycollection.OSQualities)
                    {
                        qualities.path = qualities.path.Replace(@"\", "/");
                        if (qualities.os == user.os && qualities.quality == user.quality)
                        {
                            if (File.Exists(System.IO.Path.GetFullPath(basePath + qualities.path + path)))
                            {
                                bestQualities = qualities;
                            }
                        }
                    }
                }
                string basepath = ((bestQualities != null) ? bestQualities.path : UMI3D.Scene.OSQualitycollection.DefaultQuality.path);
                if (basepath != null && basepath != "")
                {
                    return umi3d.Path.Combine(basepath + path);
                }
            }
            return path;
        }

        public string GetUrl(UMI3DUser user = null)
        {
            Path = Path.Replace(@"\", "/");
            if ( Path != null && Path != "" && !(Path.StartsWith("/") /*|| Path.StartsWith(@"\")*/))
            {
                Path = "/" + Path;
            }
            return umi3d.Path.Combine(((IsLocalFile && UMI3D.
                Server.
                GetEnvironmentUrl() != null) ? UMI3D.Server.GetEnvironmentUrl() : Domain) , GetOsFolder(Path,user));
        }

        public ResourceDto ToDto(UMI3DUser user)
        {
            var dto = new ResourceDto();

            dto.Url = GetUrl(user);
            dto.ApiKey = ApiKey;
            dto.Login = objectLogin.GetValue(user);
            dto.Password = objectPassword.GetValue(user);
            dto.RequestType = RequestType;

            return dto;
        }

        public ResourceDto ToDto()
        {
            var dto = new ResourceDto();
            dto.Url = GetUrl();
            dto.ApiKey = ApiKey;
            dto.Login = Login;
            dto.Password = Password;
            dto.RequestType = RequestType;
            return dto;
        }

        public void addListener(IHasAsyncProperties listener)
        {
            if (!lstListeners.Contains(listener))
                lstListeners.Add(listener);
        }

        public void removeListener(IHasAsyncProperties listener)
        {
            if (lstListeners.Contains(listener))
                lstListeners.Remove(listener);
        }

        public void NotifyUpdate()
        {
            updated = true;
            foreach (var listener in lstListeners)
            {
                listener.NotifyUpdate();
            }
        }

        public void NotifyUpdate(UMI3DUser u)
        {
            if (!updatedFor.Contains(u))
                updatedFor.Add(u);

            foreach (var listener in lstListeners)
            {
                listener.NotifyUpdate(u);
            }
        }
    }
}