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

using System.Collections.Generic;

namespace umi3d.edk
{
    /// <summary>
    /// Interface for collaborative room, managing the relayed streams.
    /// </summary>
    public interface ICollaborationRoom : UMI3DEntity
    {
        /// <summary>
        /// Control the relay for the data channel.
        /// </summary>
        /// <param name="sender">Node associated to the request.</param>
        /// <param name="userSender">User sending data.</param>
        /// <param name="data">Data that is relayed to other users.</param>
        /// <param name="target">User receiving data.</param>
        /// <param name="receiverSetting">Who should receive the data stream.</param>
        /// <param name="isReliable">Is the transaction reliable?</param>
        List<UMI3DUser> RelayDataRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false);

        /// <summary>
        /// Control the relay for the user tracking channel.
        /// </summary>
        /// <param name="sender">Node associated to the request.</param>
        /// <param name="userSender">User sending data.</param>
        /// <param name="data">Data that is relayed to other users.</param>
        /// <param name="target">User receiving data.</param>
        /// <param name="receiverSetting">Who should receive the data stream.</param>
        /// <param name="isReliable">Is the transaction reliable?</param>
        List<UMI3DUser> RelayTrackingRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false);

        /// <summary>
        /// Control the relay for the voice over IP channel.
        /// </summary>
        /// <param name="sender">Node associated to the request.</param>
        /// <param name="userSender">User sending data.</param>
        /// <param name="data">Data that is relayed to other users.</param>
        /// <param name="target">User receiving data.</param>
        /// <param name="receiverSetting">Who should receive the data stream.</param>
        /// <param name="isReliable">Is the transaction reliable?</param>
        List<UMI3DUser> RelayVoIPRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false);

        /// <summary>
        /// Control the relay for the video channel.
        /// </summary>
        /// <param name="sender">Node associated to the request.</param>
        /// <param name="userSender">User sending data.</param>
        /// <param name="data">Data that is relayed to other users.</param>
        /// <param name="target">User receiving data.</param>
        /// <param name="receiverSetting">Who should receive the data stream.</param>
        /// <param name="isReliable">Is the transaction reliable?</param>
        List<UMI3DUser> RelayVideoRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false);
    }
}
