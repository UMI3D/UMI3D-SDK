/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License; Version 2.0 (the );
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing; software
distributed under the License is distributed on an  BASIS;
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND; either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
namespace umi3d.cdk.notification
{
    public static class UMI3DClientNotificatonKeys
    {
        public const string CameraPropertiesNotification = "UMI3DClientNotificatonKeys_CameraProperties";
    
        public static class Info
        {
            /// <summary>
            /// New Camera Properties.<br/>
            /// Value is <see cref="umi3d.common.PerspectiveCameraPropertiesDto"/> or <see cref="umi3d.common.OrthographicCameraPropertiesDto"/>.<br/>
            /// <br/>
            /// See Notification key: <see cref="CameraPropertiesNotification"/>
            /// </summary>
            public const string CameraProperties = "UMI3DClientNotificatonKeys_CameraPropertiesValue";
        }
    }
}


