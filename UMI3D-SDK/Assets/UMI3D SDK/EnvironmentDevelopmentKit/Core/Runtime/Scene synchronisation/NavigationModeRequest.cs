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

using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Request relative to vehicle boarding.
    /// </summary>
    public abstract class NavigationModeRequest : Operation
    {
        public NavigationModeRequest()
        {
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            NavigationModeRequestDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract uint GetOperationKey();

        protected abstract NavigationModeRequestDto CreateDto();
        protected abstract void WriteProperties(NavigationModeRequestDto dto);

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey());
        }
    }

    public class lockedNavigationModeRequest : NavigationModeRequest
    {
        public lockedNavigationModeRequest() : base()
        {
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.LockedNavigationMode;
        }

        protected override NavigationModeRequestDto CreateDto() => new LockedModeRequestDto();
        protected override void WriteProperties(NavigationModeRequestDto dto)
        {

        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return base.ToBytable(user);
        }
    }

    public abstract class FpsNavigationModeRequest : FlyingNavigationModeRequest
    {
        public float jumpHeigth { get; set; }
        public float fishingRoadMaxDistance { get; set; }

        public FpsNavigationModeRequest() : base()
        {
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.FpsNavigationMode;
        }

        protected override NavigationModeRequestDto CreateDto() => new FPSModeRequestDto();
        protected override void WriteProperties(NavigationModeRequestDto dto)
        {
            base.WriteProperties(dto);
            if(dto is FPSModeRequestDto requestDto)
            {
                requestDto.fishingRoadMaxDistance = fishingRoadMaxDistance;
                requestDto.jumpHeigth = jumpHeigth;
            }
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return base.ToBytable(user)
                + UMI3DSerializer.Write(fishingRoadMaxDistance)
                + UMI3DSerializer.Write(jumpHeigth);
        }
    }

    public abstract class FlyingNavigationModeRequest : NavigationModeRequest
    {

        public SpeedDto walking { get; set; }
        public SpeedDto crouching { get; set; }
        public SpeedDto running { get; set; }
        public SpeedDto runningCrouching { get; set; }

        public FlyingNavigationModeRequest() : base()
        {
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.FlyingNavigationMode;
        }

        protected override NavigationModeRequestDto CreateDto() => new FlyingModeRequestDto();
        protected override void WriteProperties(NavigationModeRequestDto dto)
        {
            if (dto is FlyingModeRequestDto requestDto)
            {
                requestDto.walking = walking;
                requestDto.crouching = crouching;
                requestDto.running = running;
                requestDto.runningCrouching = runningCrouching;
            }
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return base.ToBytable(user)
                + UMI3DSerializer.Write(walking)
                + UMI3DSerializer.Write(crouching)
                + UMI3DSerializer.Write(running)
                + UMI3DSerializer.Write(runningCrouching);
        }
    }


    public abstract class LayeredFlyingNavigationModeRequest : FlyingNavigationModeRequest
    {
        public float upDownSpeed { get; set; }

        public LayeredFlyingNavigationModeRequest() : base()
        {
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.LayeredFlyingNavigationMode;
        }

        protected override NavigationModeRequestDto CreateDto() => new FlyingLayeredModeRequestDto();
        protected override void WriteProperties(NavigationModeRequestDto dto)
        {
            base.WriteProperties(dto);
            if (dto is FlyingLayeredModeRequestDto requestDto)
            {
                requestDto.upDownSpeed = upDownSpeed;
            }
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return base.ToBytable(user)
                + UMI3DSerializer.Write(upDownSpeed);
        }
    }
}