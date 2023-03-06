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

using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// <see cref="UMI3DUser"/> with a UMI3D Avatar attached to it.
    /// </summary>
    public class UMI3DTrackedUser : UMI3DUser
    {
        /// <summary>
        /// User's avatar
        /// </summary>

        public UserTrackingFrameDto CurrentTrackingFrame;


        //To Delete
        //private UMI3DAvatarNode avatar;

        ///// <summary>
        ///// User's avatar
        ///// </summary>
        //public UMI3DAvatarNode Avatar
        //{
        //    get => avatar;
        //    set
        //    {
        //        if (avatar == value)
        //            return;
        //        if (avatar != null)
        //            GameObject.Destroy(avatar.gameObject);
        //        if (value != null)
        //            value.userId = Id();
        //        avatar = value;
        //        UMI3DServer.Instance.NotifyUserChanged(this);
        //    }
        //}
    }
}