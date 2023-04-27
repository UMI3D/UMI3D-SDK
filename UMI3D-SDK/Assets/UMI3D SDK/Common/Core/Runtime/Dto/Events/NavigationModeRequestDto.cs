/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.common
{

    public abstract class NavigationModeRequestDto : AbstractOperationDto
    {
        public NavigationModeRequestDto() : base() { }
    }

    public class LockedModeRequestDto : NavigationModeRequestDto
    {
        public LockedModeRequestDto() : base() { }
    }

    public class FPSModeRequestDto : FlyingModeRequestDto
    {
        public float jumpHeigth { get; set; }
        public float fishingRoadMaxDistance { get; set; }

        public FPSModeRequestDto() : base() { }
    }

    public class FlyingModeRequestDto : NavigationModeRequestDto
    {
        public speedDto walking { get; set; }
        public speedDto crouching { get; set; }
        public speedDto running { get; set; }
        public speedDto runningCrouching { get; set; }

        public FlyingModeRequestDto() : base() { }
    }

    public class FlyingLayeredModeRequestDto : FlyingModeRequestDto
    {
        public float upDownSpeed { get; set; }

        public FlyingLayeredModeRequestDto() : base() { }
    }

    public class speedDto
    {
        public float forwardSpeed { get; set; }
        public float backwardSpeed { get; set; }
        public float sideSpeed { get; set; }
    }
}