/*
Copyright 2022 Inetum

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

using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.collaboration;

namespace umi3d.worldController
{
    public abstract class StandAloneWorldControllerAPI : WorldControllerAPI, IWorldController_Client, IWorldController
    {
        public virtual string worldName { get; set; }
        public virtual string ip { get; set; }

        public umi3d.edk.UMI3DResource Icon2D;
        public umi3d.edk.UMI3DResource Icon3D;

        public override void Setup()
        {
            worldName = UMI3DCollaborationEnvironment.Instance.environmentName;
            this.ip = edk.UMI3DServer.GetHttpUrl();
            edk.collaboration.UMI3DHttp.Instance.AddRoot(new worldController.UMI3DStandAloneApi(this));
        }

        public abstract Task<UMI3DDto> Connect(ConnectionDto connectionDto);
        public virtual Task<MediaDto> GetMediaDto()
        {
            return Task.FromResult(ToDto());
        }

        public virtual MediaDto ToDto()
        {
            var res = new MediaDto
            {
                name = worldName,
                url = ip,

                versionMajor = UMI3DVersion.major,
                versionMinor = UMI3DVersion.minor,
                versionStatus = UMI3DVersion.status,
                versionDate = UMI3DVersion.date,

                icon2D = Icon2D.ToDto(),
                icon3D = Icon3D.ToDto()
            };

            return res;
        }

        public abstract Task<PrivateIdentityDto> RenewCredential(PrivateIdentityDto identityDto);

    }
}