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
    public class UMI3DVideoPlayer : UMI3DAbstractAnimation
    {

        new public static UMI3DVideoPlayer Get(string id) { return UMI3DAbstractAnimation.Get(id) as UMI3DVideoPlayer; }

        public UMI3DVideoPlayer(UMI3DVideoPlayerDto dto) : base(dto)
        {

        }

        ///<inheritdoc/>
        public override float GetProgress()
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public override void Start()
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public override void Stop()
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public override void Start(float atTime)
        {
            throw new NotImplementedException();
        }
    }
}