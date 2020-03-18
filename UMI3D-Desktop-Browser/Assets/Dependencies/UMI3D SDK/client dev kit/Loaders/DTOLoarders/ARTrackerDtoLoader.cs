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
using System;
using umi3d.common;

namespace umi3d.cdk
{
    public class ARTrackerDtoLoader : AbstractDTOLoader<ARTrackerDto, ARTrackerObject>
    {
        /// <summary>
        /// Create an ARTrackerObject from an ARTrackerDto and raise a given callback.
        /// </summary>
        /// <param name="dto">ARTrackerDto to load</param>
        /// <param name="callback">Callback to raise (the argument is the ARTrackerObject)</param>
        public override void LoadDTO(ARTrackerDto dto, Action<ARTrackerObject> callback, Action<string> onError)
        {
            var entity = GetComponent<ARTrackerObject>() ?? gameObject.AddComponent<ARTrackerObject>();
            UpdateFromDTO(entity, null, dto);
            callback(entity);
        }

        /// <summary>
        /// Update an ARTrackerObject from dto.
        /// </summary>
        /// <param name="go">ARTrackerObject to update</param>
        /// <param name="olddto">Previous dto describing the ARTrackerObject</param>
        /// <param name="newdto">Dto to update the ARTrackerObject to</param>
        public override void UpdateFromDTO(ARTrackerObject entity, ARTrackerDto olddto, ARTrackerDto newdto)
        {
            entity.TrackerId = newdto.TrackerId;
        }
    }
}