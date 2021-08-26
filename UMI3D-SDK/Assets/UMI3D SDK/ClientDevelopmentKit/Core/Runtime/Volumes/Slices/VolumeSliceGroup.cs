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
using UnityEngine;
using umi3d.common.volume;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Group of volume slices, constitue a volume cell.
    /// </summary>
	public class VolumeSliceGroup : AbstractVolumeCell
	{
        private ulong id;
        private List<VolumeSlice> slices = new List<VolumeSlice>();

        public void Setup(VolumeSlicesGroupDto dto)
        {
            id = dto.id;
            slices = VolumeSliceGroupManager.Instance.GetVolumeSlices();
        }

        public override ulong Id() => id;
        
        public override bool IsInside(Vector3 point)
        {
            foreach(VolumeSlice s in slices)
            {
                if (s.isInside(point))
                    return true;
            }
            return false;
        }

        public void SetSlices(List<VolumeSliceDto> newSlices)
        {
            slices = newSlices.ConvertAll(dto => VolumeSliceGroupManager.Instance.GetVolumeSlice(dto.id));
        }

        public VolumeSlice[] GetSlices() => slices.ToArray();
	}
}