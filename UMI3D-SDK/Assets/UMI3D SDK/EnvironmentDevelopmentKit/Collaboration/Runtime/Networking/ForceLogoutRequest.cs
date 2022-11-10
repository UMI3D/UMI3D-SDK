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

using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk;
using UnityEngine;

/// <summary>
/// Request to prevent the user that they are being forced logged out.
/// </summary>
public class ForceLogoutRequest : DispatchableRequest
{
    /// <summary>
    /// Explanation for the forced log out.
    /// </summary>
    string reason;

    public ForceLogoutRequest(string reason, bool reliable, HashSet<UMI3DUser> users) : base(reliable, users)
    {
        this.reason = reason;
    }

    protected virtual Bytable ToBytable()
    {
        return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.ForceLogoutRequest)
            + UMI3DNetworkingHelper.Write(reason);
    }

    /// <inheritdoc/>
    public override byte[] ToBytes()
    {
        return ToBytable().ToBytes();
    }

    /// <inheritdoc/>
    public override byte[] ToBson()
    {
        ForceLogoutDto dto = CreateDto();
        WriteProperties(dto);
        return dto.ToBson();
    }

    protected virtual ForceLogoutDto CreateDto() { return new ForceLogoutDto(); }
    protected virtual void WriteProperties(ForceLogoutDto dto) { dto.reason = reason; }
}
